using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Ability_ReanimatorTorpedo : Ability
{
    Ability_RockMode arm;
    bool hasActiveTorpedo = false;
    Torpedo_AI activeTorpedo;
    [SerializeField] float weaponTurnRate;

    protected override void Start()
    {
        base.Start();
        arm = GetComponent<Ability_RockMode>();
    }

    protected override void MouseClickDownEffect()
    {
        if (arm.CheckFullyDeployed())
        {
            CmdRequestFireWeapon();
            hasActiveTorpedo = true;
            //TODO audio for client
        }

    }

    private void CmdRequestFireWeapon()
    {
        if (es.CheckSpendEnergy(costToActivate) && arm.CheckFullyDeployed())
        {
            GameObject bullet = Instantiate(abilityPrefabs[0], am.SecondaryMuzzle.position, am.SecondaryMuzzle.transform.rotation) as GameObject;
            NetworkServer.Spawn(bullet);
            activeTorpedo = bullet.GetComponent<Torpedo_AI>();
            activeTorpedo.normalSpeed = weaponSpeed;
            activeTorpedo.maxTurnRate = weaponTurnRate;
            bullet.layer = 9;
            bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * weaponSpeed;

            bullet.GetComponent<Torpedo_AI>().SetTargetPoint(MouseHelper.GetMouseCursorLocation());

            DamageDealer dd = bullet.GetComponent<DamageDealer>();
            dd.SetNormalDamage(normalDamage);
            Destroy(bullet, weaponLifetime);
        }
    }

    protected override void MouseClickUpEffect()
    {
        if (hasActiveTorpedo)
        {
            hasActiveTorpedo = false;
            CmdDetonateTorpedo();
        }
    }

    [Command]
    private void CmdDetonateTorpedo()
    {
        activeTorpedo.Detonate();
    }

    private void CmdPassTargetPoint(Vector2 vector2)
    {
        activeTorpedo.SetTargetPoint(vector2);
    }

    private void Update()
    {
        if (isClient && hasActiveTorpedo)
        {
            CmdPassTargetPoint(MouseHelper.GetMouseCursorLocation());
        }
    }
}
