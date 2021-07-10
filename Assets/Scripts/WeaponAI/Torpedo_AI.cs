using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Torpedo_AI : MonoBehaviour
{
    Rigidbody2D rb;
    [SerializeField] GameObject payloadPrefab = null;

    Vector3 targetPoint;
    public float normalSpeed;
    public float maxTurnRate;
    float thrustTurning = 90f;
    float closeEnough = 1.0f;

    float distToDest;
    float angleToDest;

    private void Awake()
    {
        if (!NetworkClient.prefabs.ContainsValue(payloadPrefab))
        {
            NetworkClient.RegisterPrefab(payloadPrefab);
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetTargetPoint(Vector2 newTargetPoint)
    {
        targetPoint = newTargetPoint;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateNavData();
        SteerTowardsDestination();
        rb.velocity = transform.up * normalSpeed;

        HandleReachingDestination();

    }

    private void UpdateNavData()
    {
        Vector3 dir = targetPoint - transform.position;
        distToDest = dir.magnitude;
        angleToDest = Vector3.SignedAngle(dir, transform.up, transform.forward);
    }
    private void SteerTowardsDestination()
    {
        if (angleToDest > 1)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, -maxTurnRate, thrustTurning * Time.deltaTime);
        }
        if (angleToDest < -1)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, maxTurnRate, thrustTurning * Time.deltaTime);
        }
    }
    private void HandleReachingDestination()
    {
        if (distToDest < closeEnough)
        {
            Detonate();
        }
    }

    public void Detonate()
    {
        if (payloadPrefab)
        {
            GameObject payload = Instantiate(payloadPrefab, transform.position, transform.rotation) as GameObject;
            NetworkServer.Spawn(payload);
        }
        Destroy(gameObject);
    }


}
