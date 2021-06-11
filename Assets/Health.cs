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

    UIManager uim;
    [SerializeField] Slider hullSlider;
    [SerializeField] Slider shieldSlider;
    [SerializeField] TextMeshProUGUI hullMaxTMP = null;
    [SerializeField] TextMeshProUGUI shieldMaxTMP = null;
    [SerializeField] TextMeshProUGUI shieldRateTMP = null;

    AudioClip chosenHurtSound;
    AudioClip chosenDieSound;
    Rigidbody2D rb;

    //param
    [SerializeField] bool isPlayer = false;

    [SyncVar(hook = nameof(UpdateUI))]
    [SerializeField] float hullMax = 1;

    [SyncVar(hook = nameof(UpdateUI))]
    [SerializeField] float shieldMax;

    [SyncVar(hook = nameof(UpdateUI))]
    [SerializeField] float shieldRegenPerSecond;

    float dragAtDeath = 30f;
    [SerializeField] bool countsTowardsScore = false;

    //hood
    [SerializeField] bool isDying = false;

    [SyncVar(hook = nameof(UpdateUI))]
     float shieldCurrentLevel;

    [SyncVar(hook = nameof(UpdateUI))]
    float hullCurrentLevel;

    DamageDealer lastDamageDealerToBeHitBy;
    GameObject ownerOfLastDamageDealerToBeHitBy;
    void Start()
    {
        shieldCurrentLevel = shieldMax;
        hullCurrentLevel = hullMax;
        SetAudioClips();
        rb = GetComponent<Rigidbody2D>();

        if (hasAuthority)
        {
            HookIntoLocalUI();
        }
    }

    private void HookIntoLocalUI()
    {

        ClientInstance ci = ClientInstance.ReturnClientInstance();
        uim = FindObjectOfType<UIManager>();
        UIPack uipack = uim.GetUIPack(ci);
        hullSlider = uipack.HullSlider;
        shieldSlider = uipack.ShieldSlider;
        hullMaxTMP = uipack.HullMaxTMP;
        shieldMaxTMP = uipack.ShieldMaxTMP;
        shieldRateTMP = uipack.ShieldRateTMP;

        UpdateUI(0, 0);
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
    }

    private void RechargeShield()
    {
        if (shieldCurrentLevel < shieldMax)
        {
            shieldCurrentLevel += shieldRegenPerSecond * Time.deltaTime;
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
            if (shieldCurrentLevel < 0 && !affectHullToo)
            {
                shieldCurrentLevel = 0;
            }
        }
        if (shieldCurrentLevel + amount > shieldMax)
        {
            //Debug.Log("can't overcharge the shields");
            shieldCurrentLevel = shieldMax;
        }
    }

    public void ModifyHullLevel(float amount)
    {
        hullCurrentLevel += amount;
        hullCurrentLevel = Mathf.Clamp(hullCurrentLevel, 0, hullMax);
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

    public void ResetShields()
    {
        shieldCurrentLevel = shieldMax;
    }

    public void SetMaxHull(float newMaxHull)
    {
        hullMax = newMaxHull;
        if (hullSlider)
        {
            hullSlider.maxValue = hullMax;
            hullMaxTMP.text = hullMax.ToString();

        }
    }
    public void SetMaxShield(float newMaxShield)
    {
        shieldMax = newMaxShield;
        if (isPlayer)
        {
            shieldSlider.maxValue = shieldMax;
            //Debug.Log("new shield max: " + shieldMax);
            shieldMaxTMP.text = shieldMax.ToString();
        }
    }

    public void SetShieldRegen(float newShieldRegen)
    {
        shieldRegenPerSecond = newShieldRegen;
        if (isPlayer)
        {
            //Debug.Log("new shield regen: " + shieldRegenPerSecond);
            shieldRateTMP.text = shieldRegenPerSecond.ToString();
        }
    }
    public float GetMaxHull()
    {
        return hullMax;
    }

    private void UpdateUI(float oldValue, float newValue)
    {
        if (hullSlider)
        {
            hullSlider.maxValue = hullMax;
            hullSlider.value = hullCurrentLevel;
        }
        if (shieldSlider)
        {
            shieldSlider.maxValue = shieldMax;
            shieldSlider.value = shieldCurrentLevel;
        }
        if (hullMaxTMP)
        {
            hullMaxTMP.text = hullMax.ToString();
        }
        if (shieldMaxTMP && shieldRateTMP)
        {
            shieldMaxTMP.text = shieldMax.ToString();
            shieldRateTMP.text = shieldRegenPerSecond.ToString();
        } 
    }

}

