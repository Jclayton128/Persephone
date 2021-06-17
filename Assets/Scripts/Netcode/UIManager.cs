﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIManager : MonoBehaviour
{
    //init
    ClientInstance playerAtThisComputer;

    [SerializeField] Slider hullSlider = null;
    [SerializeField] Slider shieldSlider = null;
    [SerializeField] Slider energySlider = null;
    [SerializeField] TextMeshProUGUI hullMaxTMP = null;
    [SerializeField] TextMeshProUGUI shieldMaxTMP = null;
    [SerializeField] TextMeshProUGUI energyMaxTMP = null;
    [SerializeField] TextMeshProUGUI shieldRateTMP = null;
    [SerializeField] TextMeshProUGUI energyRateTMP = null;
    [SerializeField] Slider shieldIonizationSlider = null;
    [SerializeField] Slider energyIonizationSlider = null;

    [SerializeField] Slider throttle = null;

    [SerializeField] Image scrapBar = null;
    [SerializeField] TextMeshProUGUI upgradePointsTMP = null;

    [SerializeField] Slider persephoneHealth = null;
    [SerializeField] TextMeshProUGUI persephoneStatusTMP = null;

    [SerializeField] Image primaryAbilityPlaceholder = null;
    [SerializeField] Image[] secondaryAbilityPlaceholders = null;


    public void SetLocalPlayerForUI(ClientInstance ci)
    {
        playerAtThisComputer = ci;
    }
    public UIPack GetUIPack(ClientInstance askingCI)
    {
        if (askingCI == playerAtThisComputer)
        {
            UIPack uipack = new UIPack
            {
                HullSlider = hullSlider,
                ShieldSlider = shieldSlider,
                EnergySlider = energySlider,
                HullMaxTMP = hullMaxTMP,
                ShieldMaxTMP = shieldMaxTMP,
                EnergyMaxTMP = energyMaxTMP,
                ShieldRateTMP = shieldRateTMP,
                EnergyRateTMP = energyRateTMP,
                ShieldIonizationSlider = shieldIonizationSlider,
                EnergyIonizationSlider = energyIonizationSlider,           
                ScrapBar = scrapBar,
                UpgradePointsTMP = upgradePointsTMP
                //Throttle = throttle;  

            };
            return uipack;

        }
        else
        {
            Debug.Log("No UI Pack for you!");
            return null;
        }
    }

    public Slider GetPersephoneHealthSlider()
    {
        return persephoneHealth;
    }
    public TextMeshProUGUI GetPersephoneStatusTMP()
    {
        return persephoneStatusTMP;
    }

    public Image[] GetAbilityIcons(ClientInstance askingCI , int numberOfIconsToReturn)
    {
        if (askingCI == playerAtThisComputer)
        {
            Image[] iconsToSend = new Image[numberOfIconsToReturn];

            iconsToSend[0] = primaryAbilityPlaceholder;

            for (int i = 1; i < numberOfIconsToReturn; i++)
            {
                iconsToSend[i] = secondaryAbilityPlaceholders[i - 1];
            }
            for (int i = numberOfIconsToReturn; i <= secondaryAbilityPlaceholders.Length; i++)
            {
                secondaryAbilityPlaceholders[i-1].enabled = false;
            }
            return iconsToSend;
        }
        else
        {
            return null;
        }
    }
}
