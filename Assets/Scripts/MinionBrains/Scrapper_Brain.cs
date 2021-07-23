using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Scrapper_Brain : Brain
{
    GameObject homeAsteroid;
    GameObject previousHomeAsteroid;
    [SerializeField] GameObject scrapTarget;
    ScrapCollector sc;
    [SerializeField] GameObject particlesWhileBuildingTurret = null;
    float timeRequiredToBuildTurret = 20f;
    float timeBonusPerScrap = 10f;

    //hood
    [SerializeField] float timeSpentBuildingAtHome = 0;
    bool isCarryingScrap = false;

    public enum Mode { Homeless, SeekScrapWhileAtHome, CollectScrap, ReturnHomeWithScrap}
    [SerializeField] Mode mode = Mode.Homeless;

    public override void OnStartServer()
    {
        base.OnStartServer();
        sc = GetComponent<ScrapCollector>();
        sc.OnScrapPickup += PickUpScrap;
        closeEnough = 1f;
    }


    #region Decide
    protected override void Update()
    {
        if (isServer)
        {
            DetermineCurrentModeBasedOnStatus();
            switch (mode)
            {
                case Mode.Homeless:
                    TrackTimeBetweenScans();
                    UpdateNavData();
                    ExecuteIdleNavigationBehavior();
                    return;

                case Mode.SeekScrapWhileAtHome:
                    TrackTimeBetweenScans();
                    currentDest = homeAsteroid.transform.position;
                    UpdateNavData();
                    BuildTurretIfPossible();
                    return;

                case Mode.ReturnHomeWithScrap:
                    UpdateNavData();
                    currentDest = homeAsteroid.transform.position;
                    UnloadScrapIfAtHome();
                    return;

                case Mode.CollectScrap:
                    UpdateNavData();
                    currentDest = scrapTarget.transform.position;
                    return;
            }

        }
    }

    private void DetermineCurrentModeBasedOnStatus()
    {
        if (!homeAsteroid) // || homeAsteroid?.GetComponentInChildren<Turret_AI>())
        {
            previousHomeAsteroid = homeAsteroid;
            homeAsteroid = null;
            mode = Mode.Homeless;
            return;
        }
        if (!isCarryingScrap && !scrapTarget)
        {
            mode = Mode.SeekScrapWhileAtHome;
            return;
        }
        if (!isCarryingScrap && scrapTarget)
        {
            mode = Mode.CollectScrap;
            return;
        }
        if (isCarryingScrap)
        {
            mode = Mode.ReturnHomeWithScrap;
            return;
        }
    }

    protected override void Scan()
    {
        switch(mode)
        {
            case Mode.Homeless:
                homeAsteroid = CUR.GetNearestGameObjectWithTag(transform, "Asteroid", detectorRange);
                return;

            case Mode.SeekScrapWhileAtHome:
                scrapTarget = CUR.GetNearestGameObjectWithTag(transform, "Scrap", detectorRange);
                return;

            default:
                //Don't scan for anything.
                return;

        }
    }

    #endregion

    #region Act

    protected override void FixedUpdate()
    {
        TurnToFaceDestination(faceMode);
        MoveTowardsNavTarget(stoppingDist);
    }

    private void BuildTurretIfPossible()
    {
        if (distToDest > closeEnough) { return; }
        timeSpentBuildingAtHome += Time.deltaTime;
        if (timeSpentBuildingAtHome > timeRequiredToBuildTurret)
        {
            GameObject newTurret = Instantiate(weaponPrefab, homeAsteroid.transform.position, homeAsteroid.transform.rotation) as GameObject;
            NetworkServer.Spawn(newTurret);
            newTurret.GetComponent<AsteroidTurretBase>().AssignTransformToMatch(homeAsteroid.transform);
            homeAsteroid.tag = "ScrapperTurret";
            homeAsteroid = null;
            timeSpentBuildingAtHome = 0;
        }
    }

    private void UnloadScrapIfAtHome()
    {
        if (distToDest < closeEnough)
        {
            isCarryingScrap = false;
            timeSpentBuildingAtHome += timeBonusPerScrap;
        }
    }
    #endregion


    private void PickUpScrap()
    {
        isCarryingScrap = true;
    }



}
