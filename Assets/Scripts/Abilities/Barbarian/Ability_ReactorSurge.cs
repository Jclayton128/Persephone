using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Ability_ReactorSurge : Ability
{
    [SerializeField] float regenBonus;
    [SerializeField] float regenBonusDuration;

    //hood
    bool isSurging = false;
    bool isCharged = true;
    LevelManager lm;

    protected override void Start()
    {
        base.Start();
        lm = FindObjectOfType<LevelManager>();
        lm.OnLevelAdvance += RechargeAbility;
        ToggleAbilityStatusOnUI(true);
    }


    protected override void MouseClickDownEffect()
    {
        CmdRequestExecuteAbility();      

    }

    [Command]
    private void CmdRequestExecuteAbility()
    {
        if (isCharged)
        {
            isCharged = false;
            es.SetTemporaryRegen(regenBonus, regenBonusDuration);
            ToggleAbilityStatusOnUI(false);
        }

    }

    protected override void MouseClickUpEffect()
    {
        
    }

    private void RechargeAbility(int index)
    {
        isCharged = true;
        ToggleAbilityStatusOnUI(true);
    }

    public void OnDestroy()
    {
        lm.OnLevelAdvance -= RechargeAbility;
    }


}
