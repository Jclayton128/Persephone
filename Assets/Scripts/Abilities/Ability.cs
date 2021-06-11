using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(EnergySource))]

public abstract class Ability : NetworkBehaviour
{
    [SerializeField] protected GameObject[] abilityPrefabs = null;

    [SerializeField] public bool IsPrimaryAbility;

    [SerializeField] protected float timeBetweenShots;
    //[SerializeField] protected float damagePerShot;
    [SerializeField] protected float weaponLifetime;
    [SerializeField] protected float weaponSpeed;
    [SerializeField] protected float hullDamage;
    [SerializeField] protected float shieldDamage;
    [SerializeField] protected float ionDamage;
    [SerializeField] protected float costPerShot;
    [SerializeField] AudioClip insufficientEnergySound = null;

    protected Experience exp;
    protected EnergySource es;

    protected virtual void Awake()
    {
        foreach (GameObject prefab in abilityPrefabs)
        {
            NetworkClient.RegisterPrefab(prefab);
        }
    }

    protected virtual void Start()
    {
        exp = GetComponent<Experience>();
        es = GetComponent<EnergySource>();
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




}
