using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIManager : MonoBehaviour
{
    //init
    ClientInstance playerAtThisComputer;

    [SerializeField] Slider healthSlider = null;
    [SerializeField] Slider shieldSlider = null;
    [SerializeField] Slider energySlider = null;
    [SerializeField] Slider throttle = null;

    public void SetLocalPlayerForUI(ClientInstance ci)
    {
        playerAtThisComputer = ci;
    }

    public Slider GetHealthSlider(ClientInstance askingCI)
    {
        if (askingCI == playerAtThisComputer)
        {
            return healthSlider;
        }
        else
        {
            return null;
        }
    }
    public Slider GetShieldSlider(ClientInstance askingCI)
    {
        if (askingCI == playerAtThisComputer)
        {
            return shieldSlider;
        }
        else
        {
            return null;
        }
    }
    public Slider GetEnergySlider(ClientInstance askingCI)
    {
        if (askingCI == playerAtThisComputer)
        {
            return energySlider;
        }
        else
        {
            return null;
        }
    }
}
