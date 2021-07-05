using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Stalker_Brain : Brain
{
    [SerializeField] AudioClip cloakSound;
    [SerializeField] AudioClip decloakSound;
    [SerializeField] AudioClip weaponFiringSound;


    //weapon param

    float projectileLifetimeRandomFactor = 0.2f;
    int projectilesInBurst = 6;
    float degreesSpreadOfEntireBurst = 60f;

    bool shouldBeCloaked = false;

    [SyncVar(hook = nameof(HandleCloakingOnClient))]
    float cloakFactor = 0.5f;
    float cloakRate = 1f;

    float chargeFactor;
    float chargeRate = 0.2f;

    float timeForNextBurst;
    bool firstBurstComplete = false;

    public override void OnStartServer()
    {
        base.OnStartServer();
        weaponIsCharged = true;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        HandleCloakingOnClient(1, 0);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isServer)
        {
            TurnToFaceDestination(faceMode);
            MoveTowardsNavTarget(attackRange*.5f);
        }
    }
    protected override void Update()
    {
        base.Update();

        if (isServer)
        {
            HandleCloakingOnServer();
            HandleAttackBehaviour();
            HandleCharging();
        }
    }

    private void HandleCharging()
    {
        if (!weaponIsCharged)
        {
            chargeFactor += chargeRate * Time.deltaTime;
            chargeFactor = Mathf.Clamp01(chargeFactor);
        }
        if (chargeFactor == 1)
        {
            weaponIsCharged = true;
        }
    }

    [Client]
    private void HandleCloakingOnClient(float oldValue, float newValue)
    {
        sr.color = new Color(cloakFactor, cloakFactor, cloakFactor, .5f + cloakFactor * 0.5f);
        if (oldValue == 0 && newValue > 0)
        {
            //TODO audio, decloaking sound
        }
        if (oldValue == 1 && newValue < 1)
        {
            //TODO audio, cloaking sound
        }

    }

    [Server]
    private void HandleCloakingOnServer()
    {
        DetermineIfShouldBeCloaked();
        if (shouldBeCloaked && cloakFactor > 0)
        {
            Cloak();
        }
        if (!shouldBeCloaked && cloakFactor < 1)
        {
            Decloak();
        }
        AdjustPhysicsLayerWhenCloaked();

    } 

    private void DetermineIfShouldBeCloaked()
    {
        if (!weaponIsCharged || distToAttackTarget > attackRange)
        {
            shouldBeCloaked = true;
            return;
        }
        if (weaponIsCharged && distToAttackTarget <= attackRange )
        {
            shouldBeCloaked = false;
            return;
        }
    }

    private void AdjustPhysicsLayerWhenCloaked()
    {
        if (cloakFactor == 0)
        {
            gameObject.layer = 21;
        }
        if (cloakFactor == 1)
        {
            gameObject.layer = 10;
        }
    }

    [Server]
    private void HandleAttackBehaviour()
    {
        if (weaponIsCharged && distToAttackTarget < attackRange*.7f)
        {
            shouldBeCloaked = false;
            if (!firstBurstComplete && cloakFactor == 1)
            {
                SpawnBurstOfProjectiles(muz.PrimaryMuzzle);
                firstBurstComplete = true;
                timeForNextBurst = Time.time + intervalBetweenWeapons;
            }
        }
        if (firstBurstComplete)
        {
            if (Time.time >= timeForNextBurst)
            {
                SpawnBurstOfProjectiles(muz.SecondaryMuzzle);
                firstBurstComplete = false;
                weaponIsCharged = false;
                chargeFactor = 0;
            }
        }
    }  

    private void SpawnBurstOfProjectiles(Transform muzzle)
    {
        // push audio to clients
        float spreadSubdivided = degreesSpreadOfEntireBurst / projectilesInBurst;
        for (int i = 0; i < projectilesInBurst; i++)
        {
            Quaternion sector = Quaternion.Euler(0, 0, (i * spreadSubdivided) - (degreesSpreadOfEntireBurst / 2) + transform.eulerAngles.z);
            GameObject shuriken = Instantiate(weaponPrefab, muzzle.position, sector) as GameObject;
            shuriken.layer = 11;
            shuriken.GetComponent<Rigidbody2D>().velocity = shuriken.transform.up * weaponSpeed;
            DamageDealer damageDealer = shuriken.GetComponent<DamageDealer>();
            damageDealer.SetNormalDamage(weaponNormalDamage);
            damageDealer.SetOwnership(gameObject);
            float randomLifetime = weaponLifetime + UnityEngine.Random.Range(-projectileLifetimeRandomFactor, projectileLifetimeRandomFactor);
            Destroy(shuriken, randomLifetime);
            NetworkServer.Spawn(shuriken);
        }
    }

    private void Decloak()
    {
        cloakFactor += Time.deltaTime * cloakRate;
        cloakFactor = Mathf.Clamp01(cloakFactor);
    }

    private void Cloak()
    {
        cloakFactor -= Time.deltaTime * cloakRate;
        cloakFactor = Mathf.Clamp01(cloakFactor);
    }
}
