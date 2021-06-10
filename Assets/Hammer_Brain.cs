﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Hammer_Brain : Brain
{
    //init
    Rigidbody2D rb;
    [SerializeField] GameObject damageBallPrefab = null;
    [SerializeField] Transform weaponEmitterPoint = null;
    GameObject damageBall;
    SpriteRenderer dbsr;
    GameObject targetPlayer;

    //ship param
    float thrustForward = 20.0f;
    float thrustTurning = 10f;
    float maxTurnRate = 180f;
    float angleOffForFullThrust = 10f;
    float timeRequiredToChargeMotors = 7f;
    float sprintingAngularDrag = 100f;
    float chargingAngularDrag = 0.01f;
    float sprintDuration = 1.5f;
    float randomVarianceToChargeUpTime = 2.0f;
    float damageBallDamage = 10;

    //hood
    float distanceToPlayer = 10f;
    float angleToPlayer = 0f;
    float timeSinceBeganCharging = 0f;
    bool isSprinting = false;
    float timeSinceBeganSprinting = 0f;
    float chargeFractionRemaining = 0f;

    void Start()
    {
        if (!isServer) { return; }
        AcquirePlayerTarget();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        timeSinceBeganCharging = 0 + UnityEngine.Random.Range(-1 * randomVarianceToChargeUpTime, randomVarianceToChargeUpTime);
    }

    private void AcquirePlayerTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        int rand = UnityEngine.Random.Range(0, players.Length);
        targetPlayer = players[rand];
    }

    private void FixedUpdate()
    {
        if (!isServer) { return; }
        SprintTowardsPlayer();
        ChargeMotorsWhileFacingPlayer();
    }
    void Update()
    {
        if (!isServer) { return; }
        TrackPlayer();
        CreateDamageBall();
        if (!targetPlayer)
        {
            AcquirePlayerTarget();
        }
    }


    private void CreateDamageBall()
    {
        if (!damageBall)
        {
            damageBall = Instantiate(damageBallPrefab, weaponEmitterPoint.position, weaponEmitterPoint.rotation) as GameObject; //weaponEmitterPoint.transform.position, weaponEmitterPoint.transform.rotation) as GameObject;
            damageBall.layer = 11;  //11 means that the hammer won't hurt other enemy units
            NetworkServer.Spawn(damageBall);
            //damageBall.transform.parent = gameObject.transform;  // I think having a child with a netidentity is bad.
            damageBall.GetComponent<DamageDealer>().SetDamage(0.0f);
            dbsr = damageBall.GetComponent<SpriteRenderer>();
            dbsr.color = new Color(1, 1, 1, 0);
        }
        if (damageBall)
        {
            damageBall.transform.position = weaponEmitterPoint.position;
            chargeFractionRemaining = (timeSinceBeganCharging / timeRequiredToChargeMotors);
            //damageBall.transform.position = weaponEmitterPoint.transform.position;
            if (!isSprinting)
            {
                dbsr.color = new Color(1, 1, 1, chargeFractionRemaining);
                damageBall.GetComponent<Rigidbody2D>().angularVelocity = chargeFractionRemaining * 720f;
                if (chargeFractionRemaining >= 0.8)
                {
                    damageBall.GetComponent<DamageDealer>().SetDamage(damageBallDamage);
                    //damageBall.GetComponent<CircleCollider2D>().enabled = true;
                }
            }
            if (isSprinting)
            {
                damageBall.GetComponent<Rigidbody2D>().angularVelocity = 0f;
            }
        }

    }

    private void ChargeMotorsWhileFacingPlayer()
    {
        if (!targetPlayer) { return; }
        Vector2 targetDir = targetPlayer.transform.position - transform.position;
        angleToPlayer = Vector3.SignedAngle(targetDir, transform.up, transform.forward);
        if (angleToPlayer > 5)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, -maxTurnRate, thrustTurning * Time.deltaTime);
        }
        if (angleToPlayer < -5)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, maxTurnRate, thrustTurning * Time.deltaTime);
        }

        if (timeSinceBeganCharging < timeRequiredToChargeMotors)
        {
            timeSinceBeganCharging += Time.deltaTime;
        }
        if (timeSinceBeganCharging >= timeRequiredToChargeMotors && !isSprinting) //Charged up and not already sprinting: begin sprinting!
        {
            //sr.color = Color.red;
            rb.angularDrag = sprintingAngularDrag;
            isSprinting = true;
        }

    }

    private void SprintTowardsPlayer()
    {
        if (isSprinting)
        {
            timeSinceBeganSprinting += Time.deltaTime;
            rb.AddForce(thrustForward * transform.up * Time.timeScale);
            if (timeSinceBeganSprinting >= sprintDuration) //Once sprinting duration is done: decolor, decrease angular drag,
            {
                //sr.color = Color.white;
                rb.angularDrag = chargingAngularDrag;
                timeSinceBeganSprinting = 0f;
                timeSinceBeganCharging = 0 + UnityEngine.Random.Range(-1 * randomVarianceToChargeUpTime, randomVarianceToChargeUpTime);
                isSprinting = false;
            }
        }
    }

    private void TrackPlayer()
    {
        if (!targetPlayer) { return; }
        distanceToPlayer = (targetPlayer.transform.position - transform.position).sqrMagnitude;
    }

    private void OnDestroy()
    {
        Destroy(damageBall);
    }

}
