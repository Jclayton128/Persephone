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
    float timeUntilNextShot = 0f;

    public override void OnStartServer()
    {
        base.OnStartServer();
        currentDest = ab.CreateValidRandomPointWithinArena();
 
    }

    protected override void Update()
    {
        base.Update();
        if (isServer)
        { 
            AttackBehaviour();
        }
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (isServer)
        {
            TurnToFaceDestination(faceMode);
            MoveTowardsNavTarget();
            Debug.DrawLine(transform.position, currentDest, Color.blue);
        }
    }

    private void AttackBehaviour()
    {
        timeUntilNextShot -= Time.deltaTime;
        if (timeUntilNextShot > 0) { return; }
        if (currentAttackTarget && distToAttackTarget < attackRange && angleToAttackTarget < boresightThreshold)
        {
            GameObject newBlasterProjectile = Instantiate(weaponPrefab, muz.PrimaryMuzzle.position, muz.PrimaryMuzzle.rotation) as GameObject;
            newBlasterProjectile.layer = 11;
            newBlasterProjectile.transform.Rotate(new Vector3(0, 0, UnityEngine.Random.Range(-randomSpread, randomSpread)));
            newBlasterProjectile.GetComponent<Rigidbody2D>().velocity = (weaponSpeed) * newBlasterProjectile.transform.up;
            DamageDealer damageDealer = newBlasterProjectile.GetComponent<DamageDealer>();
            damageDealer.SetNormalDamage(weaponNormalDamage);
            damageDealer.SetIonization(weaponIonization);
            //SelectRandomFiringSound();
            //AudioSource.PlayClipAtPoint(selectedBlasterSound, gameObject.transform.position);
            NetworkServer.Spawn(newBlasterProjectile);
            Destroy(newBlasterProjectile, weaponLifetime);
            timeUntilNextShot = intervalBetweenWeapons;
        }
    }


}
