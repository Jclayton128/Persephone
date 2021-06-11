using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class PowerSource : NetworkBehaviour
{
    //init
    [SerializeField] Slider powerReserve = null;
    [SerializeField] TextMeshProUGUI maxEnergytmp = null;
    [SerializeField] TextMeshProUGUI regenEnergytmp = null;

    //param
    float maxPower = 10;
    float powerRegenPerSecond = 0;
    float megaPowerRegenPerSecond = 50.0f;
    float maxTimeInBonusMode = 10;

    //hood
    float currentPower;
    bool isInMegaEnergyBonusMode = false;
    float timeInBonusMode = 0;
    void Start()
    {
        currentPower = maxPower;
        powerReserve.maxValue = maxPower;
    }

    // Update is called once per frame
    void Update()
    {
        RegenPower();
        currentPower = Mathf.Clamp(currentPower, 0, maxPower);
        powerReserve.value = currentPower;

    }

    private void RegenPower()
    {
        if (currentPower < maxPower)
        {
            currentPower += powerRegenPerSecond * Time.deltaTime;
        }
    }

    public float GetCurrentPowerLevel()
    {
        return currentPower;
    }

    public void ModifyCurrentPowerLevel(float powerChange)
    {
        currentPower += powerChange;
    }

    public void ResetPowerLevel()
    {
        currentPower = maxPower;
    }

    public float GetMaxPower()
    {
        return maxPower;
    }

    public float GetPowerRegen()
    {
        return powerRegenPerSecond;
    }
    public void SetMaxPower(float newMaxPower)
    {
        maxPower = newMaxPower;
        if (powerReserve && maxEnergytmp)
        {
            powerReserve.maxValue = maxPower;
            maxEnergytmp.text = maxPower.ToString();
            //Debug.Log("attempting to adjust max energy text");
        }
    }

    public void SetPowerRegen(float newRegen)
    {
        powerRegenPerSecond = newRegen;
        if (regenEnergytmp)
        {
            regenEnergytmp.text = powerRegenPerSecond.ToString("F1");
            //Debug.Log("attempting to adjust energy regen text");
        }
    }

    public void SetMegaPowerBonusMode()
    {
        isInMegaEnergyBonusMode = true;
        StartCoroutine(MegaPowerBonusModeTimer());
        powerRegenPerSecond += megaPowerRegenPerSecond;
        regenEnergytmp.text = powerRegenPerSecond.ToString("F1");
    }

    IEnumerator MegaPowerBonusModeTimer()
    {
        while (true)
        {
            timeInBonusMode += Time.deltaTime;
            if (timeInBonusMode >= maxTimeInBonusMode)
            {
                isInMegaEnergyBonusMode = false;
                timeInBonusMode = 0;
                powerRegenPerSecond -= megaPowerRegenPerSecond;
                regenEnergytmp.text = powerRegenPerSecond.ToString("F1");
                yield break;
            }
            yield return new WaitForFixedUpdate();
        }
    }
}
