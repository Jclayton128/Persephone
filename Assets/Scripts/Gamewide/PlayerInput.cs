using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AbilityManager))]


public class PlayerInput : NetworkBehaviour
{
    //init
    ClientInstance playerAtThisComputer;
    IFF iff;
    Rigidbody2D rb;
    AbilityManager am;
    Health health;

    //param
    [SerializeField] float accelRate_normal;
    [SerializeField] float drag_normal;
    [SerializeField] float drag_retro;
    [SerializeField] float maxTurnSpeed_normal;
    [SerializeField] float turnAccelRate_normal;
    [SerializeField] ParticleSystem[] engineParticleFX = null;

    [SyncVar]
    float performanceFactor = 1;  //This factor should change how fast a ship moves and turns on a temporary basis.

    [SyncVar (hook = nameof(ToggleEngineFXOnClient))]
    [SerializeField] bool isEmittingEngineFX = false;
    
    float throttleNullThreshold = 0.05f;
    float scrollSensitivity = 0.1f;
    float aimSensitivity = 1.0f;

    //hood
    //[SyncVar]
    //[SerializeField] Vector2 truePosition;
    //[SyncVar]
    //[SerializeField] float trueZRotation;
    //[SerializeField] float correctionRate;

    Vector2 previousAimDir = Vector2.one;
    Vector2 desAimDir = Vector2.zero;
    float desMoveSpeed = 0;
    Vector3 mousePos = Vector3.zero;

    [SyncVar]
    bool isDisabled = false;

    void Start()
    {

        iff = GetComponent<IFF>();
        rb = GetComponent<Rigidbody2D>();
        am = GetComponent<AbilityManager>();
        health = GetComponent<Health>();
        health.EntityIsDying += ReactToBecomingDisabled;
        health.EntityIsRepaired += ReactToBecomingRepaired;
        HookIntoLocalUI();
        ToggleEngineFXOnClient(false, false);
    }
    private void HookIntoLocalUI()
    {
        playerAtThisComputer = ClientInstance.ReturnClientInstance();
    }

    // Update is called once per frame
    void Update()
    {
        if (hasAuthority)
        {
            HandleMouseInput();
            HandleKeyboardInput();
            CmdSendServerDesiredInputs(desAimDir, desMoveSpeed);
           // ReconcileDifferenceBetweenClientAndServerPositionOverTime();
        }
    }

    private void HandleMouseInput()
    {
        UpdateAimDir();
        UpdateGadgetScrolling();
        UpdateMouseClicking();
    }

    private void UpdateAimDir()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        desAimDir = (mousePos - transform.position).normalized;
        float difference = Vector3.Angle(desAimDir, previousAimDir);
        if (difference < aimSensitivity)
        {
            desAimDir = previousAimDir;
        }
        else
        {
            previousAimDir = desAimDir;
        }
    }

    private void UpdateGadgetScrolling()
    {
        if (Input.mouseScrollDelta.y * scrollSensitivity > 0)
        {
            am.ScrollUpThroughAbilities();
        }
        if (Input.mouseScrollDelta.y * scrollSensitivity < 0)
        {
            am.ScrollDownThroughAbilities();
        }
    }

    private void UpdateMouseClicking()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            am.PrimaryAbility.MouseClickDownValidate();
            //gd.primaryGadget.OnClickDown(mousePos, transform);
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            am.PrimaryAbility.MouseClickUpValidate();
            //gd.primaryGadget.OnClickUp(mousePos);
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            am.SelectedSecondaryAbility?.MouseClickDownValidate();
        }
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            am.SelectedSecondaryAbility?.MouseClickUpValidate();
        }
    }


    private void HandleKeyboardInput()
    {
        float throttle = Input.GetAxis("Vertical");
        if (Mathf.Abs(throttle) > throttleNullThreshold)
        {
            desMoveSpeed = throttle;
        }
        else
        {
            desMoveSpeed = Mathf.MoveTowards(desMoveSpeed, 0, Time.deltaTime);
        }

    }

    [Command]
    private void CmdSendServerDesiredInputs(Vector2 aimDir, float speed)
    {
        desAimDir = aimDir;
        desMoveSpeed = speed;
    }


    private void FixedUpdate()
    {
        if (isServer && !isDisabled) 
        {
            ExecuteTurn(desAimDir);
            ExecuteSpeedChange(desMoveSpeed);
            //Vector2 truePosToPush = new Vector2(transform.position.x, transform.position.y);
            //float trueZRotToPush = transform.rotation.eulerAngles.z;
            //RpcPushTruePosAndZRotation(truePosToPush, trueZRotToPush);
        }
        if (isClient)
        {
            //Execute client-side movement for satisfying movement. Not true movement, however.
            //ExecuteTurn(desAimDir);
            //ExecuteSpeedChange(desMoveSpeed);
        }
    }
    //private void ReconcileDifferenceBetweenClientAndServerPositionOverTime()
    //{
    //    transform.position = Vector2.MoveTowards(transform.position, truePosition, correctionRate * Time.deltaTime);
    //}
    //private void ReconcileDifferenceBetweenClientAndServerPositionInstantly()
    //{
    //    transform.position = (Vector3)truePosition;
    //}


    //[ClientRpc]
    //private void RpcPushTruePosAndZRotation(Vector2 newPos, float newZRot)
    //{
    //    truePosition = newPos;
    //    trueZRotation = newZRot;
    //}

    private void ExecuteTurn(Vector2 aimDir)
    {
        //if (isDisabled) { return; }
        float theta = Vector2.SignedAngle(aimDir, transform.up);
        float factor = Mathf.Clamp01(Mathf.Abs(theta/30));
        if (theta > 0.3f)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, -1 * maxTurnSpeed_normal * performanceFactor * factor, turnAccelRate_normal * Time.deltaTime);
            return;
        }
        if (theta < -0.3f)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, maxTurnSpeed_normal * performanceFactor * factor, turnAccelRate_normal * Time.deltaTime);
            return;
        }
        else
        {
            rb.angularVelocity = 0;
        }

    }

    private void ExecuteSpeedChange(float desMoveSpeed)
    {
        //if (isDisabled) { return; }
        if (desMoveSpeed > 0)
        {
            rb.drag = drag_normal;
            rb.AddForce(transform.up * accelRate_normal * performanceFactor);
            isEmittingEngineFX = true;
            return;
        }
        if (desMoveSpeed < 0)
        {
            rb.drag = drag_retro;
            isEmittingEngineFX = false;
            return;
        }
        else
        {
            isEmittingEngineFX = false;
        }

    }


    private void ReactToBecomingDisabled()
    {
        isDisabled = true;
        isEmittingEngineFX = false;
    }

    private void ReactToBecomingRepaired()
    {
        isDisabled = false;
    }

    public void SetPerformanceFactor(float zeroToOneFactor)
    {
        performanceFactor = zeroToOneFactor;
    }

    public bool GetDisabledStatus()
    {
        return isDisabled;
    }

    [Client]
    private void ToggleEngineFXOnClient(bool oldValue, bool currentValue)
    {
        if (currentValue)
        {
            foreach(ParticleSystem ps in engineParticleFX)
            {
                ps.Play();
            }
            return;
        }
        if (!currentValue)
        {
            foreach (ParticleSystem ps in engineParticleFX)
            {
                ps.Stop();
            }
        }
    }

    public void ModifyMobility(float amount)
    {
        accelRate_normal += amount;
        maxTurnSpeed_normal += (amount * 10f);
        turnAccelRate_normal += (amount * 10f);
    }

}
