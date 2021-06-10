using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    public float damage = 0;
    bool hasKnockBack = false;
    float knockBackAmount = 0;
    float speedModifier = 0; //this is a speed multiplier. 0 creates a sudden halt, 2 creates a doubling of speed.
    GameObject safeObject = null;
    int penetration = 1;
    float bonusScrapThreshold = 0; // between 0 and 10;
    public GameObject particleExplosionAtImpact = null;

    public bool IsReal = false;

    public float GetBonusScrapThreshold()
    {
        return bonusScrapThreshold;
    }

    public void SetBonusScrapThreshold(int amount)
    {
        bonusScrapThreshold = amount;
    }

    public void SetSpeedModifier(float amount)
    {
        speedModifier = amount;
    }

    public float GetSpeedModifier()
    {
        return speedModifier;
    }

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
    }
    public void SetSafeObject(GameObject obj)
    {
        safeObject = obj;
    }

    public GameObject GetSafeObject()
    {
        return safeObject;
    }

    public float GetDamage()
    {
        return damage;
    }

    public void SetDamage(float value)
    {
        damage = value;
    }

    public void SetKnockBack(bool trueIfHasKnockback)
    {
        hasKnockBack = trueIfHasKnockback;
    }

    public bool GetKnockBack()
    {
        return hasKnockBack;
    }

    public float GetKnockBackAmount()
    {
        return knockBackAmount;
    }

    public void SetKnockBackAmount(float amount)
    {
        knockBackAmount = amount;
    }
}

