using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BonusClump")]

public class BonusClump : ScriptableObject
{
    [SerializeField] public Sprite Sprite = null;
    
    public enum BonusOptions { ShieldRegenBoost, SpeedBoost, WeaponFireRateBoost, WeaponPowerBoost, WeaponIonizationBoost};

    [SerializeField] public BonusOptions BoostType;
    [SerializeField] public float BoostAmount;
}
