using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Ability_PopRockets : Ability
{
    float accuracy = 1.0f;
    int numberInSalvo = 3;


    protected override void MouseClickDownEffect()
    {
        if (es.CheckSpendEnergy(costToActivate))
        {
            //TODO AudioClip here
            Vector2 tgtPos = MouseHelper.GetMouseCursorLocation();
            CmdRequestFirewWeapon(tgtPos);
        }


    }

    [Command]
    private void CmdRequestFirewWeapon(Vector2 tgtPos)
    {

        if (es.CheckSpendEnergy(costToActivate))
        {
            for (int i = 0; i < numberInSalvo; i++)
            {
                Vector3 actualTarget = tgtPos + (UnityEngine.Random.insideUnitCircle * accuracy); //CUR.CreateRandomPointNearInputPoint(mousePos, param1, param1 / 10);
                float nominalLifetime = ((actualTarget - transform.position).magnitude / weaponSpeed) * 1.1f;
                GameObject rocket = Instantiate(abilityPrefabs[0], am.SecondaryMuzzle.position, transform.rotation);
                rocket.transform.Rotate(0, 0, UnityEngine.Random.Range(-70, 70));
                DamageDealer dd = rocket.GetComponent<DamageDealer>();
                dd.SetNormalDamage(normalDamage);
                dd.SetOwnership(gameObject);
                AoERocket_AI popRocket = rocket.GetComponent<AoERocket_AI>();
                popRocket.SetNavTarget(actualTarget);
                popRocket.speed = weaponSpeed;
                NetworkServer.Spawn(rocket);
                Destroy(rocket, nominalLifetime);
            }
        }
    }

    protected override void MouseClickUpEffect()
    {

    }
}
