using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class Health : NetworkBehaviour
{
    //init
    [SerializeField] AudioClip[] hurtAudioClip = null;
    [SerializeField] AudioClip[] dieAudioClip = null;
    [SerializeField] Slider hullLevel = null;
    [SerializeField] Slider shieldLevel = null;
    [SerializeField] TextMeshProUGUI maxHulltmp = null;
    [SerializeField] TextMeshProUGUI maxShieldtmp = null;
    [SerializeField] TextMeshProUGUI regenShieldtmp = null;

    SpriteRenderer playerSR;
    AudioClip chosenHurtSound;
    AudioClip chosenDieSound;
    Rigidbody2D rb;

    //param
    public bool isPlayer = false;
    public float shieldMax = 0;
    public float shieldRegenPerSecond = 0f;
    public float hullMax = 1;
    float dragAtDeath = 30f;
    public bool countsTowardsScore = false;

    //hood
    public bool isDying = false;
    public float shieldCurrentLevel;
    public float hullCurrentLevel;
    DamageDealer lastDamageDealerToBeHitBy;
    GameObject ownerOfLastDamageDealerToBeHitBy;
    void Start()
    {
        shieldCurrentLevel = shieldMax;
        hullCurrentLevel = hullMax;
        if (hullLevel)
        {
            hullLevel.maxValue = hullMax;
            hullLevel.value = hullCurrentLevel;
        }
        if (shieldLevel)
        {
            shieldLevel.maxValue = shieldMax;
            shieldLevel.value = shieldCurrentLevel;
        }
        SetAudioClips();
        rb = GetComponent<Rigidbody2D>();
        if (isPlayer)
        {
            playerSR = GetComponent<SpriteRenderer>();
        }
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
        LiveOrDie();
        RechargeShield();
        CheatHeal();
    }

    private void CheatHeal()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            ResetHealthTotally();
        }
    }

    private void RechargeShield()
    {
        if (shieldCurrentLevel < shieldMax)
        {
            shieldCurrentLevel += shieldRegenPerSecond * Time.deltaTime;
        }
        if (shieldLevel)
        {
            shieldLevel.value = shieldCurrentLevel;
        }
    }

    public void ModifyShieldLevel(float amount, bool affectHullToo)
    {
        //if (GetComponentInChildren<PhaseShield>()) { return; } //phase shield prevent all damage

        BroadcastMessage("ReceivedDamage", ownerOfLastDamageDealerToBeHitBy, SendMessageOptions.DontRequireReceiver);
        if (shieldCurrentLevel < 0 && affectHullToo)
        {
            ModifyHullLevel(amount); //Go direct to hull and do no shield damage
        }

        if (shieldCurrentLevel >= 0)
        {
            shieldCurrentLevel += amount;
            if (shieldCurrentLevel < 0 && affectHullToo)  //If shield was positive, takes damage, and becomes negative, pass the negative amount on to the hull;
            {
                float negativeShield = shieldCurrentLevel;
                ModifyHullLevel(negativeShield);
                shieldCurrentLevel = 0;
            }
        }
        if (shieldCurrentLevel + amount > shieldMax)
        {
            //Debug.Log("can't overcharge the shields");
            shieldCurrentLevel = shieldMax;
        }
        if (shieldLevel)
        {
            shieldLevel.value = shieldCurrentLevel;
        }
    }

    public void DumpAllShields()
    {
        shieldCurrentLevel = 0f;
    }
    public void ModifyHullLevel(float amount)
    {
        hullCurrentLevel += amount;
        hullCurrentLevel = Mathf.Clamp(hullCurrentLevel, 0, hullMax);
        if (hullLevel)
        {
            hullLevel.value = hullCurrentLevel;
        }
    }

    private void LiveOrDie()
    {
        if (hullCurrentLevel <= 0 && !isDying)
        {
            rb.drag = dragAtDeath; //This slows the wreckage down.
            isDying = true;
            //if (lastDamageDealerToBeHitBy && GetComponent<ScrapDropper>() == true)
            //{
            //    float bonusScrapThresh = lastDamageDealerToBeHitBy.GetBonusScrapThreshold();

            //    GetComponent<ScrapDropper>().bonusScrapThreshold = bonusScrapThresh;
            //}
            BroadcastMessage("DyingActions", ownerOfLastDamageDealerToBeHitBy, SendMessageOptions.DontRequireReceiver);
            if (isPlayer)
            {
                AudioSource.PlayClipAtPoint(chosenDieSound, transform.position);
                GetComponent<Rigidbody2D>().drag = 5f;
                playerSR.color = new Color(0, 0, 0, 0);
            }
            if (chosenDieSound && !isPlayer)
            {
                AudioSource.PlayClipAtPoint(chosenDieSound, transform.position);
                //if (countsTowardsScore)
                //{
                //    sk.IncreaseScore();
                //}
                Destroy(gameObject);
                //Destroy(gameObject, chosenDieSound.length);
            }
            if (!chosenDieSound && !isPlayer)
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
        if (!damageDealer) { return; }
        if (damageDealer.IsReal == false) { return; }
        if (gameObject == damageDealer.GetSafeObject()) { return; }
        lastDamageDealerToBeHitBy = damageDealer;
        if (damageDealer.GetSafeObject())
        {
            ownerOfLastDamageDealerToBeHitBy = damageDealer.GetSafeObject();
        }

        if (damageDealer.particleExplosionAtImpact)
        {
            GameObject damageParticleEffect = Instantiate(damageDealer.particleExplosionAtImpact, transform.position, transform.rotation) as GameObject;
            Destroy(damageParticleEffect, 10);
        }

        if (damageDealer.GetKnockBack() == true)
        {
            Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
            Rigidbody2D collRB = other.transform.GetComponent<Rigidbody2D>();
            rb2d.AddForce(damageDealer.GetKnockBackAmount() * collRB.velocity, ForceMode2D.Impulse);
        }

        if (Mathf.Abs(damageDealer.GetSpeedModifier()) > 0)
        {
            rb.velocity = (rb.velocity.magnitude * damageDealer.GetSpeedModifier()) * rb.velocity.normalized;
        }

        float incomingDamage = damageDealer.GetDamage();
        if (incomingDamage == 0) { return; } //Don't do anymore work if the impact doesn't cause damage

        ModifyShieldLevel(incomingDamage * -1, true);

        if (chosenHurtSound)
        {
            AudioSource.PlayClipAtPoint(chosenHurtSound, transform.position);
        }
        //Debug.Log("destroying projectile");

        damageDealer.ModifyPenetration(-1);
        if (damageDealer.GetPenetration() <= 0)
        {
            Destroy(other.gameObject);
        }
    }

    public void ResetHealthTotally()
    {
        if (isPlayer)
        {
            shieldCurrentLevel = shieldMax;
            hullCurrentLevel = hullMax;
        }
    }

    public void ResetShields()
    {
        shieldCurrentLevel = shieldMax;
    }

    public void SetMaxHull(float newMaxHull)
    {
        hullMax = newMaxHull;
        if (hullLevel)
        {
            hullLevel.maxValue = hullMax;
            maxHulltmp.text = hullMax.ToString();

        }
    }
    public void SetMaxShield(float newMaxShield)
    {
        shieldMax = newMaxShield;
        if (isPlayer)
        {
            shieldLevel.maxValue = shieldMax;
            //Debug.Log("new shield max: " + shieldMax);
            maxShieldtmp.text = shieldMax.ToString();
        }
    }

    public void SetShieldRegen(float newShieldRegen)
    {
        shieldRegenPerSecond = newShieldRegen;
        if (isPlayer)
        {
            //Debug.Log("new shield regen: " + shieldRegenPerSecond);
            regenShieldtmp.text = shieldRegenPerSecond.ToString();
        }
    }
    public float GetMaxHull()
    {
        return hullMax;
    }

}

