using System;
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
    AbilityManager am;
    Health health;
    EnergySource es;
    PlayerInput pi;

    enum UpgradeType { ShieldMax, ShieldRegen, EnergyMax, EnergyRegen, PriDamage, PriRange, PriIonization, PriCount, Mobility, ScrapVacRange, Custom}
    [SerializeField] UpgradeType upgradeType;
    [SerializeField] float upgradeAmount;
    [SerializeField] string customAbility;
    [SerializeField] string customMethod;


    public virtual void ExecuteUpgrade(UpgradeManager callingUM)
    {
        if (PurchaseCount == 0)
        {
            GatherDependencies(callingUM.gameObject);
        }

        switch (upgradeType)
        {
            case UpgradeType.ShieldMax:
                health.ModifyMaxShield(upgradeAmount);
                return;
            case UpgradeType.ShieldRegen:
                health.ModifyShieldRegen(upgradeAmount);
                return;
            case UpgradeType.EnergyMax:
                es.ModifyMaxEnergy(upgradeAmount);
                return;
            case UpgradeType.EnergyRegen:
                es.ModifyEnergyRegen(upgradeAmount);
                return;
            case UpgradeType.PriCount:
                int countToAdd = Mathf.RoundToInt(upgradeAmount);
                am.PrimaryAbility.ModifyCount(countToAdd);
                return;
            case UpgradeType.PriDamage:
                am.PrimaryAbility.ModifyNormalDamage(upgradeAmount);
                return;
            case UpgradeType.PriIonization:
                am.PrimaryAbility.ModifyIonization(upgradeAmount);
                return;
            case UpgradeType.PriRange:
                am.PrimaryAbility.ModifyRangeViaSpeedOrLifetime(upgradeAmount);
                return;
            case UpgradeType.Mobility:
                pi.ModifyMobility(upgradeAmount);
                return;
            case UpgradeType.Custom:
                Ability abilityToUpgrade = (Ability)callingUM.gameObject.GetComponent(customAbility);
                abilityToUpgrade.Invoke(customMethod, 0);
                return;
        }
        
    }

    protected virtual void GatherDependencies(GameObject go)
    {
        es = go.GetComponent<EnergySource>();
        health = go.GetComponent<Health>();
        am = go.GetComponent<AbilityManager>();
        pi = go.GetComponent<PlayerInput>();

    }

    public void IncrementPurchaseCountForClient()
    {
        PurchaseCount++;
    }


}
