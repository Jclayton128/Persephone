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
    bool isFiring = false;


    //hood
    float timeSinceLastShot = 0f;
    float attackRange;



    public override void OnStartServer()
    {
        base.OnStartServer();
        currentDest = ab.CreateValidRandomPointWithinArena();
        attackRange = weaponLifetime * weaponSpeed;
    }

    protected override void Update()
    {
        if (isServer)
        {
            base.Update();
            SelectBestTarget();
            AdjustColorIfPursuingTarget();
            AttackTarget();
            UpdateRandomDestination();
        }
    }


    private void EvaluateTarget()
    {

        if(targets.Count > 0)
        {

            if (targets[0].GetCurrentImportance() <= 0)
            {
                targets.RemoveAt(0);
            }


            currentAttackTarget = targets[0].gameObject;
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
        if (currentAttackTarget && distToAttackTarget < attackRange && angleToAttackTarget < boresightThreshold)
        {
            GameObject newBlasterProjectile = Instantiate(weaponPrefab, muz.PrimaryMuzzle.position, muz.PrimaryMuzzle.rotation) as GameObject;
            newBlasterProjectile.layer = 11;
            NetworkServer.Spawn(newBlasterProjectile);
            uint idToSim = newBlasterProjectile.GetComponent<NetworkIdentity>().netId;
            RpcMakeBulletSimulatedOnClientSide(idToSim);
            newBlasterProjectile.transform.Rotate(new Vector3(0, 0, UnityEngine.Random.Range(-randomSpread, randomSpread)));
            newBlasterProjectile.GetComponent<Rigidbody2D>().velocity = (weaponSpeed) * newBlasterProjectile.transform.up;
            DamageDealer damageDealer = newBlasterProjectile.GetComponent<DamageDealer>();
            //damageDealer.IsReal = true;
            damageDealer.SetNormalDamage(weaponNormalDamage);
            damageDealer.SetIonization(1f);
            damageDealer.SetDraining(1f);
            //SelectRandomFiringSound();
            //AudioSource.PlayClipAtPoint(selectedBlasterSound, gameObject.transform.position);
            Destroy(newBlasterProjectile, weaponLifetime);
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
        if (isServer)
        {
            TurnToFaceDestination(FaceMode.simple);
            MoveTowardsNavTarget();
            Debug.DrawLine(transform.position, currentDest, Color.blue);
        }
    }
}
