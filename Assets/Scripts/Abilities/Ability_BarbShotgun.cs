using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Ability_BarbShotgun : Ability
{
    //upgradeable param
    int bulletsPerShot = 5;
    //float weaponLifetime already captured in base Ability

    //immutable param
    float lifetimeRandomFactor = 0.2f;
    float degreesToCover = 60f;



    protected override void MouseClickDownEffect()
    {

        if (es.CheckSpendEnergy(costPerShot))
        {
            //TODO "Pa-CHOW" shotgun blast sound.
            float spreadSubdivided = degreesToCover / bulletsPerShot;
            for (int i = 0; i < bulletsPerShot; i++)
            {
                Quaternion sector = Quaternion.Euler(0, 0, (i * spreadSubdivided) - (degreesToCover / 2) + transform.eulerAngles.z);
                GameObject pellet = Instantiate(abilityPrefabs[0], am.PrimaryMuzzle.position, sector) as GameObject;
                pellet.GetComponent<Rigidbody2D>().velocity = pellet.transform.up * weaponSpeed;
                pellet.layer = 9;
                DamageDealer damageDealer = pellet.GetComponent<DamageDealer>();
                damageDealer.SetRegularDamage(normalDamage);
                damageDealer.SetShieldBonusDamage(shieldBonusDamage);

                damageDealer.SetPenetration(2);
                float randomLifetime = weaponLifetime + Random.Range(-lifetimeRandomFactor, lifetimeRandomFactor);
                Destroy(pellet, randomLifetime);
            }


        }
    }

    protected override void MouseClickUpEffect()
    {
        
    }
}
