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
    [SerializeField] Slider healthSlider;
    [SerializeField] TextMeshProUGUI statusTMP;
    Rigidbody2D rb;

    //param
    float TimeRequiredToWarpIn;
    [SerializeField] Vector3 startingSpot;
    string persCountdownText = "Arrival in: ";

    float speed_WarpingIn = 30f;
    float speed_InSystem = 3f;
    float turnRate = 45f;
    float minTravelDist = 16f;
    float closeEnoughDist = 3f;

    //hood

    [SyncVar(hook = nameof(UpdateHealthUI))]
    float currentHealth;

    [SyncVar(hook = nameof(UpdateStatusUI))]
    string statusText;

    bool isStarted = false;
    [SerializeField] bool isInArena = false;
    float speed_Current;
    Vector3 positionOfWarpPortal;
    float distToWarpPortal;


    void Start()
    {
        if (isClient)
        {
            UIManager uim = FindObjectOfType<UIManager>();
            healthSlider = uim.GetPersephoneHealthSlider();
            statusTMP = uim.GetPersephoneStatusTMP();
            rb = GetComponent<Rigidbody2D>();

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isClient)
        {
            HandleHealthBarVisibility();
        }

        if (isServer && isStarted)
        {
            HandleArrivalTimer();
            FaceWarpPortal();
            MoveTowardsWarpPortal();
        }

    }

    private void HandleHealthBarVisibility()
    {
        if (!isInArena)
        {
            healthSlider.gameObject.SetActive(false);
        }
        else
        {
            healthSlider.gameObject.SetActive(true);
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
        // Insta-rotate to face locationOfWarpPortal
        speed_Current = speed_WarpingIn;
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
        distToWarpPortal = (transform.position - positionOfWarpPortal).magnitude;

        if (distToWarpPortal <= minTravelDist)
        {
            float factor = Mathf.Clamp01(distToWarpPortal / closeEnoughDist);
            speed_Current = speed_InSystem * factor;
        }

        Debug.Log($"current speed: {speed_Current} at distance: {distToWarpPortal}");
        
    }


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
    private void UpdateHealthUI(float v1, float v2)
    {
        healthSlider.value = currentHealth;
    }

    private void UpdateStatusUI(string v1, string v2)
    {
        statusTMP.text = statusText;
    }

    #endregion
}
