using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Fencer_Brain : Brain
{
    //param
    //float minRangeToMine = 2.0f;  This is captured under the superclass Brain's TimeBetweenShots
    int mineLayerMask = 1 << 19;

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    protected override void Update()
    {
        base.Update();
        LayMines();

    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        TurnToFaceDestination(faceMode);
        MoveTowardsNavTarget(true);
    }

    private void LayMines()
    {
        RaycastHit2D possibleMines = Physics2D.CircleCast(transform.position, intervalBetweenWeapons, transform.up, 0.0f, mineLayerMask);
        if (!possibleMines)
        {
            GameObject mine = Instantiate(weaponPrefab, muz.PrimaryMuzzle.position, muz.PrimaryMuzzle.rotation) as GameObject;
            mine.layer = 19;
            NetworkServer.Spawn(mine);
            Destroy(mine, weaponLifetime);
        }
    }

    #region Do-Nothing Targeting Overrides due to Fencer Indifference
    protected override void SelectBestTarget()
    {
        //Do nothing. Fencers are indifferent to players.
    }

    public override void CheckAddTargetToList(IFF target)
    {
        //Do nothing. Fencers are indifferent to players.
    }

    public override void RemoveTargetFromList(IFF target)
    {
        //Do nothing. Fencers are indifferent to players.
    }
    public override void ResortList()
    {
        //Do nothing. Fencers are indifferent to players.
    }
    #endregion




}
