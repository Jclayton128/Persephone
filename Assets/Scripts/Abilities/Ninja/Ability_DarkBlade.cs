using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Ability_DarkBlade : Ability
{
    [SerializeField] float ionizationDamage;
    Ability_Blink abbl;

    public override void OnStartServer()
    {
        base.OnStartServer();
        abbl = GetComponent<Ability_Blink>();
    }
    protected override void MouseClickDownEffect()
    {
        CmdRequestSpawnDarkBlade();
    }

    private void CmdRequestSpawnDarkBlade()
    {
        if (es.CheckSpendEnergy(costToActivate) && !abbl.IsBlinking)
        {
            GameObject priBlade = Instantiate(abilityPrefabs[0], am.PrimaryMuzzle.position, am.PrimaryMuzzle.rotation) as GameObject;
            NetworkServer.Spawn(priBlade);
            priBlade.layer = 9;
            priBlade.GetComponent<Rigidbody2D>().velocity = priBlade.transform.up * weaponSpeed;

            DamageDealer dd = priBlade.GetComponent<DamageDealer>();
            dd.SetNormalDamage(normalDamage);
            dd.SetShieldBonusDamage(shieldBonusDamage);
            dd.SetIonization(ionizationDamage);

            Destroy(priBlade, weaponLifetime);


            GameObject secBlade = Instantiate(abilityPrefabs[0], am.PrimaryMuzzle.position, am.PrimaryMuzzle.rotation) as GameObject;
            NetworkServer.Spawn(secBlade);
            secBlade.layer = 9;
            secBlade.GetComponent<Rigidbody2D>().velocity = secBlade.transform.up * weaponSpeed;

            DamageDealer dd2 = secBlade.GetComponent<DamageDealer>();
            dd2.SetNormalDamage(normalDamage);
            dd2.SetShieldBonusDamage(shieldBonusDamage);
            dd2.SetIonization(ionizationDamage);

            Destroy(secBlade, weaponLifetime);

        }
    }

    protected override void MouseClickUpEffect()
    {

    }

}
