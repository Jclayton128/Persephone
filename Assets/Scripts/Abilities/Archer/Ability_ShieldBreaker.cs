using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Ability_ShieldBreaker : Ability
{
    [SerializeField] Transform muzzle;
    GameObject chargingBullet;
    bool isCharging = false;
    float chargeRate = 0.33f;

    [SyncVar(hook = nameof(UpdateChargingWeaponGraphic))]
    float chargingFactor;

    protected override void Start()
    {
        base.Start();
        am.ToggleStatusIcon(this, false);
    }

    protected override void MouseClickDownEffect()
    {
        CmdRequestBeginCharging();
        //TODO audio: a steadily-building hum or whine as the shot charges up.
    }

    [Command]
    private void CmdRequestBeginCharging()
    {
        if (es.CheckEnergy(costToActivate))
        {
            isCharging = true;
        }
    }

    protected override void MouseClickUpEffect()
    {
        CmdRequestFireChargedWeapon();
    }

    [Command]
    private void CmdRequestFireChargedWeapon()
    {
        FireWeapon();
    }

    [Server]
    private void FireWeapon()
    {
        if (chargingFactor > 0.2f)
        {

            chargingBullet.layer = 9;

            chargingBullet.GetComponent<Rigidbody2D>().velocity = chargingBullet.transform.up * weaponSpeed;

            DamageDealer dd = chargingBullet.GetComponent<DamageDealer>();
            dd.SetNormalDamage(normalDamage * chargingFactor);

            Destroy(chargingBullet, weaponLifetime);
            chargingBullet = null;
        }
        else
        {
            Destroy(chargingBullet);
        }

        isCharging = false;
        chargingFactor = 0;
        am.ToggleStatusIcon(this, false);
    }


    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            HandleCharging();
        }
    }

    [Server]
    private void HandleCharging()
    {
        if (isCharging)
        {
            if (!es.CheckEnergy(costToActivate * Time.deltaTime))
            {
                FireWeapon();
                return;
            }
            else
            {
                if (chargingBullet == null)
                {
                    chargingBullet = Instantiate(abilityPrefabs[0], muzzle.position, muzzle.rotation) as GameObject;
                    NetworkServer.Spawn(chargingBullet);
                }
                chargingBullet.transform.position = muzzle.position;
                chargingBullet.transform.rotation = muzzle.rotation;

                chargingFactor += Time.deltaTime * chargeRate;
                chargingFactor = Mathf.Clamp01(chargingFactor);

                if (chargingFactor >= 0.9f)
                {
                    am.ToggleStatusIcon(this, true);
                    es.CheckSpendEnergy(costToActivate / 2f * Time.deltaTime);
                }
                else
                {
                    es.CheckSpendEnergy(costToActivate * Time.deltaTime);
                }
            }

        }
    }


    private void UpdateChargingWeaponGraphic(float v1, float v2)
    {
        if (chargingBullet)
        {
            chargingBullet.transform.localScale = Vector3.one * chargingFactor;
        }
    }

    public void Cheapen()
    {
        costToActivate *= 0.8f;
    }
}
