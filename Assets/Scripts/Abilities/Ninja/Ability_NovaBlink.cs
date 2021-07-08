using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Ability_NovaBlink : Ability_Blink
{
    int shrapnelCount = 24;
    [SerializeField] float ionizationDamage = 5;

    protected override void Blink()
    {
        base.Blink();
        float circleSubdivided = 360 / shrapnelCount;
        for (int i = 1; i <= shrapnelCount; i++)
        {
            Quaternion sector = Quaternion.Euler(0, 0, i * circleSubdivided + transform.eulerAngles.z + (weaponSpeed / 2) + 180);
            GameObject newShrapnel = Instantiate(abilityPrefabs[1], transform.position, sector) as GameObject;
            newShrapnel.layer = 9;
            newShrapnel.transform.localScale = Vector3.one * 0.5f;
            newShrapnel.GetComponent<Rigidbody2D>().velocity = newShrapnel.transform.up * weaponSpeed;
            newShrapnel.GetComponent<DamageDealer>().SetIonization(ionizationDamage);
            NetworkServer.Spawn(newShrapnel);
            Destroy(newShrapnel, weaponLifetime);
        }
    }
}
