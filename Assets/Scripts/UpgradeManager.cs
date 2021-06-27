using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using TMPro;
using UnityEngine.UI;

public class UpgradeManager : NetworkBehaviour
{
    Image scrapBar;
    TextMeshProUGUI upgradePointsAvailableTMP;
    TextMeshProUGUI shipLevelCounterTMP;
    UpgradePanelUI upui;
    [SerializeField] UpgradeOption[] allUpgradeOptions = null;

    [SyncVar(hook = nameof(UpdateUIForLevel))]
    int currentLevel = 1;

    //public int CurrentLevel { get; private set; } = 1;
    int baseUpgradeCost = 10; //This is the scrap required for the first upgrade.
    int currentUpgradeCost; //This is calculated by multiplying the current upgrade level by base cost. Each upgrade costs more than last.

    int currentScrap = 0;

    [SyncVar(hook = nameof(UpdateUIForUpgradePoints))]
    int currentUpgradePoints;

    [SyncVar(hook = nameof(UpdateUIForScrap))]
    float scrapBarFactor;

    float scrapBarZeroPoint = 0.1f;
    float scrapBarOnePoint = 0.9f;

    public Action<int> OnLevelUp;

    UpgradeOption[] currentUpgradeOptions = new UpgradeOption[3];
    int currentUpgradeSelectionIndex = -1;
    void Start()
    {
        if (hasAuthority)
        {
            HookIntoLocalUI();
        }
        PrepareUpgradeOptions();
    }

    private void PrepareUpgradeOptions()
    {
        int i = 0;
        foreach (UpgradeOption upgradeOption in allUpgradeOptions)
        {
            upgradeOption.PurchaseCount = 0;
            upgradeOption.LocalUpgradeOptionID = i;
            i++;
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
        shipLevelCounterTMP = uipack.ShipLevelCounterTMP;

        UpdateUIForScrap(0, 0);
        CreateNewUpgradeOptionsForLocalClient();

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
        if (hasAuthority && Input.GetKeyDown(KeyCode.Tab))
        {
            upui.TogglePanelPosition();
            if (!upui.IsExtended && currentUpgradeSelectionIndex >= 0 && currentUpgradePoints > 0)
            {
                int indexWithinAllOptions = currentUpgradeOptions[currentUpgradeSelectionIndex].LocalUpgradeOptionID;
                CmdPurchaseSelectedUpgrade(indexWithinAllOptions);

                allUpgradeOptions[indexWithinAllOptions].IncrementPurchaseCountForClient();
                currentUpgradeSelectionIndex = -1;
                CreateNewUpgradeOptionsForLocalClient();
                //TODO Cha-Ching audio for purchasing an upgrade.

            }
        }

    }

    [Command]
    private void CmdPurchaseSelectedUpgrade(int index)
    {
        if (currentUpgradePoints > 0)
        {
            currentUpgradePoints--;
            allUpgradeOptions[index].ExecuteUpgrade();
        }

    }

    private void HandleUpgradeSelection()
    {
        if (!upui.IsExtended) { return; }
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            //TODO insert selector click audio
            currentUpgradeSelectionIndex = -1;
            upui.SetSelectorKnob(currentUpgradeSelectionIndex);
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //TODO insert selector click audio
            currentUpgradeSelectionIndex = 0;
            upui.SetSelectorKnob(currentUpgradeSelectionIndex+1, currentUpgradeOptions[currentUpgradeSelectionIndex]);
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            //TODO insert selector click audio
            currentUpgradeSelectionIndex = 1;
            upui.SetSelectorKnob(currentUpgradeSelectionIndex+1, currentUpgradeOptions[currentUpgradeSelectionIndex]);
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            //TODO insert selector click audio
            currentUpgradeSelectionIndex = 2;
            upui.SetSelectorKnob(currentUpgradeSelectionIndex+1, currentUpgradeOptions[currentUpgradeSelectionIndex]);
            return;
        }
    }

    public void GainScrap(int amount)
    {
        if (!isServer) { return; }
        currentUpgradeCost = baseUpgradeCost * currentLevel;
        currentScrap += amount;

        if (currentScrap >= currentUpgradeCost)
        {
            //TODO fanfare on level up sound via TargetRPC
            LevelUp();
        }
        if (currentScrap < currentUpgradeCost)
        {
            //TODO Scrap bloop pickup sound via TargetRPC
            scrapBarFactor = (float)currentScrap / (float)currentUpgradeCost;
            //UpdateUIForScrap(0, 0);
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        currentUpgradePoints++;
        currentScrap = 0;
        scrapBarFactor = 0;
        OnLevelUp?.Invoke(currentLevel);
        //UpdateUI(0, 0);

    }

    private void CreateNewUpgradeOptionsForLocalClient()
    {
        int rand1 = UnityEngine.Random.Range(0, allUpgradeOptions.Length);
        int rand2 = 0;
        int rand3 = 0;
        do
        {
            rand2 = UnityEngine.Random.Range(0, allUpgradeOptions.Length);
        }
        while (rand2 == rand1);
        do
        {
            rand3 = UnityEngine.Random.Range(0, allUpgradeOptions.Length);
        }
        while (rand3 == rand1 || rand3 == rand2);

        currentUpgradeOptions[0] = allUpgradeOptions[rand1];
        currentUpgradeOptions[1] = allUpgradeOptions[rand2];
        currentUpgradeOptions[2] = allUpgradeOptions[rand3];
        upui.UpdateOptions(currentUpgradeOptions[0], currentUpgradeOptions[1], currentUpgradeOptions[2]);
        //Create 3 random ints from 0 to 5
        //Pull the upgrade options at those indices and put them in the chosenUpgradeOptions array
        //Put the UI stuff from the different chosen upgrade options into their correct cubbies on the panel
    }

    private void UpdateUIForScrap(float v1, float v2)
    {
        if (scrapBar)
        {
            float factor = ConvertFactorIntoFillAmount();
            scrapBar.fillAmount = factor;
        }
    }

    private void UpdateUIForUpgradePoints(int v1, int v2)
    {
        if (upgradePointsAvailableTMP)
        {
            upgradePointsAvailableTMP.text = currentUpgradePoints.ToString();
        }
    }

    private void UpdateUIForLevel(int v1, int v2)
    {
        if (shipLevelCounterTMP)
        {
            shipLevelCounterTMP.text = "Lvl\r\n" + currentLevel.ToString();
        }
    }

    private float ConvertFactorIntoFillAmount()
    {
        float fac = Mathf.Lerp(scrapBarZeroPoint, scrapBarOnePoint, scrapBarFactor);
        return fac;
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

}
