using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeOption")]
public class UpgradeOption : ScriptableObject
{
    [SerializeField] public int PurchaseCount = 0;
    [SerializeField] public Sprite UpgradeIcon = null;
    [SerializeField] public string nameForUI;

    public int LocalUpgradeOptionID;


    public virtual void ExecuteUpgrade()
    {
        PurchaseCount++;
        // implement upgrade logic here
        Debug.Log($"purchased {nameForUI}");
        
    }


}
