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
        currentMinions.Remove(deadMinion);
        currentMinionCount = currentMinions.Count;
        if (currentMinionCount == 0)
        {
            // TODO Speed up the Pers since there aren't any enemies left?
        }
    }

    [Server]
    public void DestroyAllMinions()
    {
        GameObject[] minionArray = currentMinions.ToArray();
        for (int i = minionArray.Length-1; i >= 0; i--)
        {
            Destroy(minionArray[i]);
        }
        currentMinions.Clear();
    }


}
