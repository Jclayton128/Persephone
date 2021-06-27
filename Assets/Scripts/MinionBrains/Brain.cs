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
    [SerializeField] protected List<IFF> targets = new List<IFF>();
    protected GameObject incomingDamager;
    [SerializeField] protected GameObject currentAttackTarget;
    protected Vector3 currentDest = Vector3.zero;
    protected Detector det;
    protected UnitTracker ut;
    protected Muzzle muz;
    protected Health health;

    //param
    [SerializeField] protected float detectorRange;
    [SerializeField] protected float accelRate_normal;
    [SerializeField] protected float maxTurnSpeed_normal;
    [SerializeField] protected float turnAccelRate_normal;
    [SerializeField] protected FaceMode faceMode;

    [SerializeField] protected GameObject weaponPrefab = null;
    [SerializeField] protected float weaponLifetime;
    [SerializeField] protected float weaponSpeed;
    [SerializeField] protected float weaponTurnRate;
    [SerializeField] protected float timeBetweenShots;

    [SerializeField] protected float weaponNormalDamage;
    [SerializeField] protected float weaponShieldBonusDamage;
    [SerializeField] protected float weaponKnockback;
    [SerializeField] protected float weaponIonization;
    [SerializeField] protected float weaponSpeedMod;

    protected float closeEnough = 0.2f;
    protected float angleThresholdForAccel = 10f;
    protected float timeBetweenScans = 0.1f;
    protected float boresightThreshold = 2f;

    public enum FaceMode { complex, simple};

    float param1;
    float param2;
    float param3;


    [SerializeField] AudioClip[] firingSounds;
    AudioClip selectedFiringSound;


    //hood
    protected float angleToDest;
    protected float distToDest;
    protected float angleToAttackTarget;
    protected float distToAttackTarget;

    protected float timeSinceLastScan = 0;

    protected virtual void Awake()
    {
        NetworkClient.RegisterPrefab(weaponPrefab);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        ab = FindObjectOfType<ArenaBounds>();
        det = GetComponent<Detector>();
        det.SetDetectorRange(detectorRange);
        timeSinceLastScan = UnityEngine.Random.Range(0, timeBetweenScans);
        ut = FindObjectOfType<UnitTracker>();
        ut.AddMinion(gameObject);
        muz = GetComponent<Muzzle>();
        health = GetComponent<Health>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (isServer)
        {
            TimeBetweenScans();
            UpdateNavData();
        }        
    }


    protected void TimeBetweenScans()
    {
        timeSinceLastScan += Time.deltaTime;
        if (timeSinceLastScan >= timeBetweenScans)
        {
            Scan();
            timeSinceLastScan = 0;
        }
    }

    protected virtual void Scan()
    {
        
    }
    private void UpdateNavData()
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
    }

    protected virtual void FixedUpdate()
    {

    }

    #region TargetPrioritization

    public enum TargetingMode { closest, mostImportant, lowestHealth, mostIonized }
    public void SelectBestTarget(TargetingMode mode)
    {
        if (targets.Count == 0) { return; }
        switch (mode)
        {
            case TargetingMode.closest:
                //TODO implement this sorting
                currentAttackTarget = targets[0].gameObject;
                return;

            case TargetingMode.mostImportant:
                currentAttackTarget = targets[0].gameObject;
                return;

            case TargetingMode.lowestHealth:
                currentAttackTarget = targets[0].gameObject;
                return;

            case TargetingMode.mostIonized:
                currentAttackTarget = targets[0].gameObject;
                return;
        }
    }

    #endregion

    #region Targeting
    public void AddTargetToList(IFF target)
    {
        targets.Add(target);
        ResortListBasedOnImportance();
    }

    public void RemoveTargetFromList(IFF target)
    {
        targets.Remove(target);
    }

    public virtual void WarnOfIncomingDamageDealer(GameObject damager)
    {
        incomingDamager = damager;
    }

    public void ResortListBasedOnImportance()
    {
        IFF iif = new IFF();
        targets.Sort(iif);
    }

    #endregion

    #region Movement

    protected virtual void MoveTowardsNavTarget()
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
    protected virtual void MoveTowardsNavTarget(bool adjustForDistanceToTarget)
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

    protected virtual void MoveTowardsNavTarget(bool adjustForDistanceToTarget, float distanceToStopAt)
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

    protected virtual void MoveTowardsNavTargetOmnidirectionally(bool adjustForDistanceToTarget)
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
    protected virtual void TurnToFaceDestination(FaceMode mode) //Optimize to decrease velocity with small angles off;
    {
        if (mode == FaceMode.simple)
        {
            float factor = Mathf.Abs(angleToDest) / boresightThreshold;
            factor = Mathf.Clamp01(factor);
            if (angleToDest > 0)
            {
                rb.angularVelocity = -1 * maxTurnSpeed_normal * factor;
            }
            if (angleToDest < 0)
            {
                rb.angularVelocity = maxTurnSpeed_normal * factor;
            }
            return;
        }
    
        if (mode == FaceMode.complex)
        {
            if (angleToDest > 0)
            {
                rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, -maxTurnSpeed_normal, turnAccelRate_normal * Time.deltaTime);
            }
            if (angleToDest <= 0)
            {
                rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, maxTurnSpeed_normal, turnAccelRate_normal * Time.deltaTime);
            }
            Debug.Log("angVel: " + rb.angularVelocity);
            return;
        }

    }
    protected void TurnToFaceDestinationWithLead(float weaponSpeed)
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
