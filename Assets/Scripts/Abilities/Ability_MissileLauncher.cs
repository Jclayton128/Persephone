using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Ability_MissileLauncher : Ability
{
    //init
    [SerializeField] AudioClip missileFiringSound = null;
    PersNetworkManager pnm;

    protected override void Start()
    {
        base.Start();
        if (isServer)
        {
            pnm = FindObjectOfType<PersNetworkManager>();
        }
    }
    private Vector2 GetMouseCursorLocation()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
        Vector2 flatPos = worldPosition;
        return flatPos;
    }
    protected override void MouseClickDownEffect()
    {
         CmdRequestFireMissile();
    }


    [Command]
    private void CmdRequestFireMissile()
    {
        if (es.CheckDrainEnergy(costPerShot))
        {
            FireMissile();
        }

    }

    [Server]
    private void FireMissile()
    {
        GameObject missile = Instantiate(abilityPrefabs[0], transform.position, transform.rotation) as GameObject;
        missile.layer = 9;
        Missile_AI missileAI = missile.GetComponent<Missile_AI>();
        missileAI.SetNavTarget(GetMouseCursorLocation());
        DamageDealer dd = missile.GetComponent<DamageDealer>();
        dd.SetDamage(hullDamage);
        dd.SetSafeObject(gameObject);
        dd.IsReal = true;
        missileAI.normalSpeed = weaponSpeed;
        missileAI.SetMissileOwner(gameObject);
        missileAI.SetLifetime(weaponLifetime);

        // TODO: AudioSource.PlayClipAtPoint(missileFiringSound, gameObject.transform.position);
        NetworkServer.Spawn(missile);
    }

    protected override void MouseClickUpEffect()
    {

    }
}
