using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeOption")]
public class UpgradeOption : ScriptableObject
{
    [SerializeField] public int PurchaseCount = 0;
    [SerializeField] public Sprite UpgradeIcon = null;
    [SerializeField] public string NameForUI;
    [SerializeField] public string Explanation;

    public int LocalUpgradeOptionID;


    public virtual void ExecuteUpgrade()
    {
        // implement upgrade logic here      

        
    }

    public void IncrementPurchaseCountForClient()
    {
        PurchaseCount++;
    }


}
