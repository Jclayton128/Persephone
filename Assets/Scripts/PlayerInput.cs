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

    float throttleSensitivity = 0.05f;
    float scrollSensitivity = 0.1f;

    //hood
    [SerializeField] Vector2 desAimDir = Vector2.zero;
    [SerializeField] float desMoveSpeed = 0;
    Vector3 mousePos = Vector3.zero;
    [SerializeField] bool isDisabled = false;

    void Start()
    {

        iff = GetComponent<IFF>();
        rb = GetComponent<Rigidbody2D>();
        am = GetComponent<AbilityManager>();
        health = GetComponent<Health>();
        health.EntityIsDying += ReactToBecomingDisabled;
        health.EntityIsRepaired += ReactToBecomingRepaired;
        HookIntoLocalUI();

    
    }


    private void HookIntoLocalUI()
    {
        playerAtThisComputer = ClientInstance.ReturnClientInstance();
    }



    // Update is called once per frame
    void Update()
    {
        if (hasAuthority && isDisabled == false)
        {
            HandleMouseInput();
            HandleKeyboardInput();

            CmdSendServerDesiredInputs(desAimDir, desMoveSpeed);
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
    }

    private void UpdateGadgetScrolling()
    {
        if (Input.mouseScrollDelta.y * scrollSensitivity < 0)
        {
            am.ScrollUpThruAbilities();
        }
        if (Input.mouseScrollDelta.y * scrollSensitivity > 0)
        {
            am.ScrollUpThruAbilities();
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
            am.SelectedSecondaryAbility.MouseClickDownValidate();
        }
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            am.SelectedSecondaryAbility.MouseClickUpValidate();
        }
    }



    private void HandleKeyboardInput()
    {
        float throttle = Input.GetAxis("Vertical");
        if (throttle > throttleSensitivity)
        {
            desMoveSpeed = throttle;
        }
        if (throttle <= throttleSensitivity)
        {
            desMoveSpeed = 0;
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
        if (isDisabled) { return; }
        if (isServer)
        {
            ExecuteTurn(desAimDir);
            ExecuteSpeedChange(desMoveSpeed);
        }
    }

    private void ExecuteTurn(Vector2 aimDir)
    {
        float theta = Vector2.SignedAngle(aimDir, transform.up);
        if (theta > 0.02)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, -1 * maxTurnSpeed_normal, turnAccelRate_normal * Time.deltaTime);
        }
        if (theta < -0.02)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, maxTurnSpeed_normal, turnAccelRate_normal * Time.deltaTime);
        }
    }

    private void ExecuteSpeedChange(float desMoveSpeed)
    {
        if (desMoveSpeed > 0)
        {
            rb.drag = drag_normal;
            rb.AddForce(transform.up * accelRate_normal);
        }
        if (desMoveSpeed < 0)
        {
            rb.drag = drag_retro;
        }

    }


    private void ReactToBecomingDisabled()
    {
        isDisabled = true;
    }

    private void ReactToBecomingRepaired()
    {
        isDisabled = false;
    }

}
