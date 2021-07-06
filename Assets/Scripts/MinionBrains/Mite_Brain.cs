using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Mite_Brain : Brain
{
    // This AI should orbit around a mothership until a player is set as the target.
    // A player is set as a target anytime it gets within range of a mite.
    // Once one mite detects a player, all mites get broadcasted the player's location and they all begin to chase the player.
    // Mites then fly quickly, but slower than player's top speed.
    // Once they are "at" the player, they begin to orbit the player, slowing it down.
    // Mites can only be removed by shooting them or scraping them off, or somehow outrunning them.
    // If they are outrunned and sufficient space is gained, they then attempt to travel back to the mothership.
    // If there is no mothership, then they just self-destruct.


    //init
    Maker_Brain mothership;

    //param
    float orbitRange = 1.0f;
    float timeToResetAngryStatus = 4.0f;
    float maxSurvivalTimeWithoutMothership = 2.0f;


    //hood
    bool isAngry = false;
    float timeSpentAngry = 0;
    float timeUntilNextShot = 0;
    float distanceToAttackTarget = 0;

    // Start is called before the first frame update

    public override void OnStartServer()
    {
        base.OnStartServer();
        currentDest = ab.CreateRandomPointWithinArena(mothership.transform.position, orbitRange, ArenaBounds.DestinationMode.noFartherThan);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    protected override void Update()
    {
        base.Update();
        SelectBestTarget();
        AttackTarget();
        UpdateAnger();

    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        OrbitMothershipIfNoTarget();
        ChaseAndOrbitTarget();
    }


    private void UpdateAnger()
    {
        if (isAngry)
        {
            timeSpentAngry += Time.deltaTime;
            if (timeSpentAngry >= timeToResetAngryStatus)
            {
                isAngry = false;
                timeSpentAngry = 0;
                return;
            }
        }
    }

    public void SetAngryMode()
    {
        isAngry = true;
    }

    public void OverrideAttackTarget(GameObject target)
    {
        if (!currentAttackTarget)
        {
            currentAttackTarget = target;
        }
    }
    private void AttackTarget()
    {
        timeUntilNextShot -= Time.deltaTime;
        if (!currentAttackTarget) { return; }
        if (timeUntilNextShot <= 0 && distanceToAttackTarget <= weaponLifetime * weaponSpeed && angleToAttackTarget <= boresightThreshold)
        {
            GameObject bullet = Instantiate(weaponPrefab, transform.position, transform.rotation) as GameObject;
            bullet.GetComponent<Rigidbody2D>().velocity = weaponSpeed * bullet.transform.up;
            bullet.layer = 11;
            DamageDealer dd = bullet.GetComponent<DamageDealer>();
            dd.SetNormalDamage(weaponNormalDamage);
            dd.SetSpeedModifier(weaponSpeedMod);
            NetworkServer.Spawn(bullet);
            Destroy(bullet, weaponLifetime);
            timeUntilNextShot = intervalBetweenWeapons;
        }
    }

    protected override void Scan()
    {
        base.Scan();
        BroadcastAttackTargetToAllMites();
        CheckForMothershipAlive();
    }

    private void BroadcastAttackTargetToAllMites()
    {
        if (mothership && currentAttackTarget)
        {
            mothership.SetCommonAttackTargetForAllMites(currentAttackTarget, false);
        }
    }

    private void ChaseAndOrbitTarget()
    {
        if (!currentAttackTarget) { return; }
        if (currentAttackTarget)
        {
            if (distanceToAttackTarget > orbitRange * 2)
            {
                currentDest = currentAttackTarget.transform.position;
                TurnToFaceDestination(faceMode);
                MoveTowardsNavTarget();
            }
            else
            {
                if (distToDest <= closeEnough)
                {
                    currentDest =  CUR.CreateRandomPointNearInputPoint(currentAttackTarget.transform.position, orbitRange, orbitRange/2f);
                }
                else
                {
                    currentDest = currentAttackTarget.transform.position;
                    TurnToFaceDestination(faceMode);
                    MoveTowardsNavTarget();
                }
            }
        }
    }

    private void OrbitMothershipIfNoTarget()
    {
        if (!mothership) { return; }
        if (currentAttackTarget) { return; }
        if (distToDest <= closeEnough)
        {
            currentDest = CUR.CreateRandomPointNearInputPoint(mothership.transform.position, orbitRange, orbitRange/2f);
        }
        else
        {
            TurnToFaceDestination(faceMode);
            MoveTowardsNavTarget();
        }

    }

    private void CheckForMothershipAlive()
    {
        if (!mothership)
        {
            SelfDestructWithNoMothership();
        }
    }

    public void SetMothership(Maker_Brain mother)
    {
        mothership = mother;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (mothership)
        {
            mothership.RemoveMiteUponDeath(this);
        }
    }
    public void SelfDestructWithNoMothership()
    {
        //TODO create a small explosion at death site.
        float randomTime = UnityEngine.Random.Range(0, maxSurvivalTimeWithoutMothership);
        Destroy(gameObject, randomTime);
    }
}
