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

    EnergySource es;
    PlayerInput pi;
    PersephoneBrain pb;

    UIManager uim;
    Slider hullSlider;
    Slider shieldSlider;
    TextMeshProUGUI hullMaxTMP;
    TextMeshProUGUI shieldMaxTMP;
    TextMeshProUGUI shieldRateTMP;
    Slider shieldDrainingSlider;
    // [SerializeField] Particle  //TODO cause drain thing to have a Purple particle effect 

    AudioClip chosenHurtSound;
    AudioClip chosenDieSound;
    Rigidbody2D rb;

    //param
    bool isPlayer = true;

    [SyncVar(hook = nameof(UpdateUI))]
    [SerializeField] float hullMax = 1;

    [SyncVar(hook = nameof(UpdateUI))]
    [SerializeField] float shieldMax_normal;  // What the shield Max can be under ideal conditions, and what shows on UI.

    [SyncVar]
    float shieldMax_current;  // What the shield Max can be accounting for Ionization

    [SyncVar]
    [SerializeField] float shieldRate_normal;  // What the shield Regen can be under ideal conditions.

    [SyncVar(hook = nameof(UpdateUI))]
    float shieldRate_current; // What the shield Regen can be accounting for Ionization, and what shows on UI

    [SyncVar]
    [SerializeField] float purificationRate = 0.3f;  // points per second. Ionization and Draining scales from 0 to max Energy/Shield level;


    float dragAtDeath = 3f;
    float angularDragAtDeath = 0.4f;


    #region Init: current state
    //hood
    [SerializeField] bool isDying = false;

    [SyncVar(hook = nameof(UpdateUI))]
    float shieldCurrentLevel;

    [SyncVar(hook = nameof(UpdateUI))]
    float hullCurrentLevel;

    [SyncVar(hook = nameof(UpdateUI))]
    float ionizationAmount;

    float ionFactor = 0;
    public GameObject AssignedWreckerDrone;

    DamageDealer lastDamageDealerToBeHitBy;
    GameObject ownerOfLastDamageDealerToBeHitBy;


    #endregion

    public Action EntityWasDamaged;
    public Action EntityIsDying;
    public Action EntityIsRepaired;

    void Start()
    {
        shieldCurrentLevel = shieldMax_normal;
        shieldMax_current = shieldMax_normal;
        hullCurrentLevel = hullMax;
        ionizationAmount = 0;
        pb = FindObjectOfType<PersephoneBrain>();

        SetAudioClips();
        rb = GetComponent<Rigidbody2D>();
        es = GetComponent<EnergySource>();

        if (gameObject.tag != "Player")
        {
            isPlayer = false;
        }
        if (hasAuthority && isPlayer)
        {
            pi = GetComponent<PlayerInput>();
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
        shieldDrainingSlider = uipack.ShieldIonizationSlider;

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
        if (isServer)
        {
            ProcessIonization();
            if (isDying == false)
            {
                RechargeShield();
            }
        }
    }

    private void ProcessIonization()
    { 
        //Remove Draining
        ionizationAmount -= purificationRate * Time.deltaTime;
        ionizationAmount = Mathf.Clamp(ionizationAmount, 0, shieldMax_normal);

        if (ionizationAmount > 0)
        {
            //TODO spawn a particle effect. Ensure it is seen on all clients
        }

        //Process Draining effects
        ionFactor = 1- ((shieldMax_normal - ionizationAmount) / shieldMax_normal);
        shieldMax_current = (1 - ionFactor) * shieldMax_normal;
        shieldRate_current = (1 - ionFactor) * shieldRate_normal;
    }

    private void RechargeShield()
    {
        shieldCurrentLevel += shieldRate_current * Time.deltaTime;
        shieldCurrentLevel = Mathf.Clamp(shieldCurrentLevel, 0, shieldMax_current);

    }

    public void ModifyShieldLevel(float amount, bool affectHullToo)
    {
        //if (GetComponentInChildren<PhaseShield>()) { return; } //phase shield prevent all damage

        //BroadcastMessage("ReceivedDamage", ownerOfLastDamageDealerToBeHitBy, SendMessageOptions.DontRequireReceiver);
        //TODO convert whatever listens for this^ to rely on a EntityWasDamaged event.

        if (shieldCurrentLevel < 0 && affectHullToo)
        {
            ModifyHullLevel(amount, false); //Go direct to hull and do no shield damage
        }

        if (shieldCurrentLevel >= 0)
        {
            shieldCurrentLevel += amount;
            if (shieldCurrentLevel < 0 && affectHullToo)  //If shield was positive, takes damage, and becomes negative, pass the negative amount on to the hull;
            {
                float negativeShield = shieldCurrentLevel;
                ModifyHullLevel(negativeShield, false);
                shieldCurrentLevel = 0;
            }
            if (shieldCurrentLevel < 0 && !affectHullToo)
            {
                shieldCurrentLevel = 0;
            }
        }
        if (shieldCurrentLevel + amount > shieldMax_current)
        {
            //Debug.Log("can't overcharge the shields");
            shieldCurrentLevel = shieldMax_current;
        }
    }

    public void ModifyHullLevel(float amount, bool shouldPurifyToo)
    {
        hullCurrentLevel += amount;
        if (hullCurrentLevel >= hullMax)
        {
            SignalRepairIsComplete();
        }
        if (shouldPurifyToo)
        {
            ionizationAmount = 0;
        }
        hullCurrentLevel = Mathf.Clamp(hullCurrentLevel, 0, hullMax);

        if (isClient)
        {
            UpdateUI(0, 0);
        }

        AssessDeathCondition();

    }    

    private void AssessDeathCondition()
    {
        if (hullCurrentLevel <= 0 && !isDying)
        {
            rb.drag = dragAtDeath; //This slows the wreckage down.
            rb.angularDrag = angularDragAtDeath;  //This slows the wreckage down.
            isDying = true;
            //if (lastDamageDealerToBeHitBy && GetComponent<ScrapDropper>() == true)
            //{
            //    float bonusScrapThresh = lastDamageDealerToBeHitBy.GetBonusScrapThreshold();

            //    GetComponent<ScrapDropper>().bonusScrapThreshold = bonusScrapThresh;
            //}
            EntityIsDying?.Invoke();
            BroadcastMessage("DyingActions", ownerOfLastDamageDealerToBeHitBy, SendMessageOptions.DontRequireReceiver);
            if (isPlayer)
            {
                if (isServer)
                {
                    pb.AddDisabledPlayer(gameObject);
                }
                //AudioSource.PlayClipAtPoint(chosenDieSound, transform.position);  //TODO play a powerdown disabled sound     
                //TODO some kind of UI feedback to signal being disabled. Hud Cracks?

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

        if (damage.KnockbackAmount != 0)
        {
            Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
            Rigidbody2D collRB = other.transform.GetComponent<Rigidbody2D>();
            rb2d.AddForce(damage.KnockbackAmount * collRB.velocity, ForceMode2D.Impulse);
        }

        if (Mathf.Abs(damage.SpeedModifier) > 0)
        {
            rb.velocity = (rb.velocity.magnitude * damageDealer.GetSpeedModifier()) * rb.velocity.normalized;
        }

        if (damage.Ionization > 0)
        {
            es.ReceiveIonizationDamage(damage.Ionization);
        }

        if (damage.Draining > 0)
        {
            ionizationAmount += damage.Draining;
        }

        ModifyShieldLevel(damage.ShieldBonusDamage * -1, false);
        ModifyShieldLevel(damage.RegularDamage * -1, true);

        if (chosenHurtSound)
        {
            AudioSource.PlayClipAtPoint(chosenHurtSound, transform.position);
        }

        damageDealer.ModifyPenetration(-1);

    }

    public void ResetShields()
    {
        shieldCurrentLevel = shieldMax_current;
    }

    public void SignalRepairIsComplete()
    {
        //TODO play a Ratchet sound
        isDying = false;
        rb.drag = 0;
        rb.angularDrag = 0;
        pb.RemoveDisabledPlayer(gameObject);
        AssignedWreckerDrone = null;
        EntityIsRepaired?.Invoke();

    }

    public void SetMaxShield(float newMaxShield)
    {
        shieldMax_normal = newMaxShield;
    }

    public void SetShieldRegen(float newShieldRegen)
    {
        shieldRate_current = newShieldRegen;
    }
    public float GetMaxHull()
    {
        return hullMax;
    }

    public float GetCurrentHull()
    {
        return hullCurrentLevel;
    }

    private void UpdateUI(float oldValue, float newValue)
    {
        if (!isPlayer) { return; }
        if (hullSlider)
        {
            hullSlider.maxValue = hullMax;
            hullSlider.value = hullCurrentLevel;
        }
        if (shieldSlider)
        {
            shieldSlider.maxValue = shieldMax_normal;
            shieldSlider.value = shieldCurrentLevel;
        }
        if (hullMaxTMP)
        {
            hullMaxTMP.text = hullMax.ToString();
        }
        if (shieldMaxTMP && shieldRateTMP)
        {
            shieldMaxTMP.text = shieldMax_normal.ToString();
            shieldRateTMP.text = shieldRate_current.ToString();
        } 
        if (shieldDrainingSlider)
        {
            shieldDrainingSlider.value = ionFactor;
        }
    }

    public float GetPurificationRate()
    {
        return purificationRate;
    }

}

