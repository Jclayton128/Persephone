﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Ability_ArcherTurret : Ability
{
    [SerializeField] GameObject turret;
    [SerializeField] Transform muzzle;
    [SerializeField] float turretTurnRate;
    [SerializeField] float knockback;

    PlayerInput plin;
    GameObject chargingBullet;
    Vector3 targetPos;
    bool isCharging = false;
    float chargeRate = 0.33f;
    int primaryLayerToTarget = 10;
    int secondaryLayerToTarget = 18;
    float range;

    [SyncVar (hook = nameof(UpdateChargingWeaponGraphic))]
    float chargingFactor;

    protected override void Start()
    {
        base.Start();
        range = weaponLifetime * weaponSpeed;
        plin = GetComponent<PlayerInput>();
        am.ToggleStatusIcon(this, false);
        
    }

    protected override void MouseClickDownEffect()
    {
        CmdRequestBeginCharging();
        //TODO audio: a steadily-building hum or whine as the shot charges up.
    }

    [Command]
    private void CmdRequestBeginCharging()
    {
        if (es.CheckEnergy(costToActivate))
        {
            isCharging = true;
        }
    }


    protected override void MouseClickUpEffect()
    {
        CmdRequestFireChargedWeapon();
    }

    [Command]
    private void CmdRequestFireChargedWeapon()
    {
        FireWeapon();
    }

    [Server]
    private void FireWeapon()
    {
        if (chargingFactor > 0.2f)
        {
            
            chargingBullet.layer = 9;

            chargingBullet.GetComponent<Rigidbody2D>().velocity = chargingBullet.transform.up * weaponSpeed;

            DamageDealer dd = chargingBullet.GetComponent<DamageDealer>();
            dd.SetNormalDamage(normalDamage * chargingFactor);
            dd.SetKnockback(knockback * chargingFactor);

            Destroy(chargingBullet, weaponLifetime);
            chargingBullet = null;
        }
        else
        {
            Destroy(chargingBullet);   
        }

        isCharging = false;
        chargingFactor = 0;
        am.ToggleStatusIcon(this, false);
    }

    void Update()
    {
        if (!plin.IsDisabled)
        {
            FindBestTargetPos();
            PointAtTargetPos();
        }

        if (isServer)
        {
            HandleCharging();
        }
    }


    [Server]
    private void HandleCharging()
    {
        if (isCharging)
        {
            if (!es.CheckSpendEnergy(costToActivate * Time.deltaTime))
            {
                FireWeapon();
                return;
            }
            else
            {
                if (chargingBullet == null)
                {
                    chargingBullet = Instantiate(abilityPrefabs[0], muzzle.position, muzzle.rotation) as GameObject;
                    NetworkServer.Spawn(chargingBullet);
                }
                chargingBullet.transform.position = muzzle.position;
                chargingBullet.transform.rotation = muzzle.rotation;

                chargingFactor += Time.deltaTime * chargeRate;
                chargingFactor = Mathf.Clamp01(chargingFactor);
                if (chargingFactor >= 0.9f)
                {
                    am.ToggleStatusIcon(this, true);
                }
            }

        }
    }

    private void UpdateChargingWeaponGraphic(float v1, float v2)
    {
        if (chargingBullet)
        {
            chargingBullet.transform.localScale = Vector3.one * chargingFactor;
        }
    }

    private void FindBestTargetPos()
    {
        int layerMask = (1 << primaryLayerToTarget) | (1 << secondaryLayerToTarget);
        GameObject target = CUR.GetNearestGameObjectOnLayer(transform, layerMask, range);
        if (!target)
        {
            targetPos = turret.transform.position + transform.up;
        }
        else
        {
            Vector3 enemyVel = target.GetComponent<Rigidbody2D>().velocity;
            float timeOfShot = ((target.transform.position + enemyVel) - transform.position).magnitude / weaponSpeed;
            targetPos = target.transform.position + (enemyVel * timeOfShot);
            Debug.DrawLine(turret.transform.position, target.transform.position, Color.blue);
        }
    }

    private void PointAtTargetPos()
    {
        Vector3 targetDir = targetPos - turret.transform.position;
        float angleToTargetFromNorth = Vector3.SignedAngle(targetDir, Vector2.up, transform.forward);
        Quaternion angleToPoint = Quaternion.Euler(0, 0, -1 * angleToTargetFromNorth);
        turret.transform.rotation = Quaternion.RotateTowards(turret.transform.rotation, angleToPoint, turretTurnRate * Time.deltaTime);
    }
}