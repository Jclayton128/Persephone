using Mirror;
using UnityEngine;

public class Ability_PopRockets : Ability
{
    float accuracy = 1.0f;
    int minNumberInSalvo = 1;
    int maxNumberInSalvo = 3;


    protected override void MouseClickDownEffect()
    {
        //TODO AudioClip here
        Vector2 tgtPos = MouseHelper.GetMouseCursorLocation();
        CmdRequestFirewWeapon(tgtPos);
    }

    [Command]
    private void CmdRequestFirewWeapon(Vector2 tgtPos)
    {

        if (es.CheckSpendEnergy(costToActivate))
        {
            int actualCount = Random.Range(minNumberInSalvo, maxNumberInSalvo+1);
            for (int i = 0; i < actualCount; i++)
            {
                Vector3 actualTarget = tgtPos + (Random.insideUnitCircle * accuracy); //CUR.CreateRandomPointNearInputPoint(mousePos, param1, param1 / 10);
                float nominalLifetime = ((actualTarget - transform.position).magnitude / weaponSpeed) * 1.1f;
                GameObject rocket = Instantiate(abilityPrefabs[0], am.SecondaryMuzzle.position, transform.rotation);
                rocket.transform.Rotate(0, 0, Random.Range(-70, 70));
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

    public void ModifyCountByOne()
    {
        minNumberInSalvo += 1;
        maxNumberInSalvo += 1;
    }
}
