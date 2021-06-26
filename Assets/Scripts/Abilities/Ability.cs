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
    //[SerializeField] protected float damagePerShot;
    [SerializeField] protected float weaponLifetime;
    [SerializeField] protected float weaponSpeed;
    [SerializeField] protected float normalDamage;
    [SerializeField] protected float shieldBonusDamage;
    [SerializeField] protected float costPerShot;
    [SerializeField] AudioClip insufficientEnergySound = null;


    protected EnergySource es;
    protected AbilityManager am;


    protected virtual void Awake()
    {
        foreach (GameObject prefab in abilityPrefabs)
        {
            NetworkClient.RegisterPrefab(prefab);
        }
    }

    protected virtual void Start()
    {
        es = GetComponent<EnergySource>();
        am = GetComponent<AbilityManager>();
    }

    public virtual void MouseClickDownValidate()
    {
        if (es.CheckEnergy(costPerShot))
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




}
