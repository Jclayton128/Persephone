using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class WreckerDroneBrain : NetworkBehaviour
{
    //init
    public GameObject RepairTarget { get; set; }
    public GameObject Persephone { get; set; }
    Health targetHealth;
    Rigidbody2D rb;

    //param
    [SerializeField] float thrust;
    [SerializeField] float turnSpeed;
    float repairRange = 0.3f;
    float timeBetweenRepairTicks = 1.0f;
    float angleThresholdForAccel = 30f;

    //hood
    float timeForNextRepairTick = 0;
    float distToNavTarget;
    float degToNavTarget;
    Vector3 navTarget;
    bool hasMadeInitialContact = false;
    float closeEnough = 0.05f;
    bool isDoneRepairing = false;




    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GetComponent<Health>().EntityIsDying += SignalDeathToPersephone;

    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            DecideNavigation();
            RepairPlayer();
        }

    }

    private void DecideNavigation()
    {
        Vector3 dirToNavTarget = (navTarget - transform.position);
        degToNavTarget = Vector3.SignedAngle(dirToNavTarget, transform.up, Vector3.forward);
        distToNavTarget = dirToNavTarget.magnitude;

        if (!isDoneRepairing)
        {
            if (!hasMadeInitialContact)
            {
                navTarget = RepairTarget.transform.position;
                if (distToNavTarget <= closeEnough)
                {
                    hasMadeInitialContact = true;
                }
            }

            if (hasMadeInitialContact && distToNavTarget <= closeEnough)
            {
                navTarget = CUR.CreateRandomPointNearInputPoint(RepairTarget.transform.position, repairRange, 0);
            }
        }
        else
        {
            navTarget = Persephone.transform.position;
            AdjustSpriteRendererForReturnLeg();

            if (distToNavTarget < closeEnough)
            {
                Persephone.GetComponent<PersephoneBrain>().RecoverWreckerDrone(gameObject);
            }
        }

    }

    private void AdjustSpriteRendererForReturnLeg()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sortingLayerName = "Nonplayers";
        int perSL = Persephone.GetComponent<SpriteRenderer>().sortingOrder;
        sr.sortingOrder = perSL - 1; //Force the little drone to be under Pers.
    }

    private void RepairPlayer()
    {
        if (distToNavTarget <= repairRange && Time.time >= timeForNextRepairTick && !isDoneRepairing)
        {
            float quintileHealth = targetHealth.GetMaxHull() / 5f;
            targetHealth.ModifyHullLevel(quintileHealth, false);
            timeForNextRepairTick = Time.time + timeBetweenRepairTicks;
        }
        if (targetHealth.GetCurrentHull() >= targetHealth.GetMaxHull())
        {
            isDoneRepairing = true;
        }
    }

    private void FixedUpdate()
    {
        FaceTarget();
        MoveToTarget();
    }
    private void FaceTarget()
    {
        float factor = Mathf.Abs(degToNavTarget) / 5f;
        factor = Mathf.Clamp01(factor);
        if (degToNavTarget > 0)
        {
            rb.angularVelocity = -1 * turnSpeed * factor;
        }
        if (degToNavTarget < 0)
        {
            rb.angularVelocity = turnSpeed * factor;
        }
    }
    private void MoveToTarget()
    {
        if (Mathf.Abs(degToNavTarget) <= angleThresholdForAccel)
        {
            float distThresh = closeEnough;
            float factor = Mathf.Clamp01(distToNavTarget / 0.2f);
            rb.AddForce(thrust * transform.up * Time.deltaTime * factor);
        }

    }

    public void SetRepairTarget(GameObject target)
    {
        RepairTarget = target;
        targetHealth = RepairTarget.GetComponent<Health>();
    }

    private void SignalDeathToPersephone()
    {
        Debug.Log("Persephone, I'm dying!");
        Persephone.GetComponent<PersephoneBrain>().HandleDestroyedWreckerDrone(this);
    }


}
