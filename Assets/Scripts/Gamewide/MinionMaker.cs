using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class MinionMaker : NetworkBehaviour
{
    PersNetworkManager pnm;
    ArenaBounds ab;
    List<GameObject> registeredMinions = null;
    public override void OnStartServer()
    {
        base.OnStartServer();
        pnm = FindObjectOfType<PersNetworkManager>();
        ab = FindObjectOfType<ArenaBounds>();
    }

    public void SpawnNewMinion(GameObject chosenMinion)
    {
        Vector2 startPos = ab.CreateRandomPointWithinArena();
        GameObject newMinion = Instantiate(chosenMinion, startPos, Quaternion.identity) as GameObject;
        NetworkServer.Spawn(newMinion);
    }
}
