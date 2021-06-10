using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Ability_Blaster : Ability
{
    public override void MouseClickDown()
    {
        CmdRequestFireWeapon();
    }

    public override void MouseClickUp()
    {
        
    }

    [Command]
    private void CmdRequestFireWeapon()
    {
        GameObject bullet = Instantiate(abilityPrefabs[0], transform.position, transform.rotation) as GameObject;
        NetworkServer.Spawn(bullet);
        bullet.layer = 9;
        bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * weaponSpeed;
        DamageDealer dd = bullet.GetComponent<DamageDealer>();
        dd.SetDamage(hullDamage);
        dd.SetDamage(shieldDamage);
        dd.IsReal = true;
        Destroy(bullet, weaponLifetime);
    }
}
