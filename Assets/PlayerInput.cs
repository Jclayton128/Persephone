using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerInput : NetworkBehaviour
{
    //init
    ClientInstance playerAtThisComputer;
    IFF iff;
    Rigidbody2D rb;

    //param
    [SerializeField] float accelRate_normal;
    [SerializeField] float drag_normal;
    [SerializeField] float drag_retro;
    [SerializeField] float maxTurnSpeed_normal;
    [SerializeField] float turnAccelRate_normal;

    float throttleSensitivity = 0.05f;

    //hood
    [SerializeField] Vector2 desAimDir = Vector2.zero;
    [SerializeField] float desMoveSpeed = 0;
    Vector3 mousePos = Vector3.zero;

    void Start()
    {
        HookIntoLocalUI();
        iff = GetComponent<IFF>();
        rb = GetComponent<Rigidbody2D>();

    }

    private void HookIntoLocalUI()
    {
        playerAtThisComputer = ClientInstance.ReturnClientInstance();
        //uim = FindObjectOfType<UIManager>();
    }



    // Update is called once per frame
    void Update()
    {
        if (hasAuthority)
        {
            HandleMouseInput();
            HandleKeyboardInput();

            CmdSendServerDesiredInputs(desAimDir, desMoveSpeed);
        }
    }

    private void HandleMouseInput()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        desAimDir = (mousePos - transform.position).normalized;
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
}
