using System.Collections;
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
    UpgradePanelUI upui;

    public int CurrentLevel = 1;
    //public int CurrentLevel { get; private set; } = 1;
    int baseUpgradeCost = 10; //This is the scrap required for the first upgrade.
    int currentUpgradeCost; //This is calculated by multiplying the current upgrade level by base cost. Each upgrade costs more than last.

    int currentScrap = 0;

    [SyncVar(hook = nameof(UpdateUI))]
    float currentUpgradePoints;

    [SyncVar(hook = nameof(UpdateUI))]
    float scrapBarFactor;

    float scrapBarZeroPoint = 0.1f;
    float scrapBarOnePoint = 0.9f;

    public Action<int> OnLevelUp;
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
        upui = uim.GetUpgradePanelUI(ci);

        upgradePointsAvailableTMP = uipack.UpgradePointsTMP;
        scrapBar = uipack.ScrapBar;

        UpdateUI(0, 0);

    }

    private void Update()
    {
        if (hasAuthority)
        {
            HandleUpgradeMenuToggle();
            HandleUpgradeSelection();
        }
    }

    private void HandleUpgradeMenuToggle()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            upui.TogglePanelPosition();
        }
    }

    private void HandleUpgradeSelection()
    {
        if (!upui.IsExtended) { return; }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            upui.SetSelectorKnob(1);
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            upui.SetSelectorKnob(2);
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            upui.SetSelectorKnob(3);
            return;
        }
    }

    public void GainScrap(int amount)
    {
        if (!isServer) { return; }
        currentUpgradeCost = baseUpgradeCost * CurrentLevel;
        currentScrap += amount;

        if (currentScrap >= currentUpgradeCost)
        {
            CurrentLevel++;
            currentUpgradePoints++;
            currentScrap = 0;
            scrapBarFactor = 0;
            OnLevelUp?.Invoke(CurrentLevel);
            UpdateUI(0, 0);
        }
        if (currentScrap < currentUpgradeCost)
        {
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
