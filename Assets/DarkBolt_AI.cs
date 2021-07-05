using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DarkBolt_AI : NetworkBehaviour
{
    [SerializeField] float maxTurnRate;
    [SerializeField] float snakeAmount;
    float thrustTurning = 100f;
    Vector3 targetDir;
    Rigidbody2D rb;

    public override void OnStartServer()
    {
        base.OnStartServer();
        rb = GetComponent<Rigidbody2D>();
        targetDir = transform.up;
    }

    private void Update()
    {
        if (isServer)
        {
            float angleToTarget = Vector3.SignedAngle(targetDir, transform.up, transform.forward);

            if (angleToTarget == 0)
            {
                rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, -maxTurnRate, thrustTurning * Time.deltaTime);
            }
            if (angleToTarget > snakeAmount)
            {
                rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, -maxTurnRate, thrustTurning * Time.deltaTime);
            }
            if (angleToTarget < -snakeAmount)
            {
                rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, maxTurnRate, thrustTurning * Time.deltaTime);
            }

            rb.velocity = transform.up * rb.velocity.magnitude;
        }
    }
}
