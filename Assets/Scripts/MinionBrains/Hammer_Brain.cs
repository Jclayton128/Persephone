using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Hammer_Brain : Brain
{
    //init
    [SerializeField] GameObject damageBallPrefab = null;
    [SerializeField] Transform weaponEmitterPoint = null;
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
    float distanceToPlayer = 10f;
    float angleToPlayer = 0f;
    float timeSinceBeganCharging = 0f;
    bool isSprinting = false;
    float timeSinceBeganSprinting = 0f;

    [SyncVar(hook = nameof(UpdateDamageBallImageOnClient))]
    float damageBallChargeFactor = 0f;

    private void Awake()
    {
        NetworkClient.RegisterPrefab(damageBallPrefab);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        TargetMostImportantAlly();
        timeSinceBeganCharging = 0 + UnityEngine.Random.Range(-1 * randomVarianceToChargeUpTime, randomVarianceToChargeUpTime);
    }

    private void TargetMostImportantAlly()
    {
        if (targets.Count != 0)
        {
            currentAttackTarget = targets[0].gameObject;
        }
    }

    protected override void Update()
    {
        if (isServer)
        {
            TrackPlayer();
            CreateDamageBall();
            TargetMostImportantAlly();
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
    private void CreateDamageBall()
    {
        if (!damageBall && !isSprinting && timeSinceBeganCharging < randomVarianceToChargeUpTime)
        {
            Debug.Log("made a new damage ball");
            damageBall = Instantiate(damageBallPrefab, weaponEmitterPoint.position, weaponEmitterPoint.rotation) as GameObject; //weaponEmitterPoint.transform.position, weaponEmitterPoint.transform.rotation) as GameObject;
            damageBall.layer = 11;  //11 means that the hammer won't hurt other enemy units
            dbdd = damageBall.GetComponent<DamageDealer>();
            dbdd.SetRegularDamage(0);
            NetworkServer.Spawn(damageBall);
            //damageBall.transform.parent = gameObject.transform;  // I think having a child with a netidentity is bad.
            dbsr = damageBall.GetComponent<SpriteRenderer>();
            damageBallChargeFactor = 0;
            dbsr.color = new Color(1, 1, 1, damageBallChargeFactor);
            
        }
        if (damageBall)
        {
            damageBall.transform.position = weaponEmitterPoint.position;
            damageBallChargeFactor = (timeSinceBeganCharging / timeRequiredToChargeMotors);
            dbdd.SetRegularDamage(damageBallChargeFactor * damageBallMaxDamage);

        }

    }

    private void ChargeMotorsWhileFacingPlayer()
    {
        if (!currentAttackTarget) { return; }
        Vector2 targetDir = currentAttackTarget.transform.position - transform.position;
        angleToPlayer = Vector3.SignedAngle(targetDir, transform.up, transform.forward);
        if (angleToPlayer > 5)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, -maxTurnSpeed_normal, turnAccelRate_normal * Time.deltaTime);
        }
        if (angleToPlayer < -5)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, maxTurnSpeed_normal, turnAccelRate_normal * Time.deltaTime);
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

    private void TrackPlayer()
    {
        if (!currentAttackTarget) { return; }
        distanceToPlayer = (currentAttackTarget.transform.position - transform.position).sqrMagnitude;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Destroy(damageBall);
    }

}
