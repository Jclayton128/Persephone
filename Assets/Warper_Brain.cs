using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Warper_Brain : Brain
{
    //param
    float minDistanceToTeleport = 3.5f;
    float timeBetweenWarps = 5.0f;

    float timeBetweenAttacks = 2.0f;

    int playerWeaponLayerMask = 1 << 9;


    //hood
    bool needToWarp = false;
    bool readyToWarp = true;
    float timeUntilCanWarp;
    float timeUntilNextAttack;

    public override void OnStartServer()
    {
        base.OnStartServer();
        currentDest = ab.CreateRandomPointWithinArena(transform.position, minDistanceToTeleport, ArenaBounds.DestinationMode.noCloserThan);

    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (isServer)
        {
            if (distToDest <= closeEnough)
            {
                currentDest = ab.CreateRandomPointWithinArena(transform.position, minDistanceToTeleport, ArenaBounds.DestinationMode.noCloserThan);
            }
            SelectBestTarget();
            AttackPlayerWithMissiles();
            HandleWarping();
        }
    }

    private void TargetMostImportantTarget()
    {
        if (targets.Count > 0 )
        {
            currentAttackTarget = targets[0].gameObject;
        }

    }

    protected override void Scan()
    {
        base.Scan();
        ScanForIncomingDamageDealers();
    }

    private void ScanForIncomingDamageDealers()
    {
        RaycastHit2D[] colls = Physics2D.CircleCastAll(transform.position, detectorRange, transform.up, 0.0f, playerWeaponLayerMask);
        foreach (RaycastHit2D coll in colls)
        {
            if (coll.transform.gameObject.GetComponent<DamageDealer>() == true)
            {
                needToWarp = true;
                return;
            }
        }
    }

    private void HandleWarping()
    {
        if (!readyToWarp)
        {
            timeUntilCanWarp -= Time.deltaTime;
        }
        if (timeUntilCanWarp <= 0)
        {
            readyToWarp = true;
        }
        if (readyToWarp && needToWarp)
        {
            WarpNow();
        }
    }

    private void AttackPlayerWithMissiles()
    {
        timeUntilNextAttack -= Time.deltaTime;
        if (currentAttackTarget && timeUntilNextAttack <= 0 )
        {
            GameObject missile = Instantiate(weaponPrefab, transform.position, transform.rotation) as GameObject;
            missile.layer = 11;
            DamageDealer dd = missile.GetComponent<DamageDealer>();
            dd.SetNormalDamage(weaponNormalDamage);
            dd.SetShieldBonusDamage(weaponShieldBonusDamage);
            dd.SetIonization(weaponIonization);
            dd.SetKnockback(weaponKnockback);
            dd.SetSpeedModifier(weaponSpeedMod);
            Missile_AI hm = missile.GetComponent<Missile_AI>();
            hm.SetLifetime(weaponLifetime);
            hm.SetMissileOwner(gameObject);
            hm.SetMissileTarget(currentAttackTarget);
            hm.hasReachedTarget = true;
            hm.normalSpeed = weaponSpeed;
            hm.maxTurnRate = weaponTurnRate;
            timeUntilNextAttack = timeBetweenAttacks;
            NetworkServer.Spawn(missile);
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (isServer)
        {
            TurnToFaceDestination(FaceMode.simple);
            MoveTowardsNavTarget(true);
        }
    }

    public override void WarnOfIncomingDamageDealer(GameObject damager)
    {
        base.WarnOfIncomingDamageDealer(damager);
        needToWarp = true;
    }

    private void WarpNow()
    {
        Vector3 teleportationSite = ab.CreateRandomPointWithinArena(transform.position, minDistanceToTeleport, ArenaBounds.DestinationMode.noCloserThan);
        transform.position = teleportationSite;
        needToWarp = false;
        readyToWarp = false;
        timeUntilCanWarp = timeBetweenWarps;
    }
}
