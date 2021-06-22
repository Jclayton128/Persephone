﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_JammerMode : Ability
{
    Sprite retractedSprite;
    [SerializeField] Sprite deployedSprite = null;
    SpriteRenderer sr;

    //hood
    bool isDeployed;


    protected override void Start()
    {
        base.Start();
        sr = GetComponent<SpriteRenderer>();
        isDeployed = false;
        retractedSprite = sr.sprite;

    }

    protected override void MouseClickDownEffect()
    {
        ToggleJammingMode();
        UpdateUI();
    }

    protected override void MouseClickUpEffect()
    {

    }

    private void ToggleJammingMode()
    {
        isDeployed = !isDeployed;
        if (isDeployed)
        {
            sr.sprite = deployedSprite;
        }
        if (!isDeployed)
        {
            sr.sprite = retractedSprite;
        }
        //Execute actual Jamming business logic here.
    }

    private void UpdateUI()
    {
        //Hook into a light on this ability panel turning on or off based on Jamming status
    }

}