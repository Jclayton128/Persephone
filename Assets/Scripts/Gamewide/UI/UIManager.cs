using System.Collections;
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
    [SerializeField] Slider ionizationSlider = null;

    [SerializeField] Slider throttle = null;

    [SerializeField] Image scrapBar = null;
    [SerializeField] TextMeshProUGUI upgradePointsTMP = null;
    [SerializeField] TextMeshProUGUI shipLevelCounterTMP = null;

    [SerializeField] Slider persephoneHealth = null;
    [SerializeField] TextMeshProUGUI persephoneStatusTMP = null;
    [SerializeField] Image persephoneCompass = null;

    [SerializeField] Image primaryAbilityPlaceholder = null;
    [SerializeField] Image[] secondaryAbilityPlaceholders = null;
    [SerializeField] Image[] secondaryAbilityStatusPlaceholders = null;

    [SerializeField] UpgradePanelUI upgradePanelUI = null;


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
                IonizationSlider = ionizationSlider,
                ScrapBar = scrapBar,
                UpgradePointsTMP = upgradePointsTMP,
                ShipLevelCounterTMP = shipLevelCounterTMP
                
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

    public Image GetPersephoneCompass()
    {
        return persephoneCompass;
    }

    public Image GetPrimaryAbilityIcon(ClientInstance askingCI)
    {
        if (askingCI == playerAtThisComputer)
        {
            return primaryAbilityPlaceholder;
        }
        else
        {
            return null;
        }
    }
    public Image[] GetSecondaryAbilityIcons(ClientInstance askingCI, int numberOfIconsToReturn)
    {
        if (askingCI == playerAtThisComputer)
        {
            Image[] iconsToSend = new Image[numberOfIconsToReturn];


            for (int i = 0; i < numberOfIconsToReturn; i++)
            {
                iconsToSend[i] = secondaryAbilityPlaceholders[i];
            }
            for (int i = numberOfIconsToReturn; i < secondaryAbilityPlaceholders.Length; i++)
            {
                secondaryAbilityPlaceholders[i].enabled = false;
            }
            return iconsToSend;
        }
        else
        {
            return null;
        }
    }

    public Image[] GetSecAbilStatusIcons(ClientInstance askingCI)
    {
        if (askingCI == playerAtThisComputer)
        {
            return secondaryAbilityStatusPlaceholders;
        }
        else
        {
            return null;
        }
    }

    public UpgradePanelUI GetUpgradePanelUI(ClientInstance askingCI)
    {
        if (askingCI == playerAtThisComputer)
        {
            return upgradePanelUI;
        }
        else
        {
            return null;
        }
    }
}
