using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Hammer_Brain : Brain
{
    //init
    GameObject damageBall;
    DamageDealer dbdd;
    SpriteRenderer dbsr;

    //ship param

    float angleOffForFullThrust = 10f;
    float timeRequiredToChargeMotors = 7f;
    float sprintingAngularDrag = 100f;
    float chargingAngularDrag = 0.01f;
    float sprintDuration = 5f;
    float randomVarianceToChargeUpTime = 2.0f;
    float damageBallMaxDamage = 10;

    //hood
    float timeSinceBeganCharging = 0f;
    bool isSprinting = false;
    float timeSinceBeganSprinting = 0f;

    [SyncVar(hook = nameof(UpdateDamageBallImageOnClient))]
    float damageBallChargeFactor = 0f;

    public override void OnStartServer()
    {
        base.OnStartServer();
        timeSinceBeganCharging = 0 + UnityEngine.Random.Range(-1 * randomVarianceToChargeUpTime, randomVarianceToChargeUpTime);
    }


    protected override void Update()
    {
        base.Update();
        if (isServer)
        {
            CreateDamageBall();
        }

    }
    private void CreateDamageBall()
    {
        if (!damageBall && !isSprinting && timeSinceBeganCharging < randomVarianceToChargeUpTime)
        {
            damageBall = Instantiate(weaponPrefab, muz.PrimaryMuzzle.position, muz.PrimaryMuzzle.rotation) as GameObject; //weaponEmitterPoint.transform.position, weaponEmitterPoint.transform.rotation) as GameObject;
            damageBall.layer = 11;  //11 means that the hammer won't hurt other enemy units
            dbdd = damageBall.GetComponent<DamageDealer>();
            dbdd.SetNormalDamage(weaponNormalDamage);
            NetworkServer.Spawn(damageBall);
            //damageBall.transform.parent = gameObject.transform;  // I think having a child with a netidentity is bad.
            dbsr = damageBall.GetComponent<SpriteRenderer>();
            damageBallChargeFactor = 0;
            dbsr.color = new Color(1, 1, 1, damageBallChargeFactor);

        }
        if (damageBall)
        {
            damageBall.transform.position = muz.PrimaryMuzzle.position;
            damageBallChargeFactor = (timeSinceBeganCharging / timeRequiredToChargeMotors);
            dbdd.SetNormalDamage(damageBallChargeFactor * damageBallMaxDamage);

        }

    }
    private void UpdateDamageBallImageOnClient(float v1, float v2)
    {
        if (damageBall && dbsr)
        {
            dbsr.color = new Color(1, 1, 1, damageBallChargeFactor);
            damageBall.GetComponent<Rigidbody2D>().angularVelocity = damageBallChargeFactor * 720f;
        }
    }


    protected override void FixedUpdate()
    {
        if (isServer)
        {
            SprintTowardsPlayer();
            ChargeMotorsWhileFacingPlayer();            
        }

    }


    private void ChargeMotorsWhileFacingPlayer()
    {
        if (!currentAttackTarget) { return; }
        if (angleToAttackTarget > 5)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, -maxTurnSpeed_normal, turnAccelRate_normal * Time.deltaTime);
        }
        if (angleToAttackTarget < -5)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, maxTurnSpeed_normal, turnAccelRate_normal * Time.deltaTime);
        }

        if (timeSinceBeganCharging < timeRequiredToChargeMotors)
        {
            timeSinceBeganCharging += Time.deltaTime;
        }
        if (timeSinceBeganCharging >= timeRequiredToChargeMotors && !isSprinting) //Charged up and not already sprinting: begin sprinting!
        {
            rb.angularDrag = sprintingAngularDrag;
            isSprinting = true;
        }

    }

    private void SprintTowardsPlayer()
    {
        if (isSprinting)
        {
            timeSinceBeganSprinting += Time.deltaTime;
            rb.AddForce(accelRate_normal * transform.up * Time.timeScale);
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


    protected override void OnDestroy()
    {
        base.OnDestroy();
        Destroy(damageBall);
    }

}
