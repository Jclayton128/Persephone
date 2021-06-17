using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class AbilityManager : NetworkBehaviour
{
    Image[] abilityIcons;

    List<Ability> secondaryAbilities = new List<Ability>();
    public Ability SelectedSecondaryAbility { get; private set; }
    public Ability PrimaryAbility { get; private set; }

    int selectedSecondaryAbilityIndex = 0;
    Dictionary<int, int> unlockLevels = new Dictionary<int, int>();

    private void Start()
    {
        PrepAllAbilities();
        if (hasAuthority)
        {
            HookIntoLocalUI(secondaryAbilities.Count + 1);
        }
    }

    private void PrepAllAbilities()
    {
        Ability[] allAbilities = GetComponents<Ability>();
        Debug.Log($" found this many abilities: {allAbilities.Length}");
        for (int i = 0; i < allAbilities.Length; i++)
        {
            if (allAbilities[i].IsPrimaryAbility)
            {
                PrimaryAbility = allAbilities[i];
            }
            else
            {
                secondaryAbilities.Add(allAbilities[i]);
            }
        }
        selectedSecondaryAbilityIndex = 0;
        SelectedSecondaryAbility = secondaryAbilities[selectedSecondaryAbilityIndex];

    }
    private void HookIntoLocalUI(int numberOfAbilitiesToPull)
    {
        ClientInstance ci = ClientInstance.ReturnClientInstance();
        UIManager uim = FindObjectOfType<UIManager>();
        abilityIcons = uim.GetAbilityIcons(ci, numberOfAbilitiesToPull);

        abilityIcons[0].sprite = PrimaryAbility.AbilityIcon;
        for(int i = 1; i < numberOfAbilitiesToPull; i++)
        {
            abilityIcons[i].sprite = secondaryAbilities[i - 1].AbilityIcon;
        }
        
    }


    private void UpdateUI()
    {
        // This should do whatever highlighting or icon work is required to show what the currently selected secondary skill is.
    }

    public void ScrollUpThruAbilities()
    {
        // THis should check that the other abilities are unlocked by level.
    }

    public void ScrollDownThroughAbilities()
    {

    }

    public void CheckUnlockNewAbility(int newLevel)
    {
        // use this to unlock new abilities upon gaining a level.
    }
}
