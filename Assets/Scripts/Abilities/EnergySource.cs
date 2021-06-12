using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class EnergySource : NetworkBehaviour
{
    //init
    [SerializeField] Slider energySlider;
    [SerializeField] TextMeshProUGUI energyMaxTMP;
    [SerializeField] TextMeshProUGUI energyRateTMP;

    //param

    [SyncVar(hook = nameof(UpdateUI))]
    [SerializeField] float energyMax;

    [SyncVar(hook = nameof(UpdateUI))]
    [SerializeField] float energyRate;
    //float megaPowerRegenPerSecond = 50.0f;
    //float maxTimeInBonusMode = 10;

    //hood
    [SyncVar(hook = nameof(UpdateUI))]
    float energyCurrent;

    //bool isInMegaEnergyBonusMode = false;
    //float timeInBonusMode = 0;
    void Start()
    {
        if (hasAuthority)
        {
            Debug.Log("tried to hook into UI");
            HookIntoLocalUI();
        }
        energyCurrent = energyMax;
    }

    private void HookIntoLocalUI()
    {
        ClientInstance ci = ClientInstance.ReturnClientInstance();
        UIManager uim = FindObjectOfType<UIManager>();
        UIPack uipack = uim.GetUIPack(ci);
        Debug.Log("received: " + uipack);
        energySlider = uipack.EnergySlider;
        energyMaxTMP = uipack.EnergyMaxTMP;
        energyRateTMP = uipack.EnergyRateTMP;
        UpdateUI(0,0);
    }

    private void UpdateUI(float oldValue, float newValue)
    {
        energySlider.maxValue = energyMax;
        energySlider.value = energyCurrent;
        energyMaxTMP.text = energyMax.ToString();
        energyRateTMP.text = energyRate.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            RegenPower();
            energyCurrent = Mathf.Clamp(energyCurrent, 0, energyMax);
        }
    }

    private void RegenPower()
    {
        if (energyCurrent < energyMax)
        {
            energyCurrent += energyRate * Time.deltaTime;
        }
    }

    public float GetCurrentPowerLevel()
    {
        return energyCurrent;
    }

    public bool CheckEnergy(float value)
    {
        if (energyCurrent - value >= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CheckDrainEnergy(float value)
    {
        if (energyCurrent - value >= 0)
        {
            energyCurrent -= value;

            return true;
        }
        else
        {
            return false;
        }

    }

    //public void ModifyCurrentPowerLevel(float powerChange)
    //{
    //    energyCurrent += powerChange;
    //}

    public void ResetPowerLevel()
    {
        energyCurrent = energyMax;
    }

    public float GetMaxPower()
    {
        return energyMax;
    }

    public float GetPowerRegen()
    {
        return energyRate;
    }
    public void SetMaxPower(float newMaxPower)
    {
        energyMax = newMaxPower;
        if (energySlider && energyMaxTMP)
        {
            energySlider.maxValue = energyMax;
            energyMaxTMP.text = energyMax.ToString();
            //Debug.Log("attempting to adjust max energy text");
        }
    }

    public void SetPowerRegen(float newRegen)
    {
        energyRate = newRegen;
        if (energyRateTMP)
        {
            energyRateTMP.text = energyRate.ToString("F1");
            //Debug.Log("attempting to adjust energy regen text");
        }
    }

    //public void SetMegaPowerBonusMode()
    //{
    //    isInMegaEnergyBonusMode = true;
    //    StartCoroutine(MegaPowerBonusModeTimer());
    //    energyRate += megaPowerRegenPerSecond;
    //    energyRateTMP.text = energyRate.ToString("F1");
    //}

    //IEnumerator MegaPowerBonusModeTimer()
    //{
    //    while (true)
    //    {
    //        timeInBonusMode += Time.deltaTime;
    //        if (timeInBonusMode >= maxTimeInBonusMode)
    //        {
    //            isInMegaEnergyBonusMode = false;
    //            timeInBonusMode = 0;
    //            energyRate -= megaPowerRegenPerSecond;
    //            energyRateTMP.text = energyRate.ToString("F1");
    //            yield break;
    //        }
    //        yield return new WaitForFixedUpdate();
    //    }
    //}
}
