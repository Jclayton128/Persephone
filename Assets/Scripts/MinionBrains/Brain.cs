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
    private enum TargetSortMode {ClosestAllyFirst, MostImportantAllyFirst, MostIonizedPlayerFirst, MostHealthyPlayerFirst, InOrderOfDetection }
    [SerializeField] TargetSortMode targetSortMode;
    private enum TargetingPriority {FirstInList, LastInList};
    [SerializeField] TargetingPriority targetingPriority;
    private enum IdleNavBehaviour { WanderAroundThenAttackTarget, WanderAroundAndIgnoreTargets, WanderSaveTargetAttackWhenCharged ,EightDirFencing}
    [SerializeField] IdleNavBehaviour idleNavBehaviour;


    [SerializeField] protected float detectorRange;
    [SerializeField] protected float accelRate_normal;
    [SerializeField] protected float maxTurnSpeed_normal;
    [SerializeField] protected float turnAccelRate_normal;
    [SerializeField] protected FaceMode faceMode;
    [SerializeField] protected MoveMode moveMode;
    [SerializeField] protected float stoppingDist;
    protected enum MoveMode { General, Precise};


    [SerializeField] protected GameObject weaponPrefab = null;
    [SerializeField] protected float weaponLifetime;
    [SerializeField] protected float weaponSpeed;
    [SerializeField] protected float weaponTurnRate;
    [SerializeField] protected float intervalBetweenWeapons;

    [SerializeField] protected float weaponNormalDamage;
    [SerializeField] protected float weaponShieldBonusDamage;
    [SerializeField] protected float weaponKnockback;
    [SerializeField] protected float weaponIonization;
    [SerializeField] protected float weaponSpeedMod;

    protected float closeEnough = 0.2f;
    protected float angleThresholdForAccel = 10f;
    protected float timeBetweenScans = 0.5f;
    protected float boresightThreshold = 2f;
    protected float timeOfNextWeapon = 0;
    protected float ionizationAttackRatePenaltyCoeff = 4; // being fully ionized (1.0) means that your attack require 4x as much time to recharge.
    [SerializeField] protected bool weaponIsCharged;

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
    protected float attackRange;
    protected float timeOfNextScan = 0;

    protected virtual void Awake()
    {
        NetworkClient.RegisterPrefab(weaponPrefab);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        rb = GetComponent<Rigidbody2D>();

        ab = FindObjectOfType<ArenaBounds>();
        det = GetComponent<Detector>();
        det.SetDetectorRange(detectorRange);
        timeOfNextScan = UnityEngine.Random.Range(0, timeBetweenScans);
        ut = FindObjectOfType<UnitTracker>();
        ut.AddMinion(gameObject);
        muz = GetComponent<Muzzle>();
        health = GetComponent<Health>();
        currentDest = transform.position;
        attackRange = weaponLifetime * weaponSpeed;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (isServer)
        {
            TrackTimeBetweenScans();
            SelectBestTarget();
            UpdateNavData();
            ExecuteIdleNavigationBehavior();
        }        
    }

    protected void TrackTimeBetweenScans()
    {
       
        if (Time.time >= timeOfNextScan)
        {
            Scan();
            timeOfNextScan = Time.time + timeBetweenScans;
        }
    }

    protected virtual void Scan()
    {
        det.HiderSpotCheck(detectorRange);
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
        else
        {
            distToAttackTarget = Mathf.Infinity;
            angleToAttackTarget = 0;
        }
    }


    #region Targeting
    protected virtual void SelectBestTarget()
    {
        if (targets.Count == 0) { return; }
        if (targets[0].GetCurrentImportance() == 0)
        {
            RemoveTargetFromList(targets[0]);
            return;
        }
        switch (targetingPriority)
        {
            case TargetingPriority.FirstInList:
                //TODO implement this sorting
                currentAttackTarget = targets[0].gameObject;
                return;

            case TargetingPriority.LastInList:
                int last = targets.Count - 1;
                currentAttackTarget = targets[last].gameObject;
                return;
        }
    }

    public virtual void CheckAddTargetToList(IFF target)
    {
        if (targetSortMode == TargetSortMode.MostHealthyPlayerFirst || targetSortMode == TargetSortMode.MostIonizedPlayerFirst)
        {
            if (target.IsPersephone)
            {
                return;
            }
        }

        if (!targets.Contains(target))
        {
            targets.Add(target);
            ResortList();
        }
    }

    public virtual void RemoveTargetFromList(IFF target)
    {
        targets.Remove(target);
        if (targets.Count == 0)
        {
            currentAttackTarget = null;
        }
    }

    public virtual void WarnOfIncomingDamageDealer(GameObject damager)
    {
        incomingDamager = damager;
    }

    public virtual void ResortList()
    {
        IFF iif = new IFF();
        switch (targetSortMode)
        {
            case TargetSortMode.MostImportantAllyFirst:
                targets.Sort(IFF.CompareByImportance);
                return;

            case TargetSortMode.ClosestAllyFirst:
                //TODO figure out how to sort by distance well.
                return;

            case TargetSortMode.MostHealthyPlayerFirst:
                targets.Sort(IFF.CompareByHealthLevel);
                return;

            case TargetSortMode.MostIonizedPlayerFirst:
                targets.Sort(IFF.CompareByIonization);
                return;

            case TargetSortMode.InOrderOfDetection:
                // Do nothing; the list should already be in order of detection by default
                return;


        }
        targets.Sort(iif);
    }

    #endregion

    #region Navigation Behaviour
    // Navigation Behaviours exist solely as a strategy to update a Brain's current Destination.
    private void ExecuteIdleNavigationBehavior()
    {
        switch (idleNavBehaviour)
        {
            case IdleNavBehaviour.WanderAroundThenAttackTarget:
                NavBehaviour_WanderAroundThenAttackTarget();
                return;

            case IdleNavBehaviour.WanderAroundAndIgnoreTargets:
                NavBehaviour_WanderAroundAndIgnoreTarget();
                return;

            case IdleNavBehaviour.WanderSaveTargetAttackWhenCharged:
                NavBehaviour_WanderSaveTargetAttackWhenCharged();
                return;

            case IdleNavBehaviour.EightDirFencing:
                NavBehaviour_EightDirFencing();
                return;

        }
    }

    private void NavBehaviour_WanderAroundThenAttackTarget()
    {
        if (currentAttackTarget)
        {
            currentDest = currentAttackTarget.transform.position;
            return;
        }
        if (!currentAttackTarget)
        {
            if (distToDest < closeEnough)
            {
                currentDest = ab.CreateRandomPointWithinArena();
            }
        }
    }

    private void NavBehaviour_WanderAroundAndIgnoreTarget()
    {
        if (distToDest < closeEnough)
        {
            currentDest = ab.CreateRandomPointWithinArena();
        }
    }

    private void NavBehaviour_WanderSaveTargetAttackWhenCharged()
    {
        if (currentAttackTarget && weaponIsCharged)
        {
            currentDest = currentAttackTarget.transform.position;
            return;
        }
        if (!currentAttackTarget || !weaponIsCharged)
        {
            if (distToDest < attackRange * 4f)
            {
                currentDest = ab.CreateRandomPointWithinArena();
            }

        }
    }

    private void NavBehaviour_EightDirFencing()
    {
        float newPositionDistanceAway = 4.0f;  //This is the minimum straightline distance between turn decisions for 8-dir fencing.
        if (distToDest >= closeEnough)
        {
            Debug.DrawLine(transform.position, currentDest, Color.red);
            return;
        }
        if (distToDest < closeEnough)
        {

            int leftStraightRight = UnityEngine.Random.Range(0, 3);
            if (leftStraightRight == 0)
            {
                rb.velocity = transform.up * 0;
                if (GetCardinalDirection_Helper() == 1)
                {
                    //Debug.Log("going N, choose NW");
                    currentDest = transform.position + new Vector3(-newPositionDistanceAway, newPositionDistanceAway, 0);
                }
                if (GetCardinalDirection_Helper() == 2)
                {
                    //Debug.Log("going E, choose NE");
                    currentDest = transform.position + new Vector3(newPositionDistanceAway, newPositionDistanceAway, 0);
                }
                if (GetCardinalDirection_Helper() == 3)
                {
                    //Debug.Log("going S, choose SE");
                    currentDest = transform.position + new Vector3(newPositionDistanceAway, -newPositionDistanceAway, 0);
                }
                if (GetCardinalDirection_Helper() == 4)
                {
                    //Debug.Log("going W, choose SW");
                    currentDest = transform.position + new Vector3(-newPositionDistanceAway, -newPositionDistanceAway, 0);
                }
            }
            if (leftStraightRight == 1)
            {
                rb.velocity = transform.up * 0;
                if (GetCardinalDirection_Helper() == 1)
                {
                    //Debug.Log("going N, choose N");
                    currentDest = transform.position + new Vector3(0, newPositionDistanceAway, 0);
                }
                if (GetCardinalDirection_Helper() == 2)
                {
                    //Debug.Log("going E, choose E");
                    currentDest = transform.position + new Vector3(newPositionDistanceAway, 0, 0);
                }
                if (GetCardinalDirection_Helper() == 3)
                {
                    //Debug.Log("going S, choose S");
                    currentDest = transform.position + new Vector3(0, -newPositionDistanceAway, 0);
                }
                if (GetCardinalDirection_Helper() == 4)
                {
                    //Debug.Log("going W, choose W");
                    currentDest = transform.position + new Vector3(-newPositionDistanceAway, 0, 0);
                }
            }
            if (leftStraightRight == 2)
            {
                rb.velocity = transform.up * 0;
                if (GetCardinalDirection_Helper() == 1)
                {
                    //Debug.Log("going N, choose NE");
                    currentDest = transform.position + new Vector3(newPositionDistanceAway, newPositionDistanceAway, 0);
                }
                if (GetCardinalDirection_Helper() == 2)
                {
                    //Debug.Log("going E, choose SE");
                    currentDest = transform.position + new Vector3(newPositionDistanceAway, -newPositionDistanceAway, 0);
                }
                if (GetCardinalDirection_Helper() == 3)
                {
                    //Debug.Log("going S, choose SW");
                    currentDest = transform.position + new Vector3(-newPositionDistanceAway, -newPositionDistanceAway, 0);
                }
                if (GetCardinalDirection_Helper() == 4)
                {
                    //Debug.Log("going W, choose NW");
                    currentDest = transform.position + new Vector3(-newPositionDistanceAway, newPositionDistanceAway, 0);
                }
            }
            currentDest = ab.CheckPoint_CreateMoreCenteredPoint(currentDest);
        }
    }

    private int GetCardinalDirection_Helper()
    {
        float angleFromNorth = Vector3.SignedAngle(transform.up, Vector3.up, Vector3.forward);
        int cardinalDirection = 0;
        if (Mathf.Abs(angleFromNorth) <= 45)
        {
            cardinalDirection = 1;
        }
        if (angleFromNorth > 45 && angleFromNorth <= 135)
        {
            cardinalDirection = 2;
        }
        if (Mathf.Abs(angleFromNorth) > 135)
        {
            cardinalDirection = 3;
        }
        if (angleFromNorth < -45 && angleFromNorth >= -135)
        {
            cardinalDirection = 4;
        }

        return cardinalDirection;
    }


    #endregion

    protected virtual void FixedUpdate()
    {

    }

    #region Movement

    protected virtual void MoveTowardsNavTarget()
    {
        switch (moveMode)
        {
            case MoveMode.General:
                if (Mathf.Abs(angleToDest) <= angleThresholdForAccel)
                {
                    rb.AddForce(accelRate_normal * transform.up);
                    return;
                }
                if (Mathf.Abs(angleToDest) <= angleThresholdForAccel * 3)
                {
                    rb.AddForce(accelRate_normal / 2 * transform.up);
                }
                return;

            case MoveMode.Precise:
                if (Mathf.Abs(angleToDest) <= angleThresholdForAccel)
                {
                    rb.AddForce(accelRate_normal * transform.up);
                }
                return;
        }

    }
    protected virtual void MoveTowardsNavTarget(float distToStop)
    {
        if (Mathf.Abs(angleToDest) <= angleThresholdForAccel && distToStop > 0)
        {
            float factor = Mathf.Clamp01(distToDest - distToStop);
            rb.AddForce(accelRate_normal * transform.up * Time.timeScale * factor);
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

    protected float ReturnAttackTimePenaltyDueToIonization()
    {
        float attackTimePenalty = ionizationAttackRatePenaltyCoeff * health.IonFactor;
        return attackTimePenalty;
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
