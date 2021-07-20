using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Ability_Blink : Ability
{
    [SerializeField] float novaBlinkIonizationDamage;
    GameObject warpPortalExit;
    SpriteRenderer sr;
    PlayerInput pi;
    Rigidbody2D rb;

    float blinkRate = 2.0f;
    [SyncVar]
    float blinkFactor = 1; //
    float postBlinkFactor = 0;

    enum BlinkAbilityLevel {Basic, Nova, BlazeNova};
    [SyncVar]
    BlinkAbilityLevel blinkAbilityLevel = BlinkAbilityLevel.Basic;
    public bool IsBlinking = false;
    [SyncVar (hook = nameof(HandlePostBlinkOnClient))]
    bool inPostBlink = false;

    Vector3 blinkToPos;
    Vector3 dirToFacePostBlink;
    Vector3 blinkFromPos;

    public override void OnStartServer()
    {
        base.OnStartServer();
        sr = GetComponent<SpriteRenderer>();
        pi = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
    }
    protected override void MouseClickDownEffect()
    {
        blinkFromPos = transform.position;
        blinkToPos = MouseHelper.GetMouseCursorLocation();
        CmdRequestBlink(blinkToPos);

    }

    [Command]
    private void CmdRequestBlink(Vector3 targetPos)
    {
        if (!IsBlinking && es.CheckSpendEnergy(costToActivate))
        {
            blinkToPos = targetPos;
            warpPortalExit = Instantiate(abilityPrefabs[0], targetPos, transform.rotation) as GameObject;
            //warpPortalExit.transform.localScale = Vector3.one * 0.25f;
            NetworkServer.Spawn(warpPortalExit);
            BeginBlinking();
        }
    }

    private void BeginBlinking()
    {
        IsBlinking = true;
        blinkFactor = 1;
    }

    protected override void MouseClickUpEffect()
    {
        Vector3 outPos = MouseHelper.GetMouseCursorLocation();
        Vector3 dir = (outPos - blinkToPos).normalized;

        CmdRequestInstantRotate(dir);
    }

    [Command]
    private void CmdRequestInstantRotate(Vector3 dir)
    {
        if (!IsBlinking) { return; }
        dirToFacePostBlink = dir;
    }

    private void Update()
    {
        if (isServer)
        {
            HandleBlinkingOnServer();
            HandlePostBlinkOnServer();
        }
        if (isClient)
        {
            HandleBlinkingOnClient();
        }
    }

    private void HandlePostBlinkOnClient(bool val1, bool val2)
    {
        if (inPostBlink)
        {
            am.ToggleStatusIcon(this, true);
        }
        if (!inPostBlink)
        {
            am.ToggleStatusIcon(this, false);
        }
    }

    private void HandlePostBlinkOnServer()
    {
        if (inPostBlink)
        {
            postBlinkFactor += Time.deltaTime * blinkFactor;
            postBlinkFactor = Mathf.Clamp01(postBlinkFactor);
            pi.SetPerformanceFactor(postBlinkFactor);
        }
        if (postBlinkFactor == 1)
        {
            inPostBlink = false;
            pi.SetPerformanceFactor(1);
        }
    }

    private void HandleBlinkingOnClient()
    {
        if (IsBlinking)
        {
            sr.color = new Color(1, 1, 1, blinkFactor);
        }
        if (blinkFactor == 1)
        {
            sr.color = Color.white;
        }
        
        
    }

    private void HandleBlinkingOnServer()
    {
        if (IsBlinking)
        {
            blinkFactor -= Time.deltaTime * blinkRate;
            blinkFactor = Mathf.Clamp01(blinkFactor);
        }
        if (blinkFactor == 0)
        {
            HandleBlink();
        }
    }

    protected virtual void HandleBlink()
    {
        switch (blinkAbilityLevel)
        {
            case BlinkAbilityLevel.Basic:
                ExecuteBasicBlink();
                return;

            case BlinkAbilityLevel.Nova:
                ExecuteNovaBlink();
                return;

            case BlinkAbilityLevel.BlazeNova:
                ExecuteBlazeNovaBlink();
                return;

            default:
                Debug.Log("Unexpected input!");
                return;
        }            

    }

    private void ExecuteBasicBlink()
    {
        transform.position = blinkToPos;
        //Destroy(warpPortalExit);
        IsBlinking = false;
        blinkFactor = 1;
        inPostBlink = true;
        postBlinkFactor = 0;
        float ang = Mathf.Atan2(dirToFacePostBlink.y, dirToFacePostBlink.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.AngleAxis(ang, Vector3.forward);
        rb.velocity = Vector2.zero;

    }

    private void ExecuteNovaBlink()
    {
        ExecuteBasicBlink();
        int shrapnelCount = 24;
        float circleSubdivided = 360 / shrapnelCount;
        for (int i = 1; i <= shrapnelCount; i++)
        {
            Quaternion sector = Quaternion.Euler(0, 0, i * circleSubdivided + transform.eulerAngles.z + (weaponSpeed / 2) + 180);
            GameObject newShrapnel = Instantiate(abilityPrefabs[1], transform.position, sector) as GameObject;
            newShrapnel.layer = 9;
            newShrapnel.transform.localScale = Vector3.one * 0.75f;
            newShrapnel.GetComponent<Rigidbody2D>().velocity = newShrapnel.transform.up * weaponSpeed;
            newShrapnel.GetComponent<DamageDealer>().SetIonization(novaBlinkIonizationDamage);
            NetworkServer.Spawn(newShrapnel);
            Destroy(newShrapnel, weaponLifetime);
        }
    }

    private void ExecuteBlazeNovaBlink()
    {
        ExecuteNovaBlink();
        int shrapnelCount = 24;
        float circleSubdivided = 360 / shrapnelCount;
        for (int i = 1; i <= shrapnelCount; i++)
        {
            Quaternion sector = Quaternion.Euler(0, 0, i * circleSubdivided + transform.eulerAngles.z + (weaponSpeed / 2) + 180);
            GameObject newShrapnel = Instantiate(abilityPrefabs[2], blinkFromPos, sector) as GameObject;
            newShrapnel.layer = 9;
            newShrapnel.transform.localScale = Vector3.one;
            newShrapnel.GetComponent<Rigidbody2D>().velocity = newShrapnel.transform.up * weaponSpeed;
            newShrapnel.GetComponent<DamageDealer>().SetNormalDamage(normalDamage);
            NetworkServer.Spawn(newShrapnel);
            Destroy(newShrapnel, weaponLifetime);
        }

    }

    public override bool CheckUnlockOnLevelUp(int newLevel, out int tier)
    {
        if (newLevel >= unlockLevels[0])
        {
            if (newLevel >= unlockLevels[2])
            {
                blinkAbilityLevel = BlinkAbilityLevel.BlazeNova;
                tier = (int)blinkAbilityLevel;
                return true;
            }
            if (newLevel >= unlockLevels[1])
            {
                blinkAbilityLevel = BlinkAbilityLevel.Nova;
                tier = (int)blinkAbilityLevel;
                return true;
            }
            else
            {
                tier = (int)blinkAbilityLevel;
                return true;
            }
        }
        else
        {
            tier = -1;
            return false;
        }
    }

}
