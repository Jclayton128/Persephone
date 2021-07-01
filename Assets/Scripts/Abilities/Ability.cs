using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(EnergySource))]

public abstract class Ability : NetworkBehaviour, IComparer<Ability>
{
    [SerializeField] public Sprite AbilityIcon = null;
    [SerializeField] int unlockLevel;
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
        if (UsesStatusIcon)
        {
            ToggleAbilityStatusOnUI(true);
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
        return unlockLevel;
    }


    public virtual int Compare(Ability x, Ability y)
    {
        if (x == null || y == null)
        {
            return 0;
        }
        if (x.unlockLevel > y.unlockLevel)
        {
            return 1;
        }
        if (x.unlockLevel < y.unlockLevel)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    [Client]
    protected void ToggleAbilityStatusOnUI(bool shouldBeOn)
    {
        am.ToggleStatusIcon(this, shouldBeOn);
    }




}
