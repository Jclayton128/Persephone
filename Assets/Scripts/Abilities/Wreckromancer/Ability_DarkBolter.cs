using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Ability_DarkBolter : Ability
{
    [SerializeField] float ionizationDamage;
    Ability_RockMode arm;

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
        }

        //TODO audio firing sound
    }

    [Command]
    private void CmdRequestFireWeapon()
    {
        if (es.CheckSpendEnergy(costToActivate) && arm.CheckFullyDeployed())
        {
            GameObject bullet = Instantiate(abilityPrefabs[0], am.PrimaryMuzzle.position, am.PrimaryMuzzle.transform.rotation) as GameObject;
            NetworkServer.Spawn(bullet);
            bullet.layer = 9;
            bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * weaponSpeed;

            DamageDealer dd = bullet.GetComponent<DamageDealer>();
            dd.SetNormalDamage(normalDamage);
            dd.SetShieldBonusDamage(shieldBonusDamage);
            dd.SetIonization(ionizationDamage);
            Destroy(bullet, weaponLifetime);
        }
    }

    protected override void MouseClickUpEffect()
    {

    }


}
