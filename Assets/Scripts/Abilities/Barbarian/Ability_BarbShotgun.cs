using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Ability_BarbShotgun : Ability
{

    
    //upgradeable param
    int minBulletsPerShot = 3;
    int maxBulletsPerShot = 5;


    //immutable param
    float lifetimeRandomFactor = 0.2f;
    float degreesToCover = 40f;



    protected override void MouseClickDownEffect()
    {
        //TODO "Pa-CHOW" shotgun blast sound.
        CmdRequestFireWeapon();        
    }

    [Command]
    private void CmdRequestFireWeapon()
    {
        if (es.CheckSpendEnergy(costToActivate))
        {
            int actualCount = UnityEngine.Random.Range(minBulletsPerShot, maxBulletsPerShot + 1);
            float spreadSubdivided = degreesToCover / maxBulletsPerShot;
            for (int i = 0; i < actualCount; i++)
            {
                Quaternion sector = Quaternion.Euler(0, 0, (i * spreadSubdivided) - (degreesToCover / 2) + transform.eulerAngles.z);
                GameObject pellet = Instantiate(abilityPrefabs[0], am.PrimaryMuzzle.position, sector) as GameObject;
                pellet.GetComponent<Rigidbody2D>().velocity = (pellet.transform.up * weaponSpeed) + (Vector3)avatarRB.velocity;
                pellet.layer = 9;
                DamageDealer damageDealer = pellet.GetComponent<DamageDealer>();
                damageDealer.SetNormalDamage(normalDamage);
                damageDealer.SetShieldBonusDamage(shieldBonusDamage);

                damageDealer.SetPenetration(2);

                NetworkServer.Spawn(pellet);
                float randomLifetime = weaponLifetime + UnityEngine.Random.Range(-lifetimeRandomFactor, lifetimeRandomFactor);
                Destroy(pellet, randomLifetime);
            }


        }
    }

    protected override void MouseClickUpEffect()
    {
        
    }

    public override void ModifyCount(int amount)
    {
        minBulletsPerShot += amount;
        maxBulletsPerShot += amount;
        degreesToCover += 5f;

    }

    public override void ModifyRangeViaSpeedOrLifetime(float amount)
    {
        weaponLifetime += amount;
    }
}
