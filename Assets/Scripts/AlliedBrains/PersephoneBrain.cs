using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using System;

public class PersephoneBrain : NetworkBehaviour
{
    //init
    [SerializeField] GameObject wreckerDronePrefab = null;
    Slider healthSlider;
    TextMeshProUGUI statusTMP;
    SpriteRenderer[] srs;
    Rigidbody2D rb;
    LevelManager lm;
    PersephoneHealth ph;
    Turret_AI[] turrets;

    //param
    float TimeRequiredToWarpIn;
    public static Vector3 startingSpot = new Vector3(-45, 0, 0);
    string persCountdownText = "Arrival in: ";

    float speed_WarpingIn = 30f;
    float speed_InSystem = 1f;
    float turnRate = 45f;
    float minTravelDist = 16f;
    float closeEnoughDist = 3f;
    float timeRequiredForWarpChargeUp = 10f;

    float repairCost = 10f;

    //hood

    [SyncVar(hook = nameof(UpdateStatusUI))]
    string statusText;

    bool isStarted = false;

    [SyncVar]
    bool isInArena = true;

    float speed_Current;
    Vector3 positionOfWarpPortal;
    float distToWarpPortal;
    float timeLeftForWarpCharging = 5;

    bool isRepairingPlayers = false;
    [SerializeField] List<GameObject> wreckingDronesInUse = new List<GameObject>();
    [SerializeField] List<GameObject> disabledPlayers = new List<GameObject>();


    private void Awake()
    {
        RegisterPrefabs();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lm = FindObjectOfType<LevelManager>();
        ph = GetComponent<PersephoneHealth>();
        turrets = GetComponentsInChildren<Turret_AI>();

        if (isClient)
        {
            UIManager uim = FindObjectOfType<UIManager>();
            healthSlider = uim.GetPersephoneHealthSlider();
            statusTMP = uim.GetPersephoneStatusTMP();
            srs = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    private void RegisterPrefabs()
    {
        NetworkClient.RegisterPrefab(wreckerDronePrefab);

    }

    // Update is called once per frame
    void Update()
    {
        if (isClient)
        {
            HandleVisibility();
        }

        if (isServer && isStarted)
        {
            HandleArrivalTimer();
            FaceWarpPortal();
            distToWarpPortal = (transform.position - positionOfWarpPortal).magnitude;
            MoveTowardsWarpPortal();
            ChargeWarpEngineIfClosedEnough();
        }

    }
    private void HandleVisibility()
    {
        if (!isInArena)
        {
            healthSlider.gameObject.SetActive(false);
            foreach(SpriteRenderer sr in srs)
            {
                sr.enabled = false;
            }

        }
        else
        {
            healthSlider.gameObject.SetActive(true);
            foreach (SpriteRenderer sr in srs)
            {
                sr.enabled = true;
            }
        }
    }

    private void HandleArrivalTimer()
    {
        if (!isInArena)
        {
            TimeRequiredToWarpIn -= Time.deltaTime;
            int roundedCountdown = Mathf.RoundToInt(TimeRequiredToWarpIn);
            statusText = persCountdownText + roundedCountdown.ToString();
            if (TimeRequiredToWarpIn <= 0)
            {
                WarpIn();
            }
        }
    }

    private void WarpIn()
    {

        isInArena = true;

        // Flash the background white, like a phase bomb
        // TODO play an warp-in sound
        // Update StatusText
        // Locate Position of WarpPortal;
        timeLeftForWarpCharging = timeRequiredForWarpChargeUp;
        gameObject.layer = 0;
        speed_Current = speed_WarpingIn;
        statusText = "In Transit";
    }

    private void FaceWarpPortal()
    {
        Vector3 facingDir = (positionOfWarpPortal - transform.position);
        Quaternion targetRot = Quaternion.LookRotation(Vector3.forward, facingDir);
        Quaternion rot = Quaternion.RotateTowards(transform.rotation, targetRot, turnRate * Time.deltaTime);
        transform.rotation = rot;
    }

    private void MoveTowardsWarpPortal()
    {
        if (!isInArena) { return; }
        HandleInSystemActions();
        rb.velocity = transform.up * speed_Current;
    }

    private void HandleInSystemActions()
    {
        if (distToWarpPortal <= minTravelDist)
        {
            gameObject.layer = 8;
            float factor = Mathf.Clamp01(distToWarpPortal / closeEnoughDist);
            speed_Current = speed_InSystem * factor;
            foreach (Turret_AI turret in turrets)
            {
                turret.enabled = true;
            }
            if (disabledPlayers.Count > 0 && wreckingDronesInUse.Count == 0)
            {
                FixUpDisabledPlayers();
            }
        }      
    }
    private void ChargeWarpEngineIfClosedEnough()
    {
        if (distToWarpPortal < closeEnoughDist  && timeLeftForWarpCharging > 0)
        {
            timeLeftForWarpCharging -= Time.deltaTime;
            int round = Mathf.RoundToInt(timeLeftForWarpCharging);
            statusText = "Time To Warp: " + round;
        }
        if (distToWarpPortal < closeEnoughDist && timeLeftForWarpCharging <= 0 && wreckingDronesInUse.Count > 0)
        {
            statusText = "Waiting On Drones";
        }


        if (timeLeftForWarpCharging <= 0 && isInArena && wreckingDronesInUse.Count == 0)
        {
            WarpOut();
        }
    }

    private void WarpOut()
    {
        transform.position = startingSpot;
        lm.AdvanceToNextLevel();
        gameObject.layer = 0;
        foreach (Turret_AI turret in turrets)
        {
            turret.ResetTurret();
            turret.enabled = false;
        }
        isInArena = false;

    }

    #region Player Repair

    [Server]
    public void AddDisabledPlayer(GameObject player)
    {
        disabledPlayers.Add(player);
        if (isInArena)
        {
            FixUpSpecificPlayer(player);
            //FixUpDisabledPlayers();
        }
    }

    [Server]
    public void RemoveDisabledPlayer(GameObject player)
    {
        disabledPlayers.Remove(player);
    }

    private void FixUpDisabledPlayers()
    {
        foreach(GameObject player in disabledPlayers)
        {
            Health health = player.GetComponent<Health>();
            if (health.AssignedWreckerDrone) { return; }
            SpawnWreckerDrone(player, health);
        }
    }
    private void FixUpSpecificPlayer(GameObject player)
    {
        Health health = player.GetComponent<Health>();
        if (health.AssignedWreckerDrone) { return; }
        SpawnWreckerDrone(player, health);
    }
    private void SpawnWreckerDrone(GameObject player, Health health)
    {
        if (ph.CheckPayPlayerRepairCost(repairCost))
        {
            GameObject newDrone = Instantiate(wreckerDronePrefab, transform.position, transform.rotation) as GameObject;
            WreckerDroneBrain wreckerDroneBrain = newDrone.GetComponent<WreckerDroneBrain>();
            wreckerDroneBrain.SetRepairTarget(player);
            wreckerDroneBrain.Persephone = gameObject;

            health.AssignedWreckerDrone = newDrone;
            wreckingDronesInUse.Add(newDrone);
            NetworkServer.Spawn(newDrone);
        }
    }

    public void RecoverWreckerDrone(GameObject drone)  // a Drone calls this once it is close enough to Persphone and done repairing.
    {
        wreckingDronesInUse.Remove(drone);
        Destroy(drone);
    }

    public void HandleDestroyedWreckerDrone(WreckerDroneBrain droneWDB)
    {
        Debug.Log("send another wrecker drone, i'm dead!");
        GameObject playerInNeed = droneWDB.RepairTarget;
        FixUpSpecificPlayer(playerInNeed);
        wreckingDronesInUse.Remove(droneWDB.gameObject);

    }



    #endregion


    #region Public Methods
    public void StartPersephone()
    {
        isStarted = true;
    }

    public void SetTimerUponLevelStart(float timeUntilPersephoneArrival)
    {
        TimeRequiredToWarpIn = timeUntilPersephoneArrival;
        isInArena = false;
    }

    #endregion

    #region UI
    private void UpdateStatusUI(string v1, string v2)
    {
        statusTMP.text = statusText;
    }

    #endregion
}
