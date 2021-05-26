using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Brain : NetworkBehaviour
{
    //init
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected ArenaBounds ab;
    protected List<GameObject> targets = new List<GameObject>();
    protected GameObject currentAttackTarget;
    protected Vector3 currentDest = Vector3.zero;

    //param
    [SerializeField] float accelRate_normal;
    [SerializeField] float drag_normal;
    [SerializeField] float drag_retro;
    [SerializeField] float maxTurnSpeed_normal;
    [SerializeField] float turnAccelRate_normal;

    float closeEnough = 0.2f;
    float angleThresholdForAccel = 10f;
    float timeBetweenScans = 0.2f;

    //hood
    protected float angleToDest;
    protected float distToDest;
    protected float angleToAttackTarget;
    protected float distToAttackTarget;

    protected float timeSinceLastScan = 0;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        ab = FindObjectOfType<ArenaBounds>();
        timeSinceLastScan = Random.Range(0, timeBetweenScans);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (isServer)
        {
            Scan();
        }        
    }

    protected virtual void Scan()
    {
        timeSinceLastScan += Time.deltaTime;
        if (timeSinceLastScan >= timeBetweenScans)
        {
            Vector2 dir = currentDest - transform.position;
            distToDest = dir.magnitude;
            angleToDest = Vector3.SignedAngle(dir, transform.up, transform.forward);
            if (currentAttackTarget)
            {
                Vector2 dir2 = currentAttackTarget.transform.position - transform.position;
                angleToAttackTarget = Vector3.SignedAngle(dir2, transform.up, transform.forward);
                distToAttackTarget = dir2.magnitude;
            }
            timeSinceLastScan = 0;
        }
    }

    #region Targeting
    public void AddTargetToList(GameObject target)
    {
        targets.Add(target);
    }

    public void RemoveTargetFromList(GameObject target)
    {
        targets.Remove(target);
    }

    #endregion

    #region Movement
    protected virtual void FlyTowardsNavTarget(bool adjustForDistanceToTarget)
    {
        if (Mathf.Abs(angleToDest) <= angleThresholdForAccel && adjustForDistanceToTarget == true)
        {
            float distThresh = closeEnough;
            float factor = Mathf.Clamp(distToDest / distThresh, 0, 1);
            rb.AddForce(accelRate_normal * transform.up * Time.timeScale * factor);
        }
        if (Mathf.Abs(angleToDest) <= angleThresholdForAccel && adjustForDistanceToTarget == false)
        {
            rb.AddForce(accelRate_normal * transform.up * Time.timeScale);
        }
    }

    protected virtual void FlyTowardsNavTarget(bool adjustForDistanceToTarget, float distanceToStopAt)
    {
        if (Mathf.Abs(angleToDest) <= angleThresholdForAccel && adjustForDistanceToTarget == true)
        {
            float distThresh = closeEnough;
            float distMod = distToDest - distanceToStopAt;
            float factor = Mathf.Clamp(distMod / distThresh, -1, 1);
            rb.AddForce(accelRate_normal * transform.up * Time.timeScale * factor);
        }
        if (Mathf.Abs(angleToDest) <= angleThresholdForAccel && adjustForDistanceToTarget == false)
        {
            rb.AddForce(accelRate_normal * transform.up * Time.timeScale);
        }
    }

    protected virtual void FlyTowardsNavTargetOmnidirectionally(bool adjustForDistanceToTarget)
    {
        if (adjustForDistanceToTarget)
        {
            float distThresh = closeEnough;
            float factor = Mathf.Clamp01(distToDest / distThresh);
            Vector2 thrustAxis = currentDest - transform.position;
            rb.AddForce(accelRate_normal * thrustAxis * Time.timeScale * factor);
        }
        if (!adjustForDistanceToTarget)
        {
            Vector2 thrustAxis = currentDest - transform.position;
            rb.AddForce(accelRate_normal * thrustAxis * Time.timeScale);
        }
    }
    #endregion

    #region Facing
    protected virtual void TurnToFaceNavTarget() //Optimize to decrease velocity with small angles off;
    {

        float angleWithTurnDamper = Mathf.Clamp(angleToDest, -10, 10);
        float currentTurnRate = Mathf.Clamp(-maxTurnSpeed_normal * angleWithTurnDamper / 10, -maxTurnSpeed_normal, maxTurnSpeed_normal);
        if (angleToDest > 0.02)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, currentTurnRate, turnAccelRate_normal * Time.deltaTime);
        }
        if (angleToDest < -0.02)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, currentTurnRate, turnAccelRate_normal * Time.deltaTime);
        }
        if (Mathf.Abs(angleToDest) <= 0.02)
        {
            //rb.angularVelocity = 0;
        }
    }
    protected void TurnToFacePlayerWithLead(float weaponSpeed)
    {
        throw new NotImplementedException();
        //Vector3 vel = rb.velocity;
        //float timeOfShot = ((player.transform.position + vel) - transform.position).magnitude / weaponSpeed;
        //Vector3 leadPos = player.transform.position + (vel * timeOfShot);
        //Vector2 targetDir = leadPos - transform.position;
        //float angleToTargetFromNorth = Vector3.SignedAngle(targetDir, Vector2.up, transform.forward);
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, -1 * angleToTargetFromNorth), maxTurnRate * Time.deltaTime);
    }
    #endregion


}
