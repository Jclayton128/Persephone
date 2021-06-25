using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Ability_Blaster : Ability
{
    protected override void MouseClickDownEffect()
    {
        CmdRequestFireWeapon();
    }

    protected override void MouseClickUpEffect()
    {
        
    }

    [Command]
    private void CmdRequestFireWeapon()
    {
        if (es.CheckSpendEnergy(costPerShot))
        {
            GameObject bullet = Instantiate(abilityPrefabs[0], transform.position, transform.rotation) as GameObject;
            NetworkServer.Spawn(bullet);
            bullet.layer = 9;
            bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * weaponSpeed;
            DamageDealer dd = bullet.GetComponent<DamageDealer>();
            dd.SetRegularDamage(hullDamage);
            dd.SetShieldBonusDamage(shieldDamage);
            dd.SetIonization(ionDamage);
            //dd.IsReal = true;
            Destroy(bullet, weaponLifetime);
        }
        else
        {

        }

    }
}
