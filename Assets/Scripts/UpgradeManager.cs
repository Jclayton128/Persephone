﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using TMPro;
using UnityEngine.UI;

public class UpgradeManager : NetworkBehaviour
{
    [SerializeField] Image scrapBar;
    [SerializeField] TextMeshProUGUI upgradePointsAvailableTMP;

    int currentUpgradeLevel = 1;
    int baseUpgradeCost = 10; //This is the scrap required for the first upgrade.
    int currentUpgradeCost; //This is calculated by multiplying the current upgrade level by base cost. Each upgrade costs more than last.

    int currentScrap = 0;

    [SyncVar(hook = nameof(UpdateUI))]
    float currentUpgradePoints;

    [SyncVar(hook = nameof(UpdateUI))]
    float scrapBarFactor;

    float scrapBarZeroPoint = 0.1f;
    float scrapBarOnePoint = 0.9f;


    void Start()
    {        
        if (hasAuthority)
        {
            HookIntoLocalUI();
        }
    }

    private void HookIntoLocalUI()
    {

        ClientInstance ci = ClientInstance.ReturnClientInstance();
        UIManager uim = FindObjectOfType<UIManager>();
        UIPack uipack = uim.GetUIPack(ci);

        upgradePointsAvailableTMP = uipack.UpgradePointsTMP;
        scrapBar = uipack.ScrapBar;

        UpdateUI(0, 0);

    }


    public void GainScrap(int amount)
    {
        if (!isServer) { return; }
        currentUpgradeCost = baseUpgradeCost * currentUpgradeLevel;
        currentScrap += amount;

        if (currentScrap >= currentUpgradeCost)
        {
            Debug.Log("more scrap than required to level up");
            currentUpgradeLevel++;
            currentUpgradePoints++;
            currentScrap = 0;
            scrapBarFactor = 0;
            UpdateUI(0, 0);
        }
        if (currentScrap < currentUpgradeCost)
        {
            Debug.Log($"Scrap: {currentScrap}/{currentUpgradeCost}");
            Debug.Log("Scrap Bar Factor: " + scrapBarFactor);
            scrapBarFactor = (float)currentScrap / (float)currentUpgradeCost;
            UpdateUI(0, 0);
        }
    }


    private void UpdateUI(float v1, float v2)
    {
        if (upgradePointsAvailableTMP)
        {
            upgradePointsAvailableTMP.text = currentUpgradePoints.ToString();
        }
        if (scrapBar)
        {
            float factor = ConvertFactorIntoFillAmount();
            Debug.Log("UI update, factor should be: " + factor);
            scrapBar.fillAmount = factor;
        }
    }

    private float ConvertFactorIntoFillAmount()
    {
        float fac = Mathf.Lerp(scrapBarZeroPoint, scrapBarOnePoint, scrapBarFactor);
        return fac;
    }

}