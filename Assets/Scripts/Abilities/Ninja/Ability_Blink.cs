using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Ability_Blink : Ability
{
    [SerializeField] Sprite[] upgradedAbilityIcons = null;
    GameObject currentBlinkShadow;
    GameObject warpPortalExit;
    SpriteRenderer sr;
    PlayerInput pi;
    Rigidbody2D rb;

    float blinkRate = 1.0f;
    [SyncVar]
    float blinkFactor = 1; //
    float postBlinkFactor = 0;

    int blinkAbilityLevel = 0; // 0 = basic blink, 1 = nova blink, 2 = blaze nova blink
    public bool IsBlinking = false;
    [SyncVar (hook = nameof(HandlePostBlinkOnClient))]
    bool inPostBlink = false;

    Vector3 blinkToPos;
    Vector3 dirToFacePostBlink;

    public override void OnStartServer()
    {
        base.OnStartServer();
        sr = GetComponent<SpriteRenderer>();
        pi = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
    }
    protected override void MouseClickDownEffect()
    {
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
            Blink();
        }
    }

    private void Blink()
    {
        transform.position = blinkToPos;
        Destroy(warpPortalExit);
        IsBlinking = false;
        blinkFactor = 1;
        inPostBlink = true;
        postBlinkFactor = 0;
        float ang = Mathf.Atan2(dirToFacePostBlink.y, dirToFacePostBlink.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.AngleAxis(ang, Vector3.forward);
        rb.velocity = Vector2.zero;

    }

    public void AdvanceBlinkAbilityLevel()
    {
        blinkAbilityLevel++;
        blinkAbilityLevel = Mathf.Clamp(blinkAbilityLevel, 0, 2);
    }
}
