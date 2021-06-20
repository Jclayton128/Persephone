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

    AudioClip chosenHurtSound;
    AudioClip chosenDieSound;

    UIManager uim;
    Slider persephoneHealthSlider;

    // [SerializeField] Particle  //TODO cause drain thing to have a Purple particle effect 

    //param
    [SerializeField] float startingHealth;
    
    #region Init: current state
    //hood
    [SerializeField] bool isDying = false;


    [SyncVar(hook = nameof(UpdateUI))]
    float currentHealth;

    DamageDealer lastDamageDealerToBeHitBy;
    GameObject ownerOfLastDamageDealerToBeHitBy;


    #endregion

    public Action EntityWasDamaged;
    public Action EntityIsDying;

    void Start()
    {

        currentHealth = startingHealth;

        SetAudioClips();

        if (isClient)
        {
            HookIntoLocalUI();
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
            EntityIsDying?.Invoke();
            BroadcastMessage("DyingActions", ownerOfLastDamageDealerToBeHitBy, SendMessageOptions.DontRequireReceiver);
            if (chosenDieSound)
            {
                AudioSource.PlayClipAtPoint(chosenDieSound, transform.position);
                //if (countsTowardsScore)
                //{
                //    sk.IncreaseScore();
                //}
                Destroy(gameObject);
                //Destroy(gameObject, chosenDieSound.length);
            }
            if (!chosenDieSound)
            {
                //if (countsTowardsScore)
                //{
                //    sk.IncreaseScore();
                //}
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageDealer damageDealer = other.gameObject.GetComponent<DamageDealer>();

        //Begin check for validity of received damage - "should this weapon actually affect me?"
        if (!damageDealer) { return; }
        if (damageDealer.IsReal == false) { return; }
        if (gameObject == damageDealer.GetOwningEntity()) { return; }

        // Its a valid hit; begin responding
        lastDamageDealerToBeHitBy = damageDealer;
        if (damageDealer.GetOwningEntity())
        {
            ownerOfLastDamageDealerToBeHitBy = damageDealer.GetOwningEntity();
        }

        if (damageDealer.particleExplosionAtImpact)
        {
            GameObject damageParticleEffect = Instantiate(damageDealer.particleExplosionAtImpact, transform.position, transform.rotation) as GameObject;
            Destroy(damageParticleEffect, 10);
        }

        Damage damage = damageDealer.GetDamage();

        ModifyHullLevel(damage.RegularDamage * -1);

        if (chosenHurtSound)
        {
            AudioSource.PlayClipAtPoint(chosenHurtSound, transform.position);
        }

        damageDealer.ModifyPenetration(-10);  //Nothing should be able to penetrate through Pers
    }


    private void UpdateUI(float v1, float v2)
    {
       
        if (persephoneHealthSlider)
        {
            persephoneHealthSlider.maxValue = startingHealth;
            persephoneHealthSlider.value = currentHealth;
        }        
    }

    internal void CheckPayPlayerRepairCost(float repairCost)
    {
        if (repairCost > currentHealth)
        {
            Debug.Log("Persephone is too damaged to repair a player.");
        }
        else
        {
            currentHealth -= repairCost;
        }
    }
}

