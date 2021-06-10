using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class MinionMaker : NetworkBehaviour
{
    PersNetworkManager pnm;
    ArenaBounds ab;
    List<GameObject> registeredMinions = new List<GameObject>();
    public override void OnStartServer()
    {
        base.OnStartServer();
        pnm = FindObjectOfType<PersNetworkManager>();
        ab = FindObjectOfType<ArenaBounds>();
    }

    public void RegisterMinionAcrossNetwork(GameObject minion)
    {
        // Minions should reside in each level. Just before any minions are spawned, they should be registered on all clients.
        // Minion prefabs should not be hand-dropped into the network manager's list.

        if (!registeredMinions.Contains(minion))
        {
            registeredMinions.Add(minion);
            NetworkClient.RegisterPrefab(minion);
        }
    }

    public void SpawnNewMinion(GameObject chosenMinion)
    {
        RegisterMinionAcrossNetwork(chosenMinion);
        Vector2 startPos = ab.CreateValidRandomPointWithinArena();
        GameObject newMinion = Instantiate(chosenMinion, startPos, Quaternion.identity) as GameObject;
        NetworkServer.Spawn(newMinion);
    }

}
