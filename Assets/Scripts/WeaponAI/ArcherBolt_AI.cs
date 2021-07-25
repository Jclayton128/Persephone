using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class ArcherBolt_AI : NetworkBehaviour
{
    //init
    Rigidbody2D rb;
    [SerializeField] GameObject targetGO;

    //param
    float maxTurnSpeed = 90f; //deg per second against fully ionized target
    float searchRadius = 20f;
    int enemyLayer_Pri = 10;
    int enemyLayer_Sec = 18;
    int enemyLayer;

    //hood
    float speed;



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyLayer = enemyLayer_Pri;
    }

    // Update is called once per frame
    void Update()
    {
        if (speed == 0)
        {
            speed = rb.velocity.magnitude;
        }

        if (isServer)
        {
            LookForTarget();
            TurnTowardsTarget();
            rb.velocity = transform.up * speed;
        }
    }

    private void LookForTarget()
    {
        if (!targetGO)
        {
            Debug.Log("looking");
            targetGO = CUR.GetNearestGameObjectOnLayer(transform.position, enemyLayer, searchRadius);
        }
    }

    private void TurnTowardsTarget()
    {
        if (targetGO)
        {
            Vector2 dir = (targetGO.transform.position - transform.position);
            Quaternion targetRot = Quaternion.LookRotation(Vector3.forward, dir);
            Quaternion rot = Quaternion.RotateTowards(transform.rotation, targetRot, maxTurnSpeed * Time.deltaTime);
            transform.rotation = rot;
        }
    }
}
