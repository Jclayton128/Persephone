using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Ability_JammerMode : Ability
{
    Sprite retractedSprite;
    [SerializeField] Sprite deployedSprite = null;
    [SerializeField] GameObject energyBallPrefab = null;
    SpriteRenderer sr;
    IFF iff;
    Health health;

    //param
    int jammerImportance = 11;

    //hood
    [SyncVar(hook = nameof(UpdateSprite))]
    [SerializeField] bool isDeployed;

    int regularImportance;


    protected override void Start()
    {
        base.Start();
        health = GetComponent<Health>();
        sr = GetComponent<SpriteRenderer>();
        isDeployed = false;
        retractedSprite = sr.sprite;
        iff = GetComponent<IFF>();
        regularImportance = iff.GetCurrentImportance();
        ToggleAbilityStatusOnUI(false);
        
    }

    protected override void MouseClickDownEffect()
    {
        ToggleJammingMode();
    }

    protected override void MouseClickUpEffect()
    {

    }

    private void ToggleJammingMode()
    {
        isDeployed = !isDeployed;
        CmdUpdateJammingMode(isDeployed);
        UpdateSprite(false, false);
        ToggleAbilityStatusOnUI(isDeployed);

    }

    [Command]
    private void CmdUpdateJammingMode(bool deployedStatus)
    {
        isDeployed = deployedStatus;
        if (isDeployed)
        {
            iff.OverrideCurrentImportance(jammerImportance);
            health.SetShieldRegenDiverted(isDeployed);
        }
        if (!isDeployed)
        {
            iff.OverrideCurrentImportance(regularImportance);
            health.SetShieldRegenDiverted(isDeployed);
        }
    }

    private void UpdateUI()
    {
        //Hook into a light on this ability panel turning on or off based on Jamming status
    }

    private void UpdateSprite(bool v1, bool v2)
    {
        if (isDeployed)
        {
            sr.sprite = deployedSprite;
        }
        if (!isDeployed)
        {
            sr.sprite = retractedSprite;
        }
    }

}
