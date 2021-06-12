using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Missile_AI : NetworkBehaviour
{
    //init
    Rigidbody2D rb;
    GameObject entityThatFiredMissile;
    int myIFFAllegiance;

    //param
    public float normalSpeed;
    public float maxTurnRate = 180f;
    float timeUntilExpired = 5.0f;
    float thrustTurning = 90f;
    float closeEnough = 1.0f;
    //float scanRangeNear = 1.0f;
    float scanRangeMed = 2f;
    float scanRangeFar = 4f;
    float timeBetweenScans = 0.1f;
    public bool shouldSnake = false;
    float snakeAmount = 30f;
    float lifetime;

    //hood
    Vector3 navTarget;
    float angleToTarget;
    float distanceToTarget = 10f;
    public bool hasReachedTarget = false;
    GameObject acquiredTarget = null;
    float timeSinceLastScan = 0f;
    float timeSinceLaunched = 0;

    public override void OnStartServer()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = normalSpeed * transform.up;
        snakeAmount += Random.Range(-snakeAmount / 2, snakeAmount / 2);
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            timeSinceLaunched += Time.deltaTime;
            if (timeSinceLaunched >= lifetime)
            {
                Destroy(gameObject);
            }
            if (!hasReachedTarget)  //!acquiredTarget)
            {
                ScanForGOTarget();
                SteerTowardsNavTarget();
            }
            distanceToTarget = (navTarget - transform.position).sqrMagnitude;

            if (acquiredTarget && hasReachedTarget)
            {
                navTarget = acquiredTarget.transform.position;
                SteerTowardsNavTarget();
            }
            rb.velocity = normalSpeed * transform.up;
            if (distanceToTarget < closeEnough)
            {
                hasReachedTarget = true;
                rb.angularVelocity = 0f;
            }
        }      

    }
    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawSphere(transform.position + transform.up * scanRangeMed * 1, 1.0f);
    //    //Gizmos.DrawSphere(transform.position + transform.up * scanRangeMed * 1, scanRangeMed);
    //    Gizmos.DrawSphere(transform.position + transform.up * scanRangeFar * 1, scanRangeMed);
    //}

    public void SetLifetime(float value)
    {
        lifetime = value;
    }

    public void SetIFFAllegiance(int newAllegiance)
    {
        myIFFAllegiance = newAllegiance;
    }

    private void ScanForGOTarget()
    {
        if (acquiredTarget) { return; }
        timeSinceLastScan += Time.deltaTime;
        if (timeSinceLastScan >= timeBetweenScans)
        {
            RaycastHit2D[] colls_near = Physics2D.CircleCastAll(transform.position, 1.0f, transform.up, 2.0f);
            foreach (RaycastHit2D coll in colls_near)
            {
                if (coll.transform.gameObject != gameObject && coll.transform.gameObject != entityThatFiredMissile)  //universalize based on IFF
                {
                    if (coll.transform.gameObject.GetComponent<Health>() == true)
                    {
                        acquiredTarget = coll.transform.gameObject;
                    }
                }
            }
            RaycastHit2D[] colls_far = Physics2D.CircleCastAll(transform.position, 2.0f, transform.up, 4.0f);
            foreach (RaycastHit2D coll in colls_far)
            {
                if (coll.transform.gameObject != gameObject && coll.transform.gameObject != entityThatFiredMissile)
                {
                    if (coll.transform.gameObject.GetComponent<Health>() == true)
                    {
                        acquiredTarget = coll.transform.gameObject;
                    }
                }
            }
            timeSinceLastScan = 0f;
        }
    }

    public void SetMissileTarget(GameObject target)
    {
        acquiredTarget = target;
    }

    public void SetMissileOwner(GameObject owner)
    {
        entityThatFiredMissile = owner;
    }
    public void SetNavTarget(Vector2 target)
    {
        navTarget = target;
    }
    private void SteerTowardsNavTarget()
    {
        Vector3 targetDir = navTarget - transform.position;
        angleToTarget = Vector3.SignedAngle(targetDir, transform.up, transform.forward);
        if (!shouldSnake)
        {
            if (angleToTarget > 1)
            {
                rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, -maxTurnRate, thrustTurning * Time.deltaTime);
            }
            if (angleToTarget < -1)
            {
                rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, maxTurnRate, thrustTurning * Time.deltaTime);
            }
        }
        if (shouldSnake)
        {
            if (angleToTarget > snakeAmount)
            {
                rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, -maxTurnRate, thrustTurning * Time.deltaTime);
            }
            if (angleToTarget < -snakeAmount)
            {
                rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, maxTurnRate, thrustTurning * Time.deltaTime);
            }
        }

    }

    private void OnDestroy()
    {
        //TODO create an explosion?
        //Play a explosion sound?
    }
}
