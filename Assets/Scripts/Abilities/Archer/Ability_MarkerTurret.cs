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
    [SerializeField] Vector3 targetPos;
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
        if (hasAuthority && !plin.GetDisabledStatus())
        {
            UpdateTargetPosition();
        }

        if (isServer)
        {
            RotateTurretTowardsTargetPos();
            HandleFiring();
        }
    }

    [Server]
    private void RotateTurretTowardsTargetPos()
    {
        Vector3 targetDir = targetPos - turret.transform.position;
        float angleToTargetFromNorth = Vector3.SignedAngle(targetDir, Vector2.up, transform.forward);
        Quaternion angleToPoint = Quaternion.Euler(0, 0, -1 * angleToTargetFromNorth);
        turret.transform.rotation = Quaternion.RotateTowards(turret.transform.rotation, angleToPoint, turretTurnRate * Time.deltaTime);
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

                bullet.GetComponent<Rigidbody2D>().velocity = (bullet.transform.up * weaponSpeed) + (Vector3)avatarRB.velocity;

                DamageDealer dd = bullet.GetComponent<DamageDealer>();
                dd.SetNormalDamage(normalDamage);
                dd.SetIonization(ionizationDamage);

                Destroy(bullet, weaponLifetime);
                NetworkServer.Spawn(bullet);
                timeOfNextShot = Time.time + timeBetweenShots;
            }

        }
    }


    private void UpdateTargetPosition()
    {
        targetPos = MouseHelper.GetMouseCursorLocation();
        CmdPushNewTargetPos(targetPos);
    }

    [Command]
    private void CmdPushNewTargetPos(Vector3 newPos)
    {
        targetPos = newPos;
    }
}
