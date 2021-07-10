using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Ability_MarkerTurret : Ability
{
    [SerializeField] GameObject turret;
    [SerializeField] Transform muzzle;
    [SerializeField] float turretTurnRate;
    [SerializeField] float ionizationDamage;
    PlayerInput plin;

    bool isFiring = false;

    protected override void Start()
    {
        base.Start();
        plin = GetComponent<PlayerInput>();
    }


    protected override void MouseClickDownEffect()
    {
        CmdRequestWeaponFire();
        //TODO audio on clientside
    }

    [Command]
    private void CmdRequestWeaponFire()
    {
        if (es.CheckEnergy(costToActivate))
        {
            isFiring = true;
        }
    }


    protected override void MouseClickUpEffect()
    {
        CmdRequestCeaseFire();
    }

    [Command]
    private void CmdRequestCeaseFire()
    {
        isFiring = false;
    }

    void Update()
    {
        if (!plin.GetDisabledStatus())
        {
            PointAtMousePosition();
        }

        if (isServer)
        {
            HandleFiring();
        }
    }

    [Server]
    private void HandleFiring()
    {
        if (isFiring && Time.time >= timeOfNextShot)
        {
            if (!es.CheckSpendEnergy(costToActivate))
            {
                isFiring = false;
                return;
            }
            else
            {
                //TODO ClientRPC a shot sound

                GameObject bullet = Instantiate(abilityPrefabs[0], muzzle.position, muzzle.rotation) as GameObject;
                bullet.layer = 9;

                bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * weaponSpeed;

                DamageDealer dd = bullet.GetComponent<DamageDealer>();
                dd.SetNormalDamage(normalDamage);
                dd.SetIonization(ionizationDamage);

                Destroy(bullet, weaponLifetime);
                NetworkServer.Spawn(bullet);
                timeOfNextShot = Time.time + timeBetweenShots;
            }

        }
    }

    private void PointAtMousePosition()
    {
        Vector3 target = MouseHelper.GetMouseCursorLocation();
        Vector3 targetDir = target - turret.transform.position;          
        float angleToTargetFromNorth = Vector3.SignedAngle(targetDir, Vector2.up, transform.forward);
        Quaternion angleToPoint = Quaternion.Euler(0, 0, -1 * angleToTargetFromNorth);
        turret.transform.rotation = Quaternion.RotateTowards(turret.transform.rotation, angleToPoint, turretTurnRate * Time.deltaTime);
    }
}
