using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class AbilityManager : NetworkBehaviour
{
    [SerializeField] Image[] secondaryAbilityIcons;
    UpgradeManager um;
    [SerializeField] AudioClip invalidSelectionAudioClip = null;
    [SerializeField] Sprite lockedAbilitySprite = null;
    Ability_Dummy dummyAbility;

    [SerializeField] List<Ability> secondaryAbilities = new List<Ability>();
    [SerializeField] List<Ability> unlockedSecondaryAbilities = new List<Ability>();
    public Ability SelectedSecondaryAbility { get; private set; }
    public Ability PrimaryAbility { get; private set; }

    int secondaryIndex = 0;

    private void Start()
    {
        um = GetComponent<UpgradeManager>();
        um.OnLevelUp += UpdateSecondaryAbilitiesOnLevelUp;
        
        IdentifyAllAbilities();

        if (hasAuthority)
        {
            HookIntoLocalUI(secondaryAbilities.Count);

            UpdateSecondaryAbilitiesOnLevelUp(1); // Hard 1 because everyone starts on level 1
            UpdateSelectionUI();
        }

        if (unlockedSecondaryAbilities.Count == 0)
        {
            secondaryIndex = -1;
            dummyAbility = gameObject.AddComponent<Ability_Dummy>();
            dummyAbility.dummyAbilityAttemptedAudioClip = invalidSelectionAudioClip;
            SelectedSecondaryAbility = dummyAbility;
        }

    }


    private void IdentifyAllAbilities()
    {
        Ability[] allAbilities = GetComponents<Ability>();

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

        secondaryAbilities.Sort(secondaryAbilities[0]);

    }
    private void HookIntoLocalUI(int numberOfAbilitiesToPull)
    {
        ClientInstance ci = ClientInstance.ReturnClientInstance();
        UIManager uim = FindObjectOfType<UIManager>();

        uim.GetPrimaryAbilityIcon(ci).sprite = PrimaryAbility.AbilityIcon;

        secondaryAbilityIcons = uim.GetSecondaryAbilityIcons(ci, numberOfAbilitiesToPull);
        for (int i = 0; i < numberOfAbilitiesToPull; i++)
        {
            if (secondaryAbilities[i].GetUnlockLevel() <= um.CurrentLevel)
            {
                secondaryAbilityIcons[i].sprite = secondaryAbilities[i].AbilityIcon;
            }
            else
            {
                secondaryAbilityIcons[i].sprite = lockedAbilitySprite;
            }

        }
        
    }

    public void ScrollUpThruAbilities()
    {
        if (secondaryIndex == -1)
        {
            Debug.Log("still no unlocked abilities");         
        }

        else
        {
            secondaryIndex++;
            if (secondaryIndex >= unlockedSecondaryAbilities.Count -1)
            {
                secondaryIndex = 0;
            }
        }

        SelectedSecondaryAbility = unlockedSecondaryAbilities[secondaryIndex];
        UpdateSelectionUI();
    }

    public void ScrollDownThroughAbilities()
    {
        if (secondaryIndex == -1)
        {
            Debug.Log("still no unlocked abilities");
        }
        else
        {
            secondaryIndex--;
            if (secondaryIndex < 0)
            {
                secondaryIndex = unlockedSecondaryAbilities.Count-1;
            }
        }

        SelectedSecondaryAbility = unlockedSecondaryAbilities[secondaryIndex];
        UpdateSelectionUI();

    }

    private void UpdateSecondaryAbilitiesOnLevelUp(int newLevel)
    {
        foreach (Ability ability in secondaryAbilities)
        {
            if (newLevel >= ability.GetUnlockLevel())
            {
                int secondaryToUnlock = secondaryAbilities.IndexOf(ability);
                unlockedSecondaryAbilities.Add(ability);
                if (isClient)
                {
                    secondaryAbilityIcons[secondaryToUnlock].sprite = secondaryAbilities[secondaryToUnlock].AbilityIcon;
                }

                if (secondaryIndex == -1)
                {
                    secondaryIndex = 0;
                    UpdateSelectionUI();
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
        for (int i = 0; i < secondaryAbilityIcons.Length; i++)
        {
            secondaryAbilityIcons[i].color = Color.grey;
        }
    }

    private void HighlightSelectedUIAbility()
    {
        secondaryAbilityIcons[secondaryIndex].color = Color.white;
    }
}
