using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Maker_Brain : Brain
{
    //This AI should trundle slowly away from the player.  It emits smaller ships called mites.
    //These mites initially start to orbit the maker until the limit is reached.
    //Anytime the player is within attack range, the mites should stop orbiting the maker ship and then begin to fly towards the player.
    //Once the mites reach the player, they begin orbiting around the player.  Each mite slows down the player a certain amount.
    //The mites must be either shot away or somehow scraped off.  Once a scraped-off mite is greater than a certain distance away, it flies back to its Maker.

    List<Mite_Brain> mitesSpawned = new List<Mite_Brain>();

    //param
    float timeBetweenMiteSpawns = 4.0f;
    int maxConcurrentMites = 8;

    //hood
    float timeUntilNextMiteSpawn = 0;

    

    public override void OnStartServer()
    {
        base.OnStartServer();
        currentDest = ab.CreateValidRandomPointWithinArena();
        health.EntityWasDamaged += ReceivedDamage;
    }

    protected override void Update()
    {
        if (isServer)
        {
            base.Update();
            SpawnMites();
            NavigateIndifferently();
        }

    }

    [Server]
    private void NavigateIndifferently()
    {
        if ((currentDest - transform.position).magnitude <= closeEnough)
        {
            currentDest = ab.CreateValidRandomPointWithinArena();
        }
    }

    protected override void FixedUpdate()
    {
        if (isServer)
        {
            base.FixedUpdate();
            TurnToFaceDestination(FaceMode.simple);
            MoveTowardsNavTarget(stoppingDist);
        }
    }

    public void ReceivedDamage(GameObject attacker)
    {
        SetCommonAttackTargetForAllMites(attacker, true);
    }

    private void SpawnMites()
    {
        timeUntilNextMiteSpawn += Time.deltaTime;
        if (timeUntilNextMiteSpawn >= timeBetweenMiteSpawns && mitesSpawned.Count < maxConcurrentMites)
        {
            GameObject newMite = Instantiate(weaponPrefab, muz.PrimaryMuzzle.position, muz.PrimaryMuzzle.rotation) as GameObject;
            Mite_Brain mite_Brain = newMite.GetComponent<Mite_Brain>();
            mite_Brain.SetMothership(this);
            newMite.GetComponent<Health>().SetMaxShield(10);
            mitesSpawned.Add(mite_Brain);
            NetworkServer.Spawn(newMite);
            timeUntilNextMiteSpawn = 0;
        }
    }

    public void RemoveMiteUponDeath(Mite_Brain brain)
    {
        mitesSpawned.Remove(brain);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        foreach (Mite_Brain mite in mitesSpawned)
        {
            mite.SelfDestructWithNoMothership();
        }
    }
    public void SetCommonAttackTargetForAllMites(GameObject target, bool shouldSetAngryMode)
    {
        foreach (Mite_Brain mite in mitesSpawned)
        {
            mite.OverrideAttackTarget(target);
            if (shouldSetAngryMode)
            {
                mite.SetAngryMode();
            }
        }
    }
}
