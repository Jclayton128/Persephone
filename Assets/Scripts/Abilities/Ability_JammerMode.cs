using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Ability_JammerMode : Ability
{
    Sprite retractedSprite;
    [SerializeField] Sprite deployedSprite = null;
    SpriteRenderer sr;
    IFF iff;

    //param
    int jammerImportance = 11;

    //hood
    [SyncVar(hook = nameof(UpdateSprite))]
    [SerializeField] bool isDeployed;

    int regularImportance;


    protected override void Start()
    {
        base.Start();
        sr = GetComponent<SpriteRenderer>();
        isDeployed = false;
        retractedSprite = sr.sprite;
        iff = GetComponent<IFF>();
        regularImportance = iff.GetCurrentImportance();
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
        if (isDeployed)
        {
            iff.OverrideCurrentImportance(jammerImportance);
        }
        if (!isDeployed)
        {
            iff.OverrideCurrentImportance(regularImportance);
        }
    }

    [Command]
    private void CmdUpdateJammingMode(bool deployedStatus)
    {
        isDeployed = deployedStatus;
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
