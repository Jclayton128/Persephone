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

    //param
    float TimeRequiredToWarpIn;
    [SerializeField] Vector3 startingSpot;
    string persCountdownText = "Arrival in: ";

    float speed_WarpingIn = 30f;
    float speed_InSystem = 1f;
    float turnRate = 45f;
    float minTravelDist = 16f;
    float closeEnoughDist = 3f;
    float timeRequiredForWarpChargeUp = 10f;
    float drainAmount = 0;  // Slows Pers' warp engine charge time, which increases her vulnerability
    float ionizationAmount = 0; // Slows Pers' move speed, which increases her vulnerability

    //hood

    [SyncVar(hook = nameof(UpdateStatusUI))]
    string statusText;

    bool isStarted = false;
    [SerializeField] bool isInArena = false;
    float speed_Current;
    Vector3 positionOfWarpPortal;
    float distToWarpPortal;
    float timeLeftForWarpCharging = 99;

    bool isRepairingPlayers = false;
    List<GameObject> wreckingDronesInUse = new List<GameObject>();
    List<GameObject> disabledPlayers = new List<GameObject>();


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lm = FindObjectOfType<LevelManager>();
        RegisterPrefabs();


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
        AdjustSpeedBasedOnDistanceToWarpPortal();
        rb.velocity = transform.up * speed_Current;
    }

    private void AdjustSpeedBasedOnDistanceToWarpPortal()
    {
        if (distToWarpPortal <= minTravelDist)
        {
            gameObject.layer = 10; // 8;
            float factor = Mathf.Clamp01(distToWarpPortal / closeEnoughDist);
            speed_Current = speed_InSystem * factor;
        }      
    }
    private void ChargeWarpEngineIfClosedEnough()
    {
        if (distToWarpPortal < closeEnoughDist)
        {
            timeLeftForWarpCharging -= Time.deltaTime;
            int round = Mathf.RoundToInt(timeLeftForWarpCharging);
            statusText = "Time To Warp: " + round;
        }
        if (timeLeftForWarpCharging <= 0 && isInArena)
        {
            WarpOut();
        }
    }

    private void WarpOut()
    {
        transform.position = startingSpot;
        lm.AdvanceToNextLevel();
        gameObject.layer = 0;
        isInArena = false;

    }

    #region Player Repairs

    [Server]
    public void AddDisabledPlayer(GameObject player)
    {
        disabledPlayers.Add(player);
        float repairCost = player.GetComponent<Health>().GetMaxHull();
        FixUpDisabledPlayer(player);

    }

    [Server]
    public void RemoveDisabledPlayer(GameObject player)
    {
        disabledPlayers.Remove(player);
    }

    private void FixUpDisabledPlayer(GameObject player)
    {
        GameObject newDrone = Instantiate(wreckerDronePrefab, transform.position, transform.rotation) as GameObject;
        newDrone.GetComponent<WreckerDroneBrain>().SetRepairTarget(player);
        NetworkServer.Spawn(newDrone);
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
