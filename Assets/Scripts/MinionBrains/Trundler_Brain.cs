using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Trundler_Brain : Brain
{
    // This AI should pick a spot, aim at it, then accelerate towards it.
    // Once it is close enough, pick a new spot and repeat.
    // Every few moments, it should scan for the player.  If the player is found, it becomes the nav target.
    // When the player is within boresight and firing range, the enemy opens fire.
    // If the player navigates outside of the enemy's scan range, it breaks lock and goes back to a random patrol.

    //init

    //blaster param
    float randomSpread = 10f;
    float boresight = 30f;
    float shotSpeed = 10f;
    float shotLifetime = 0.3f;
    float timeBetweenShots = 0.6f;
    bool isFiring = false;
    float blasterDamage = 0.3f;

    //hood
    float timeSinceLastShot = 0f;
    float attackRange;



    protected override void Start()
    {
        base.Start();
        currentDest = ab.CreateValidRandomPointWithinArena();
        attackRange = shotLifetime * shotSpeed;
    }

    protected override void Update()
    {
        if (isServer)
        {
            base.Update();
            EvaluateTarget();
            AdjustColorIfPursuingTarget();
            AttackTarget();
            UpdateRandomDestination();
        }
    }


    private void EvaluateTarget()
    {
        if(targets.Count > 0)
        {

            currentAttackTarget = targets[0];
            currentDest = currentAttackTarget.transform.position;
        }
        else
        {
            currentAttackTarget = null;
        }
    }
    private void AdjustColorIfPursuingTarget()
    {
        if (currentAttackTarget)
        {
            sr.color = Color.red;
        }
        if (!currentAttackTarget)
        {
            sr.color = Color.white;
        }
    }

    private void AttackTarget()
    {
        timeSinceLastShot += Time.deltaTime;
        if (timeSinceLastShot < timeBetweenShots) { return; }
        if (currentAttackTarget && distToAttackTarget < attackRange && angleToAttackTarget < boresight)
        {
            GameObject newBlasterProjectile = Instantiate(weaponPrefab, transform.position, transform.rotation) as GameObject;
            newBlasterProjectile.layer = 11;
            NetworkServer.Spawn(newBlasterProjectile);
            uint idToSim = newBlasterProjectile.GetComponent<NetworkIdentity>().netId;
            RpcMakeBulletSimulatedOnClientSide(idToSim);
            newBlasterProjectile.transform.Rotate(new Vector3(0, 0, UnityEngine.Random.Range(-randomSpread, randomSpread)));
            newBlasterProjectile.GetComponent<Rigidbody2D>().velocity = (shotSpeed) * newBlasterProjectile.transform.up;
            DamageDealer damageDealer = newBlasterProjectile.GetComponent<DamageDealer>();
            damageDealer.IsReal = true;
            damageDealer.SetDamage(blasterDamage);
            //SelectRandomFiringSound();
            //AudioSource.PlayClipAtPoint(selectedBlasterSound, gameObject.transform.position);
            Destroy(newBlasterProjectile, shotLifetime);
            timeSinceLastShot = 0;
        }
    }

    [ClientRpc]
    private void RpcMakeBulletSimulatedOnClientSide(uint bulletNetID)
    {
        if (!isClientOnly) { return; }
        NetworkIdentity bulletNI;
        NetworkIdentity.spawned.TryGetValue(bulletNetID, out bulletNI);
        GameObject bulletToSim;
        if (bulletNI)
        {
            bulletToSim = bulletNI.gameObject;
            bulletToSim.layer = 0;
        }
    }

    


    private void UpdateRandomDestination()
    {
        if (!currentAttackTarget)
        {
            if (distToDest < closeEnough)
            {
                currentDest = ab.CreateValidRandomPointWithinArena();
            }
        }
    }


    protected override void FixedUpdate()
    {
        TurnToFaceDestination();
        FlyTowardsDestination();
        Debug.DrawLine(transform.position, currentDest, Color.blue);
    }
}
