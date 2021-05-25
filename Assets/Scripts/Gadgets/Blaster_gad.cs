using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Blaster_gad : Gadget
{
    //WeaponEmitter we;

    public override void OnClickDown(Vector2 targetPos, Transform currentTransform)
    {
        //GameObject bullet = Instantiate(weaponPrefab, currentTransform.position, currentTransform.rotation) as GameObject;
        //bullet.GetComponent<Rigidbody2D>().velocity = projectileSpeed * bullet.transform.up;
        //Destroy(bullet, projectileLifetime);

    }

    public override void OnClickUp(Vector2 targetPos)
    {
        
    }


}
