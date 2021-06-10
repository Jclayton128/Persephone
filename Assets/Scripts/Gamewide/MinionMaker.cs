using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class MinionMaker : NetworkBehaviour
{
    PersNetworkManager pnm;
    ArenaBounds ab;
    public override void OnStartServer()
    {
        base.OnStartServer();
        pnm = FindObjectOfType<PersNetworkManager>();
        ab = FindObjectOfType<ArenaBounds>();
    }

    public void SpawnNewMinion()
    {
        GameObject minionPrefab = pnm.spawnPrefabs[2];
        Vector2 startPos = ab.CreateValidRandomPointWithinArena();
        GameObject newMinion = Instantiate(minionPrefab, startPos, Quaternion.identity) as GameObject;
        NetworkServer.Spawn(newMinion);

    }

}
