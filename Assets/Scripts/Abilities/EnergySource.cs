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
    Slider energySlider;
    TextMeshProUGUI energyMaxTMP;
    TextMeshProUGUI energyRateTMP;
    Slider energyIonizationSlider;

    //param

    [SyncVar(hook = nameof(UpdateUI))]
    [SerializeField] float energyMax_normal;

    [SyncVar]
    float energyMax_current;

    [SyncVar]
    [SerializeField] float energyRate_normal;

    [SyncVar(hook = nameof(UpdateUI))]
    float energyRate_current;

    [SyncVar(hook = nameof(UpdateUI))]
    float ionizationAmount = 0;

    float ionFactor = 0;
    //float megaPowerRegenPerSecond = 50.0f;
    //float maxTimeInBonusMode = 10;

    //hood
    [SyncVar(hook = nameof(UpdateUI))]
    float energyCurrentLevel;

    [SerializeField] float ionizationRemoveRate;
    bool isPlayer = true;
    bool isDisabled = false;
    float bonusRegen;
    float endtimeForBonusRegen;


    void Start()
    {
        if (gameObject.tag != "Player")
        {
            isPlayer = false;
        }
        if (hasAuthority && isPlayer)
        {
            HookIntoLocalUI();
        }
        if (isServer)
        {
            Health health = GetComponent<Health>();
            health.EntityIsDying += ReactToBecomingDisabled;
            health.EntityIsRepaired += ReactToBecomingRepaired;
        }

        energyCurrentLevel = energyMax_normal;
        ionizationRemoveRate = GetComponent<Health>().GetPurificationRate();
    }

    private void HookIntoLocalUI()
    {
        ClientInstance ci = ClientInstance.ReturnClientInstance();
        UIManager uim = FindObjectOfType<UIManager>();
        UIPack uipack = uim.GetUIPack(ci);
        energySlider = uipack.EnergySlider;
        energyMaxTMP = uipack.EnergyMaxTMP;
        energyRateTMP = uipack.EnergyRateTMP;
        energyIonizationSlider = uipack.EnergyIonizationSlider;
        UpdateUI(0,0);
    }

    private void UpdateUI(float oldValue, float newValue)
    {
        if (!isPlayer) { return; };
        if (hasAuthority)
        {
            energySlider.maxValue = energyMax_normal;
            energySlider.value = energyCurrentLevel;
            energyMaxTMP.text = energyMax_normal.ToString();
            energyRateTMP.text = energyRate_current.ToString();
            energyIonizationSlider.value = ionFactor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            ProcessIonization();
            ProcessBonusRegen();
            RegenerateEnergy();
        }
    }



    private void ProcessIonization()
    {
        //Remove Ionization
        ionizationAmount -= ionizationRemoveRate * Time.deltaTime;
        ionizationAmount = Mathf.Clamp(ionizationAmount, 0, energyMax_normal);

        if (ionizationAmount > 0)
        {
            //TODO spawn/maintain a particle effect. Ensure it is seen on all clients
        }

        //Process Ionization effects
        ionFactor = 1 - ((energyMax_normal - ionizationAmount) / energyMax_normal);
        if (isDisabled == false)
        {
            energyMax_current = (1 - ionFactor) * energyMax_normal;
            energyRate_current = ((1 - ionFactor) * energyRate_normal);
        }
    }

    private void ProcessBonusRegen()
    {
        if (Time.time <= endtimeForBonusRegen)
        {
            energyRate_current += bonusRegen;
        }

    }

    private void RegenerateEnergy()
    {
        if (isDisabled) { return; }
        energyCurrentLevel += energyRate_current * Time.deltaTime;
        energyCurrentLevel = Mathf.Clamp(energyCurrentLevel, 0, energyMax_current);

    }

    public float GetCurrentPowerLevel()
    {
        return energyCurrentLevel;
    }

    public bool CheckEnergy(float value)
    {
        if (isDisabled) { return false; }
        if (energyCurrentLevel - value >= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CheckSpendEnergy(float value)
    {
        if (isDisabled) { return false; }
        if (energyCurrentLevel - value >= 0)
        {
            energyCurrentLevel -= value;

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
        energyCurrentLevel = energyMax_normal;
    }

    public float GetMaxPower()
    {
        return energyMax_normal;
    }

    public void SetTemporaryRegen(float newBonusAmount, float duration)
    {
        bonusRegen = newBonusAmount;
        endtimeForBonusRegen = Time.time + duration;
    }


    public float GetPowerRegen()
    {
        return energyRate_current;
    }
    public void SetMaxPower(float newMaxPower)
    {
        energyMax_normal = newMaxPower;
        UpdateUI(0,0);
    }

    public void ReceiveIonizationDamage(float value)
    {
        ionizationAmount += value;
    }

    //public void SetPowerRegen(float newRegen)
    //{
    //    energyRate_current = newRegen;
    //    if (energyRateTMP)
    //    {
    //        energyRateTMP.text = energyRate_current.ToString("F1");
    //        //Debug.Log("attempting to adjust energy regen text");
    //    }
    //}

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

    public void ReactToBecomingDisabled()
    {
        isDisabled = true;
        energyCurrentLevel = 0;
        energyRate_current = 0;
    }

    public void ReactToBecomingRepaired()
    {
        isDisabled = false;
    }

}
