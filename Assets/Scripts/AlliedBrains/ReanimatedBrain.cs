using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class ReanimatedBrain : NetworkBehaviour
{

    [SerializeField] BonusClump[] clumpOptions = null;
    [SerializeField] List<GameObject> unusedClumpPositions = null;

    bool isActivated = false;



    [Server]
    public void AddNewClump()
    {
        if (unusedClumpPositions.Count == 0) { return; }
        int rand1 = UnityEngine.Random.Range(0, unusedClumpPositions.Count);
        int rand2 = UnityEngine.Random.Range(0, clumpOptions.Length);
        Quaternion randQuat = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-179, 179));

        unusedClumpPositions.RemoveAt(rand1);
        ImplementBonusClumpBoost(clumpOptions[rand2]);

        RpcPushClumpToClients(rand1, rand2);
    }

 

    [ClientRpc]
    private void RpcPushClumpToClients(int position, int option)
    {
        unusedClumpPositions[position].GetComponent<SpriteRenderer>().sprite = clumpOptions[option].Sprite;
        unusedClumpPositions[position].GetComponent<BoxCollider2D>().enabled = true;
    }

    private void ImplementBonusClumpBoost(BonusClump bonusClump)
    {
        BonusClump.BonusOptions bo = bonusClump.BoostType;
        switch (bo)
        {
            case BonusClump.BonusOptions.ShieldRegenBoost:
                //implement this
                return;

            case BonusClump.BonusOptions.SpeedBoost:
                //implement this
                return;

            case BonusClump.BonusOptions.WeaponFireRateBoost:
                //Imp
                return;

            case BonusClump.BonusOptions.WeaponIonizationBoost:
                //imp
                return;

            case BonusClump.BonusOptions.WeaponPowerBoost:
                //imp
                return;

        }
    }

    public void ActivateReanimatedThing()
    {
        isActivated = true;
        gameObject.layer = 16;
    }
}
