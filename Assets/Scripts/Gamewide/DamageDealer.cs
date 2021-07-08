using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    float regularDamage = 0;
    float directionalBonus = 0;
    float shieldBonusDamage = 0;
    float ionization = 0;
    float draining = 0;
    float knockBack = 0;
    float speedModifier = 0; //this is a speed multiplier. 0 creates a sudden halt, 2 creates a doubling of speed.

    GameObject owner = null;
    int penetration = 1;
    public bool UsesDirectionalBonus { private get; set; } = false;

    public GameObject particleExplosionAtImpact = null;

    //public bool IsReal = false;

    #region Regular Damage Traits

    public void SetShieldBonusDamage(float value)
    {
        shieldBonusDamage = value;
    }

    public void SetNormalDamage(float value)
    {
        regularDamage = value;
    }

    public void SetIonization(float value)
    {
        ionization = value;
    }

    public void SetDraining(float value)
    {
        draining = value;
    }
     public void SetKnockback(float amount)
    {
        knockBack = amount;
    }
    public void SetSpeedModifier(float amount)
    {
        speedModifier = amount;
    }

    public Damage GetDamage(Transform transformOfThingDamaged)
    {
        Damage damage = new Damage();

        if (UsesDirectionalBonus)
        {
            float diff = Mathf.Abs(Quaternion.Angle(transform.rotation, transformOfThingDamaged.rotation));
            float diffFactor = 1 - (diff / 180);
            damage.RegularDamage = regularDamage * diffFactor;
        }
        else
        {
            damage.RegularDamage = regularDamage;
        }

        damage.ShieldBonusDamage = shieldBonusDamage;
        damage.Ionization = ionization;
        damage.KnockbackAmount = knockBack;
        damage.SpeedModifier = speedModifier;
        return damage;
    }

    #endregion

    #region Speed Modification


    public float GetSpeedModifier()
    {
        return speedModifier;
    }
    #endregion 

    #region Penetration
    public int GetPenetration()
    {
        return penetration;
    }

    public void SetPenetration(int amount)
    {
        penetration = amount;
    }

    
    public void ModifyPenetration(int amount)
    {

        penetration += amount;
        if (penetration <= 0)
        {
            Destroy(gameObject);
        }
    }

    #endregion

    public void SetOwnership(GameObject obj)
    {
        owner = obj;
    }

    public GameObject GetOwner()
    {
        return owner;
    }



}

