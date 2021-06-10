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
    protected GameObject incomingDamager;
    [SerializeField] protected GameObject currentAttackTarget;
    [SerializeField] protected Vector3 currentDest = Vector3.zero;
    protected Detector det;
    protected UnitTracker ut;

    //param
    [SerializeField] float detectorRange;
    [SerializeField] float accelRate_normal;
    [SerializeField] float maxTurnSpeed_normal;
    [SerializeField] float turnAccelRate_normal;

    protected float closeEnough = 0.2f;
    protected float angleThresholdForAccel = 10f;
    protected float timeBetweenScans = 0.1f;

    float param1;
    float param2;
    float param3;

    [SerializeField] protected GameObject weaponPrefab = null;
    [SerializeField] AudioClip[] firingSounds;
    AudioClip selectedFiringSound;


    //hood
    protected float angleToDest;
    protected float distToDest;
    protected float angleToAttackTarget;
    protected float distToAttackTarget;

    protected float timeSinceLastScan = 0;

    public override void OnStartServer()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        ab = FindObjectOfType<ArenaBounds>();
        det = GetComponent<Detector>();
        det.SetDetectorRange(detectorRange);
        timeSinceLastScan = UnityEngine.Random.Range(0, timeBetweenScans);
        ut = FindObjectOfType<UnitTracker>();
        ut.AddMinion(gameObject);
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

    protected virtual void FixedUpdate()
    {

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

    public void WarnOfIncomingDamageDealer(GameObject damager)
    {
        incomingDamager = damager;
    }

    #endregion

    #region Movement

    protected virtual void FlyTowardsDestination()
    {
        if (Mathf.Abs(angleToDest) <= angleThresholdForAccel)
        {
            rb.AddForce(accelRate_normal * transform.up);
            return;
        }
        if (Mathf.Abs(angleToDest) <= angleThresholdForAccel*3)
        {
            rb.AddForce(accelRate_normal / 2 * transform.up);
        }
    }
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
        float factor = Mathf.Abs(angleToDest) / 5;
        factor = Mathf.Clamp01(factor);
        Debug.Log("turn speed: " + maxTurnSpeed_normal * factor);
        if (angleToDest > 0)
        {
            rb.angularVelocity = -1 * maxTurnSpeed_normal * factor;
        }
        if (angleToDest < 0)
        {
            rb.angularVelocity = maxTurnSpeed_normal * factor;
        }
    }

    protected virtual void TurnToFaceDestination()
    {
        if (angleToDest > 5)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, -maxTurnSpeed_normal, turnAccelRate_normal * Time.deltaTime);
        }
        if (angleToDest < -5)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, maxTurnSpeed_normal, turnAccelRate_normal * Time.deltaTime);
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

    #region Attacking
    protected void SelectFiringSound()
    {
        if (firingSounds.Length == 0) { return; }
        int random = UnityEngine.Random.Range(0, firingSounds.Length);
        selectedFiringSound = firingSounds[random];
    }

    #endregion

    #region LevelScaling

    public void SetParam1(float value)
    {
        param1 = value;
    }

    public void SetParam2(float value)
    {
        param2 = value;
    }

    public void SetParam3(float value)
    {
        param3 = value;
    }

    #endregion

    [Server]
    protected virtual void OnDestroy()
    {
        ut.RemoveMinion(gameObject);   
    }




}
