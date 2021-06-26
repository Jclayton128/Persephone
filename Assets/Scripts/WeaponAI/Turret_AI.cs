using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Turret_AI : MonoBehaviour
{
    //init
    [SerializeField] GameObject weaponPrefab = null;

    //param
    [SerializeField] float weaponSpeed;
    [SerializeField] float weaponLifetime;
    [SerializeField] float timeBetweenShots;
    [SerializeField] float weaponDamage;
    [SerializeField] float weaponBonusShieldDamage;
    [SerializeField] float weaponIonDamage;
    [SerializeField] float weaponKnockback;

    [SerializeField] float turretTurnRate;
    [SerializeField] bool leadsForVelocity = false;
    [SerializeField] float randomSpread;
    [SerializeField] float boresightTolerance;

    [SerializeField] int weaponPhysicsLayer;
    [SerializeField] int primaryLayerToTarget;
    [SerializeField] int secondaryLayerToTarget;


    //hood
    [SerializeField] GameObject target;
    Rigidbody2D targetRB;
    float range;
    float nextFireTime;
    float angleToTargetFromBoresight;
    float distToTarget;

    bool isServer;


    private void Awake()
    {
        NetworkClient.RegisterPrefab(weaponPrefab);
    }

    void Start()
    {
        range = (weaponLifetime * weaponSpeed);
        isServer = GetComponentInParent<PersephoneBrain>().isServer;
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            TargetClosestThreat();
            FaceTarget();
            FireAtTarget();
        }
    }

    private void TargetClosestThreat()
    {
        if (target)
        {
            distToTarget = (target.transform.position - transform.position).magnitude;
            if ( distToTarget > range * 1.2f)
            {
                target = null;
            }
            else
            {
                return;
            }
        }
        int layerMask = (1 << primaryLayerToTarget) | (1 << secondaryLayerToTarget);
        target = CUR.GetNearestGameObjectOnLayer(transform, layerMask, range);
        targetRB = target?.GetComponent<Rigidbody2D>();
        
    }

    private void FaceTarget()
    {
        if (!target) { return; }
        Vector2 targetDir;
        if (leadsForVelocity)
        {
            Vector3 enemyVel = targetRB.velocity;
            float timeOfShot = ((target.transform.position + enemyVel) - transform.position).magnitude / weaponSpeed;
            Vector3 leadPos = target.transform.position + (enemyVel * timeOfShot);
            targetDir = leadPos - transform.position;
            Debug.DrawLine(transform.position, leadPos, Color.blue, timeBetweenShots);
        }
        else
        {
            targetDir = target.transform.position - transform.position;
            Debug.DrawLine(transform.position, target.transform.position, Color.cyan, timeBetweenShots);
        }

        angleToTargetFromBoresight = Vector3.SignedAngle(targetDir, transform.up, transform.forward);

        float angleToTargetFromNorth = Vector3.SignedAngle(targetDir, Vector2.up, transform.forward);
        Quaternion angleToPoint = Quaternion.Euler(0, 0, -1 * angleToTargetFromNorth);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, angleToPoint, turretTurnRate * Time.deltaTime);
    }

    private void FireAtTarget()
    {
        if (!target) { return; }
        if (Time.time >= nextFireTime && Mathf.Abs(angleToTargetFromBoresight) <= boresightTolerance && distToTarget <= range)
        {
            GameObject bullet = Instantiate(weaponPrefab, transform.position, transform.rotation) as GameObject;
            bullet.layer = weaponPhysicsLayer; 

            bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * weaponSpeed;

            float randSpread = UnityEngine.Random.Range(-randomSpread, randomSpread);
            bullet.transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + randomSpread);

            DamageDealer dd = bullet.GetComponent<DamageDealer>();
            //dd.IsReal = true;
            dd.SetKnockback(weaponKnockback);
            dd.SetNormalDamage(weaponDamage);
            dd.SetShieldBonusDamage(weaponBonusShieldDamage);
            dd.SetIonization(weaponIonDamage);

            NetworkServer.Spawn(bullet);

            Destroy(bullet, weaponLifetime);

            nextFireTime = Time.time + timeBetweenShots;
        }

    }


    public void ResetTurret()
    {
        target = null;
        nextFireTime = Time.time;
        transform.rotation = transform.parent.rotation;
    }
}
