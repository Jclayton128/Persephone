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
    Health health;

    //param

    [SyncVar(hook = nameof(UpdateUI))]
    [SerializeField] float energyMax_normal;

    [SyncVar]
    float energyMax_current;

    [SyncVar]
    [SerializeField] float energyRate_normal;

    [SyncVar(hook = nameof(UpdateUI))]
    float energyRate_current;


    //hood
    [SyncVar(hook = nameof(UpdateUI))]
    float energyCurrentLevel;

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
            health = GetComponent<Health>();
            health.EntityIsDying += ReactToBecomingDisabled;
            health.EntityIsRepaired += ReactToBecomingRepaired;
        }

        energyCurrentLevel = energyMax_normal;
    }

    private void HookIntoLocalUI()
    {
        ClientInstance ci = ClientInstance.ReturnClientInstance();
        UIManager uim = FindObjectOfType<UIManager>();
        UIPack uipack = uim.GetUIPack(ci);
        energySlider = uipack.EnergySlider;
        energyMaxTMP = uipack.EnergyMaxTMP;
        energyRateTMP = uipack.EnergyRateTMP;
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
        if (isDisabled == false)
        {
            energyMax_current = (1 - health.IonFactor) * energyMax_normal;
            energyRate_current = ((1 - health.IonFactor) * energyRate_normal);
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
