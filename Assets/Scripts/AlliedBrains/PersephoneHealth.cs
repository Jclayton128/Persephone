using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class PersephoneHealth : NetworkBehaviour
{
    //init

    [SerializeField] AudioClip[] hurtAudioClip = null;
    [SerializeField] AudioClip[] dieAudioClip = null;
    ParticleSystem deathFX;

    AudioClip chosenHurtSound;
    AudioClip chosenDieSound;

    UIManager uim;
    Slider persephoneHealthSlider;

    // [SerializeField] Particle  //TODO cause drain thing to have a Purple particle effect 

    //param
    [SerializeField] float startingHealth;
    float timeSpentDying = 5;
    
    #region Init: current state
    //hood
    [SerializeField] bool isDying = false;


    [SyncVar(hook = nameof(UpdateUI))]
    [SerializeField] float currentHealth;

    int penetrationToSoakUp = 10;

    DamageDealer lastDamageDealerToBeHitBy;
    GameObject ownerOfLastDamageDealerToBeHitBy;


    #endregion

    //hood
    float timeToDie = Mathf.Infinity;

    public Action EntityWasDamaged;
    public Action PersephoneIsDying;
    public Action PersephoneIsDead;



    void Start()
    {
        currentHealth = startingHealth;
        SetAudioClips();

        if (isClient)
        {
            HookIntoLocalUI();
            deathFX = GetComponent<ParticleSystem>();
        }
    }

    private void HookIntoLocalUI()
    {
        ClientInstance ci = ClientInstance.ReturnClientInstance();
        uim = FindObjectOfType<UIManager>();
        persephoneHealthSlider = uim.GetPersephoneHealthSlider();
        UpdateUI(0,0);
    }

    private void SetAudioClips()
    {
        if (hurtAudioClip.Length == 0 || dieAudioClip.Length == 0) { return; }
        int selectedSound = UnityEngine.Random.Range(0, dieAudioClip.Length);
        chosenDieSound = dieAudioClip[selectedSound];
        int selectedSound_2 = UnityEngine.Random.Range(0, hurtAudioClip.Length);
        chosenHurtSound = hurtAudioClip[selectedSound_2];
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            LiveOrDie();
            if (isDying && Time.time >= timeToDie)
            {
                //stop particleFX
                PersephoneIsDead?.Invoke();
                RpcToggleDeathEffectsOnClient(false);
                Destroy(gameObject);
            }
        }
    }

    public void ModifyHullLevel(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, startingHealth);
    }

    private void LiveOrDie()
    {
        if (currentHealth <= 0 && !isDying)
        {
            isDying = true;
            //TODO Pers death results in a slow-down of time, a camera snap to the persephone, and eventual game over.
            PersephoneIsDying?.Invoke();
            if (chosenDieSound)
            {
                AudioSource.PlayClipAtPoint(chosenDieSound, transform.position);

                DepictPersephoneDeath();
                //Destroy(gameObject, chosenDieSound.length);
            }
            if (!chosenDieSound)
            {

                DepictPersephoneDeath();
            }
        }
    }

    private void DepictPersephoneDeath()
    {
        var turrets = GetComponentsInChildren<Turret_AI>();
        foreach (var turret in turrets)
        {
            turret.enabled = false;
        }
        GetComponent<IFF>().SetEnabledDisabledImportance(true);
        GetComponent<PersephoneBrain>().enabled = false;
        var rb = GetComponent<Rigidbody2D>();
        rb.drag = 3f;
        rb.angularDrag = 1f;
        RpcToggleDeathEffectsOnClient(true);
        timeToDie = Time.time + timeSpentDying;

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageDealer damageDealer = other.gameObject.GetComponent<DamageDealer>();
        if (isClient)
        {
            //Begin check for validity of received damage - "should this weapon actually affect me?"
            if (!damageDealer) { return; }

            if (damageDealer.particleExplosionAtImpact)
            {
                GameObject damageParticleEffect = Instantiate(damageDealer.particleExplosionAtImpact, transform.position, transform.rotation) as GameObject;
                Destroy(damageParticleEffect, 10);
            }

            if (chosenHurtSound)
            {
                AudioSource.PlayClipAtPoint(chosenHurtSound, transform.position);
            }

            damageDealer.ModifyPenetration(-1 * penetrationToSoakUp);
        }

        if (isServer)
        {
            //Begin check for validity of received damage - "should this weapon actually affect me?"
            if (!damageDealer) { return; }
            //if (damageDealer.IsReal == false) { return; }
            if (gameObject == damageDealer.GetOwner()) { return; }

            // Its a valid hit; begin responding
            lastDamageDealerToBeHitBy = damageDealer;
            if (damageDealer.GetOwner())
            {
                ownerOfLastDamageDealerToBeHitBy = damageDealer.GetOwner();
            }

            if (damageDealer.particleExplosionAtImpact)
            {
                GameObject damageParticleEffect = Instantiate(damageDealer.particleExplosionAtImpact, transform.position, transform.rotation) as GameObject;
                Destroy(damageParticleEffect, 10);
            }

            Damage damage = damageDealer.GetDamage(transform);

            ModifyHullLevel(damage.RegularDamage * -1);

            if (chosenHurtSound)
            {
                AudioSource.PlayClipAtPoint(chosenHurtSound, transform.position);
            }

            damageDealer.ModifyPenetration(-1 * penetrationToSoakUp);  //Nothing should be able to penetrate through Pers
        }
    }

    private void UpdateUI(float v1, float v2)
    {
       
        if (persephoneHealthSlider)
        {
            persephoneHealthSlider.maxValue = startingHealth;
            persephoneHealthSlider.value = currentHealth;
        }        
    }

    internal bool CheckPayPlayerRepairCost(float repairCost)
    {
        if (repairCost > currentHealth)
        {
            return false;
        }
        else
        {
            currentHealth -= repairCost;
            return true;
        }
    }

    [ClientRpc]
    private void RpcToggleDeathEffectsOnClient(bool isPlaying)
    {
        if (isPlaying)
        {
            deathFX.Play();
            Camera.main.GetComponent<WorldCameraController>().FollowSpecificTarget(gameObject);
        }
        if (!isPlaying)
        {
            deathFX.Pause();
        }
    }
}

