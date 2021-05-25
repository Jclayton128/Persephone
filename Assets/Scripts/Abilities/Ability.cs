using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : NetworkBehaviour
{
    [SerializeField] protected GameObject weaponPrefab = null;

    [SerializeField] public bool IsPrimaryAbility;

    [SerializeField] protected float timeBetweenShots;
    [SerializeField] protected float damagePerShot;
    [SerializeField] protected float weaponLifetime;
    [SerializeField] protected float weaponSpeed;
    [SerializeField] protected float hullDamage;
    [SerializeField] protected float shieldDamage;
    [SerializeField] protected float ionDamage;
    [SerializeField] protected float costPerShot;

    Experience exp;

    protected virtual void Start()
    {
        exp = GetComponent<Experience>();
    }

    public abstract void MouseClickDown();

    public abstract void MouseClickUp();




}
