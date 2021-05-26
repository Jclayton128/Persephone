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


    protected override void Start()
    {
        base.Start();
        currentDest = ab.CreateValidRandomPointWithinArena();
    }

    protected override void Update()
    {
        base.Update();
        EvaluateTarget();
        AdjustColorIfPursuingTarget();
        UpdateRandomDestination();
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
