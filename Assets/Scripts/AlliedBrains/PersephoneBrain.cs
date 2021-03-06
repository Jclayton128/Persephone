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
    Image compass;
    SpriteRenderer[] srs;
    Rigidbody2D rb;
    LevelManager lm;
    PersephoneHealth ph;
    Turret_AI[] turrets;
    ArenaBounds ab;
    GameObject localPlayerAvatar;

    //param
    float TimeRequiredToWarpIn;
    Vector3 startingSpot;
    string persCountdownText = "Arrival in: ";

    float speed_WarpingIn = 30f;
    float speed_InSystem = 1f;
    float turnRate = 45f;
    float minTravelDist;
    float closeEnoughDist = 3f;
    float timeRequiredForWarpChargeUp = 10f;

    float repairCost = 10f;

    //hood

    [SyncVar(hook = nameof(UpdateStatusUI))]
    string statusText;

    bool isStarted = false;

    [SyncVar]
    bool isInArena = false;

    [SyncVar]
    [SerializeField] float speed_Current;

    Vector3 positionOfWarpPortal;
    Vector2 directionToWarpPortal;
    float distToWarpPortal;
    float timeLeftForWarpCharging = 5;

    bool isRepairingPlayers = false;
    [SerializeField] List<GameObject> wreckingDronesInUse = new List<GameObject>();
    [SerializeField] List<GameObject> disabledPlayers = new List<GameObject>();


    bool debugHalt = false;

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
        ab = FindObjectOfType<ArenaBounds>();

        if (isServer)
        {
            startingSpot = ab.CreateValidRandomPointOutsideOfArena();
            transform.position = startingSpot;
            minTravelDist = ab.ArenaRadius * 0.75f;
        }


        if (isClient)
        {
            UIManager uim = FindObjectOfType<UIManager>();
            healthSlider = uim.GetPersephoneHealthSlider();
            statusTMP = uim.GetPersephoneStatusTMP();
            compass = uim.GetPersephoneCompass();
            srs = GetComponentsInChildren<SpriteRenderer>();
            localPlayerAvatar = ClientInstance.ReturnClientInstance().CurrentAvatar;

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
            HandleVisibilityOnClient();
            //OrientCompass();
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

    private void OrientCompass()
    {
        Vector3 compassDir = (transform.position - localPlayerAvatar.transform.position);
        float ang = Vector3.SignedAngle(Vector3.up, compassDir, Vector3.forward);
        Quaternion rot = Quaternion.Euler(0, 0, ang);
        compass.transform.rotation = rot;
    }

    private void HandleVisibilityOnClient()
    {
        if (!isInArena)
        {
            healthSlider.gameObject.SetActive(false);
            compass.gameObject.SetActive(false);
            foreach(SpriteRenderer sr in srs)
            {
                sr.enabled = false;
            }

        }
        else
        {
            healthSlider.gameObject.SetActive(true);
            compass.gameObject.SetActive(true);
            foreach (SpriteRenderer sr in srs)
            {
                sr.enabled = true;
            }
        }
    }

    private void HandleArrivalTimer()
    {
        if (!isInArena && !debugHalt)
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
        startingSpot = ab.CreateValidRandomPointOutsideOfArena();
        gameObject.layer = 0;
        speed_Current = speed_WarpingIn;
        statusText = "In Transit";
    }

    private void FaceWarpPortal()
    {
        directionToWarpPortal = (positionOfWarpPortal - transform.position);
        Quaternion targetRot = Quaternion.LookRotation(Vector3.forward, directionToWarpPortal);
        Quaternion rot = Quaternion.RotateTowards(transform.rotation, targetRot, turnRate * Time.deltaTime);
        transform.rotation = rot;
    }

    private void MoveTowardsWarpPortal()
    {
        if (!isInArena) { return; }
        HandleInSystemActions();

        rb.velocity = transform.up * speed_Current * Convert.ToInt32(!debugHalt) ;
    }

    private void HandleInSystemActions()
    {
        if (distToWarpPortal <= minTravelDist)
        {
            gameObject.layer = 8;
            float distanceFactor = Mathf.Clamp01(distToWarpPortal / closeEnoughDist);
            float angleToPortal = Vector2.SignedAngle(transform.up, directionToWarpPortal);
            float angleFactor = 1 - Mathf.Clamp01((angleToPortal / 45f));

            speed_Current = speed_InSystem * distanceFactor * angleFactor;
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
        if (distToWarpPortal < closeEnoughDist  && timeLeftForWarpCharging > 0 && !debugHalt)
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

    [Server]
    public void DebugToggleMovementOnOff()
    {
        debugHalt = !debugHalt;
    }

    #endregion

    #region UI
    private void UpdateStatusUI(string v1, string v2)
    {
        statusTMP.text = statusText;
    }

    #endregion
}
