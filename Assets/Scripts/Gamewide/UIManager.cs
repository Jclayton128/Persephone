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
    [SerializeField] Slider throttle = null;
    [SerializeField] TextMeshProUGUI hullMaxTMP = null;
    [SerializeField] TextMeshProUGUI shieldMaxTMP = null;
    [SerializeField] TextMeshProUGUI energyMaxTMP = null;
    [SerializeField] TextMeshProUGUI shieldRateTMP = null;
    [SerializeField] TextMeshProUGUI energyRateTMP = null;


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
                EnergyRateTMP = energyRateTMP
                //Throttle = throttle;
            };
            return uipack;

        }
        else
        {
            return null;
        }

    }

}
