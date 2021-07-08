using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Ability_DarkBlade : Ability
{
    [SerializeField] float ionizationDamage;
    Ability_Blink abbl;
    Rigidbody2D rb;

    public override void OnStartServer()
    {
        base.OnStartServer();
        abbl = GetComponent<Ability_Blink>();
        rb = GetComponent<Rigidbody2D>();
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
            priBlade.GetComponent<Rigidbody2D>().velocity = priBlade.transform.up * (weaponSpeed + rb.velocity.magnitude);

            DamageDealer dd = priBlade.GetComponent<DamageDealer>();
            dd.SetNormalDamage(normalDamage);
            dd.SetIonization(ionizationDamage);
            dd.UsesDirectionalBonus = true;
            Destroy(priBlade, weaponLifetime);
        }
    }

    protected override void MouseClickUpEffect()
    {

    }

}
