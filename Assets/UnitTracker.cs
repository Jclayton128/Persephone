using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitTracker : NetworkBehaviour
{
    List<GameObject> currentMinions = new List<GameObject>();
    LevelManager lm;

    public override void OnStartServer()
    {
        lm = GetComponent<LevelManager>();
    }

    [Server]
    public void AddMinion(GameObject newMinion)
    {
        currentMinions.Add(newMinion);
    }

    [Server]
    public void RemoveMinion(GameObject deadMinion)
    {
        currentMinions.Remove(deadMinion);

        if (currentMinions.Count == 0)
        {
            lm.AdvanceToNextLevel();
        }
    }


}
