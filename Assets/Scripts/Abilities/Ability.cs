using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(EnergySource))]

public abstract class Ability : NetworkBehaviour, IComparer<Ability>
{
    [SerializeField] public Sprite[] AbilityIcons = null;
    [SerializeField] protected int[] unlockLevels;
    [SerializeField] protected GameObject[] abilityPrefabs = null;

    [SerializeField] public bool IsPrimaryAbility;

    [SerializeField] protected float timeBetweenShots;
    [SerializeField] protected float weaponLifetime;
    [SerializeField] protected float weaponSpeed;
    [SerializeField] protected float normalDamage;
    [SerializeField] protected float shieldBonusDamage;
    [SerializeField] protected float costToActivate;
    [SerializeField] AudioClip insufficientEnergySound = null;
    [SerializeField] public bool UsesStatusIcon;
    

    protected float timeOfNextShot;
    protected EnergySource es;
    protected AbilityManager am;
    protected Rigidbody2D avatarRB;


    protected virtual void Awake()
    {
        foreach (GameObject prefab in abilityPrefabs)
        {
            if (!NetworkClient.prefabs.ContainsValue(prefab))
            {
                NetworkClient.RegisterPrefab(prefab);
            }
        }
    }

    protected virtual void Start()
    {
        es = GetComponent<EnergySource>();
        am = GetComponent<AbilityManager>();
        avatarRB = GetComponent<Rigidbody2D>();
        if (UsesStatusIcon)
        {
            ToggleAbilityStatusOnUI(true);
        }
        if(unlockLevels.Length == 0)
        {
            unlockLevels = new int[1];
        }
    }

    public virtual void MouseClickDownValidate()
    {
        if (es.CheckEnergy(costToActivate))  
        {
            MouseClickDownEffect();
        }
        else
        {
            //TODO play insufficient power sound;
        }
    }
    protected abstract void MouseClickDownEffect();

    public virtual void MouseClickUpValidate()
    {
        MouseClickUpEffect();
    }
    protected abstract void MouseClickUpEffect();

    public int GetUnlockLevel()
    {
        return unlockLevels[0];
    }

    public virtual bool CheckUnlockOnLevelUp(int newLevel, out int abilityTier)
    {

        if (newLevel >= unlockLevels[0])
        {
            abilityTier = 0;
            return true;
        }
        else
        {
            abilityTier = -1;
            return false;
        }
    }

    public virtual int Compare(Ability x, Ability y)
    {
        if (x == null || y == null)
        {
            return 0;
        }
        if (x.unlockLevels[0] > y.unlockLevels[0])
        {
            return 1;
        }
        if (x.unlockLevels[0] < y.unlockLevels[0])
        {
            return -1;
        }
        if (x.unlockLevels[0] == y.unlockLevels[0])
        {
            return 0;
        }
        else
        {
            return 0;
        }
    }

    [Client]
    protected void ToggleAbilityStatusOnUI(bool shouldBeOn)
    {
        if (hasAuthority)
        {
            am.ToggleStatusIcon(this, shouldBeOn);
        }
    }
    #region Generic Upgrades
    public virtual void ModifyCount(int amount)
    {
        //Increase weapon count
    }
    public virtual void ModifyNormalDamage(float amount)
    {
        //Increase weapon normal damage
    }
    public virtual void ModifyIonization(float amount)
    {
        //Increase weapon ionization
    }
    public virtual void ModifyRangeViaSpeedOrLifetime(float amount)
    {
        //Increase weapon normal damage
    }


    #endregion





}
