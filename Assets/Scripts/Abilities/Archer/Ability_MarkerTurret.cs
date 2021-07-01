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

    protected override void MouseClickDownEffect()
    {
        CmdRequestWeaponFire();
        //TODO audio on clientside
    }

    private void CmdRequestWeaponFire()
    {
        throw new NotImplementedException();
    }

    [Command]
    protected override void MouseClickUpEffect()
    {
        if (es.CheckSpendEnergy(costToActivate))
        {

        }
    }

    void Update()
    {
        PointAtMousePosition();
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
