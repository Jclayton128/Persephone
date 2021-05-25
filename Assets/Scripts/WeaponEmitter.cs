using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponEmitter : NetworkBehaviour
{    

    public void EmitWeapon()
    {

    }

    [Command]
    public void CmdRequestEmitWeapon(GameObject weapon, Vector3 position, Quaternion rotation, float weaponSpeed, float weaponLifetime)
    {
        GameObject bullet = Instantiate(weapon, position, rotation) as GameObject;
        bullet.GetComponent<Rigidbody2D>().velocity = weaponSpeed * bullet.transform.up;
        NetworkServer.Spawn(bullet);
        Destroy(bullet, weaponLifetime);
    }
}
