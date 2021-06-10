using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitTracker : NetworkBehaviour
{
    List<GameObject> currentMinions = new List<GameObject>();
    [SerializeField] int currentMinionCount;
    LevelManager lm;

    public override void OnStartServer()
    {
        lm = GetComponent<LevelManager>();
    }

    [Server]
    public void AddMinion(GameObject newMinion)
    {
        currentMinions.Add(newMinion);
        currentMinionCount = currentMinions.Count;
    }

    [Server]
    public void RemoveMinion(GameObject deadMinion)
    {
        Debug.Log("remove minion called");
        currentMinions.Remove(deadMinion);
        currentMinionCount = currentMinions.Count;
        if (currentMinionCount == 0)
        {
            lm.AdvanceToNextLevel();
        }
    }


}
