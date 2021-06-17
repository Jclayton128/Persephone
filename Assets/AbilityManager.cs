using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class AbilityManager : NetworkBehaviour
{
    Image[] abilityIcons;
    UpgradeManager um;
    [SerializeField] AudioClip invalidSelectionAudioClip = null;
    [SerializeField] Sprite lockedAbilitySprite = null;
    Ability_Dummy dummyAbility;

    List<Ability> secondaryAbilities = new List<Ability>();
    List<Ability> unlockedSecondaryAbilities = new List<Ability>();
    public Ability SelectedSecondaryAbility { get; private set; }
    public Ability PrimaryAbility { get; private set; }

    int selectedUnlockedSecondaryAbilityIndex = 0;

    private void Start()
    {
        um = GetComponent<UpgradeManager>();
        
        PrepAllAbilities();
        CheckUnlockNewAbility();

        if (hasAuthority)
        {
            HookIntoLocalUI(secondaryAbilities.Count + 1);
            DarkenAllSecondaryAbilities();
           
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
        selectedUnlockedSecondaryAbilityIndex = -1;
        dummyAbility = gameObject.AddComponent<Ability_Dummy>();
        dummyAbility.dummyAbilityAttemptedAudioClip = invalidSelectionAudioClip;
        SelectedSecondaryAbility = dummyAbility;

    }
    private void HookIntoLocalUI(int numberOfAbilitiesToPull)
    {
        ClientInstance ci = ClientInstance.ReturnClientInstance();
        UIManager uim = FindObjectOfType<UIManager>();
        abilityIcons = uim.GetAbilityIcons(ci, numberOfAbilitiesToPull);

        abilityIcons[0].sprite = PrimaryAbility.AbilityIcon;
        for(int i = 1; i < numberOfAbilitiesToPull; i++)
        {
            if (secondaryAbilities[i - 1].GetUnlockLevel() <= um.CurrentLevel)
            {
                abilityIcons[i].sprite = secondaryAbilities[i - 1].AbilityIcon;
            }
            else
            {
                abilityIcons[i].sprite = lockedAbilitySprite;
            }

        }
        
    }


    private void UpdateUI()
    {
        // This should do whatever highlighting or icon work is required to show what the currently selected secondary skill is.
    }

    public void ScrollUpThruAbilities()
    {
        CheckUnlockNewAbility();
        if (selectedUnlockedSecondaryAbilityIndex == -1)
        {
            Debug.Log("still no unlocked abilities");         
        }

        else
        {
            selectedUnlockedSecondaryAbilityIndex++;
            if (selectedUnlockedSecondaryAbilityIndex > unlockedSecondaryAbilities.Count - 1)
            {
                selectedUnlockedSecondaryAbilityIndex = 0;
            }
        }

        SelectedSecondaryAbility = unlockedSecondaryAbilities[selectedUnlockedSecondaryAbilityIndex];
        UpdateSelectionUI();
    }



    public void ScrollDownThroughAbilities()
    {
        CheckUnlockNewAbility();
        if (selectedUnlockedSecondaryAbilityIndex == -1)
        {
            Debug.Log("still no unlocked abilities");
        }
        else
        {
            selectedUnlockedSecondaryAbilityIndex--;
            if (selectedUnlockedSecondaryAbilityIndex <= 0)
            {
                selectedUnlockedSecondaryAbilityIndex = unlockedSecondaryAbilities.Count - 1;
            }
        }

        SelectedSecondaryAbility = unlockedSecondaryAbilities[selectedUnlockedSecondaryAbilityIndex];
        UpdateSelectionUI();

    }

    public void CheckUnlockNewAbility()
    {
        foreach (Ability ability in secondaryAbilities)
        {
            if (ability.GetUnlockLevel() <= um.CurrentLevel)
            {
                int secondaryToUnlock = secondaryAbilities.IndexOf(ability);
                //if (isClient)
                //{
                //    abilityIcons[secondaryToUnlock + 1].sprite = ability.AbilityIcon;
                //}
                unlockedSecondaryAbilities.Add(ability);

                if (selectedUnlockedSecondaryAbilityIndex == -1)
                {
                    selectedUnlockedSecondaryAbilityIndex = 0;
                }
            }
            
        }

    }

    private void UpdateSelectionUI()
    {
        DarkenAllSecondaryAbilities();
        HighlightSelectedUIAbility();
    }

    private void DarkenAllSecondaryAbilities()
    {
        for (int i = 1; i < abilityIcons.Length; i++)
        {
            abilityIcons[i].color = Color.grey;
        }
    }

    private void HighlightSelectedUIAbility()
    {
        abilityIcons[selectedUnlockedSecondaryAbilityIndex + 1].color = Color.white;
    }
}
