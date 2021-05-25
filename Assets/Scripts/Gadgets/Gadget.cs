using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gadget : NetworkBehaviour
{
    // Start is called before the first frame update

    [SerializeField] public bool IsPrimaryGadget;

    [SerializeField] protected Sprite gadgetSprite = null;
    [SerializeField] protected GameObject weaponPrefab = null;

    [SerializeField] protected float timeBetweenShots;
    [SerializeField] protected float damagePerShot;
    [SerializeField] protected float projectileLifetime;
    [SerializeField] protected float projectileSpeed;

    public abstract void OnClickDown(Vector2 targetPos, Transform currentTransform);


    public abstract void OnClickUp(Vector2 targetPos);


}
