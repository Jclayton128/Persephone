using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class AbilityManager : NetworkBehaviour
{
    Image[] secondaryAbilityIcons;
    UpgradeManager um;
    [SerializeField] public Transform PrimaryMuzzle = null;
    [SerializeField] public Transform SecondaryMuzzle = null;
    [SerializeField] AudioClip invalidSelectionAudioClip = null;
    [SerializeField] Sprite lockedAbilitySprite = null;
    [SerializeField] Sprite statusIcon_Off = null;
    [SerializeField] Sprite statusIcon_On = null;
    Image[] statusIcons;
    Ability_Dummy dummyAbility;

    [SerializeField] List<Ability> allSecondaryAbilities = new List<Ability>();
    [SerializeField] List<Ability> unlockedSecondaryAbilities = new List<Ability>();
    public Ability SelectedSecondaryAbility { get; private set; }
    public Ability PrimaryAbility { get; private set; }

    int secondaryIndex = -1;

    #region Initialization
    private void Start()
    {
        um = GetComponent<UpgradeManager>();
        //um.OnLevelUp += UpdateSecondaryAbilitiesOnLevelUp;
        um.OnLevelUp += TargetRequestClientUpdateSecondaryAbilitiesOnLevelUp;
        um.OnLevelUp += UpdateSecondaryAbilitiesOnLevelUp;


        IdentifyAllAbilities();

        if (isClient && hasAuthority)
        {
            HookIntoLocalUI(allSecondaryAbilities.Count);
            HideAllStatusIcons();
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
                continue;
            }
            else
            {
                allSecondaryAbilities.Add(allAbilities[i]);
            }
        }

        allSecondaryAbilities.Sort(allSecondaryAbilities[0]);

    }
    private void HookIntoLocalUI(int numberOfAbilitiesToPull)
    {
        ClientInstance ci = ClientInstance.ReturnClientInstance();
        UIManager uim = FindObjectOfType<UIManager>();

        uim.GetPrimaryAbilityIcon(ci).sprite = PrimaryAbility.AbilityIcons[0];

        secondaryAbilityIcons = uim.GetSecondaryAbilityIcons(ci, numberOfAbilitiesToPull);
        for (int i = 0; i < numberOfAbilitiesToPull; i++)
        {
            int unusedInt;
            if (allSecondaryAbilities[i].CheckUnlockOnLevelUp(um.GetCurrentLevel(), out unusedInt))
            {
                secondaryAbilityIcons[i].sprite = allSecondaryAbilities[i].AbilityIcons[0];
            }
            else
            {
                secondaryAbilityIcons[i].sprite = lockedAbilitySprite;
            }

        }

        statusIcons = uim.GetSecAbilStatusIcons(ci);
        
    }

    #endregion

    #region Ability Selection
    [Client]
    public void ScrollUpThroughAbilities()
    {
        if (secondaryIndex == -1)
        {
            Debug.Log("still no unlocked abilities");         
        }

        else
        {
            secondaryIndex++;
            if (secondaryIndex > unlockedSecondaryAbilities.Count -1)
            {
                secondaryIndex = 0;
            }
        }

        SelectedSecondaryAbility = unlockedSecondaryAbilities[secondaryIndex];
        CmdSetSecondaryAbility(secondaryIndex);
        UpdateSelectionUI();
    }


    [Client]
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
        CmdSetSecondaryAbility(secondaryIndex);
        UpdateSelectionUI();

    }

    [Command]
    private void CmdSetSecondaryAbility(int index)
    {
        secondaryIndex = index;
        //SelectedSecondaryAbility = unlockedSecondaryAbilities[secondaryIndex];
    }
    #endregion

    [TargetRpc]
    public void TargetRequestClientUpdateSecondaryAbilitiesOnLevelUp(int newLevel)
    {
        UpdateSecondaryAbilitiesOnLevelUpOnClient(newLevel);
    }


    private void UpdateSecondaryAbilitiesOnLevelUp(int newLevel)
    {
        foreach (Ability ability in allSecondaryAbilities)
        {
            int abilityTier;

            if (ability.CheckUnlockOnLevelUp(newLevel, out abilityTier))
            {
                Debug.Log("abilityTier passed out: " + abilityTier + "is client: " + isClient);
                int secondaryToUnlock = allSecondaryAbilities.IndexOf(ability);
                if (!unlockedSecondaryAbilities.Contains(ability))
                {
                    unlockedSecondaryAbilities.Add(ability);
                }
                if (isClient && hasAuthority && abilityTier == 0)
                {
                    Debug.Log("unlock found");
                    //Debug.Log($"icon list length: {secondaryAbilityIcons.Length} vs index: {secondaryToUnlock}. AllSecAbLeng: {allSecondaryAbilities.Count}");
                    secondaryAbilityIcons[secondaryToUnlock].sprite = allSecondaryAbilities[secondaryToUnlock].AbilityIcons[0];
                    if (ability.UsesStatusIcon)
                    {
                        statusIcons[secondaryToUnlock].enabled = true;
                    }
                }
                if (isClient && hasAuthority && abilityTier > 0)
                {
                    Debug.Log("upgrade found, should upgrade the icon");
                    UpdateUpgradedAbilityIcon(secondaryToUnlock, abilityTier);
                }
                else
                {
                    Debug.Log("ability remains locked");
                }

                if (secondaryIndex == -1)
                {
                    secondaryIndex = 0;
                    SelectedSecondaryAbility = unlockedSecondaryAbilities[secondaryIndex];
                    CmdSetSecondaryAbility(secondaryIndex);
                    UpdateSelectionUI();

                }
            }
        }

    }

    private void UpdateSecondaryAbilitiesOnLevelUpOnClient(int newLevel)
    {

    }

    private void UpdateUpgradedAbilityIcon(int index, int tier)
    {
        Sprite upgradedAbilityIcon = allSecondaryAbilities[index].AbilityIcons[tier];
        secondaryAbilityIcons[index].sprite = upgradedAbilityIcon;
    }


    #region routine UI changes
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
    private void HideAllStatusIcons()
    {
        foreach(Image statusIcon in statusIcons)
        {
            statusIcon.enabled = false;
        }
    }

    public void ToggleStatusIcon(Ability askingAbility, bool shouldBeOn)
    {
        if (!hasAuthority) { return; }
        int index = allSecondaryAbilities.IndexOf(askingAbility);
        Image status = statusIcons[index];

        if (shouldBeOn)
        {
            status.sprite = statusIcon_On;
        }
        if (!shouldBeOn)
        {
            status.sprite = statusIcon_Off;
        }
        // replace status icon with correct one.
    }

    #endregion

}
