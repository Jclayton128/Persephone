using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Ability_RockMode : Ability
{
    [SerializeField] GameObject[] partsToGoPositive90 = null;
    [SerializeField] GameObject[] partsToGoNegative90 = null;

    [SyncVar (hook = nameof(UpdateClientWithDeployFactor))]
    float deployFactor = 1;
    float deployRate;

    bool shouldDeploy = true;

    IFF iff;

    public override void OnStartServer()
    {
        base.OnStartServer();
        iff = GetComponent<IFF>();
        deployRate = 1f / timeBetweenShots;
    }

    protected override void MouseClickDownEffect()
    {
        CmdToggleDeployStatus();
    }

    protected override void MouseClickUpEffect()
    {
        
    }

    private void CmdToggleDeployStatus()
    {
        if (deployFactor == 0 || deployFactor == 1)
        {
            if (!es.CheckSpendEnergy(costToActivate)) { return; }
            shouldDeploy = !shouldDeploy;
            if (shouldDeploy)
            {
                am.ToggleStatusIcon(this, false);
            }
            if (!shouldDeploy)
            {
                am.ToggleStatusIcon(this, true);
            }
        }
    }

    void Update()
    {
        if (isClient)
        {
            
        }
        if (isServer)
        {
            HandleDeployFactor();
            HandleIFFChanges();
        }
    }

    private void HandleIFFChanges()
    {
       if (deployFactor == 0)
        {
            iff.SetEnabledDisabledImportance(true);
        }
       if (deployFactor == 1)
        {
            iff.SetEnabledDisabledImportance(false);
        }
       
    }

    private void HandleDeployFactor()
    {
        if (shouldDeploy && deployFactor < 1)
        {
            Deploy();
        }
        if (!shouldDeploy && deployFactor > 0)
        {
            Undeploy();
        }
    }

    private void Deploy()
    {
        deployFactor += Time.deltaTime * deployRate;
        deployFactor = Mathf.Clamp01(deployFactor);
    }

    private void Undeploy()
    {
        deployFactor -= Time.deltaTime * deployRate;
        deployFactor = Mathf.Clamp01(deployFactor);
    }

    private void UpdateClientWithDeployFactor(float oldValue, float newValue)
    {
        if (oldValue == 0 && newValue > 0)
        {
            //TODO audio clip of deploying
        }
        if (oldValue == 1 && newValue < 1)
        {
            //TODO audio clip of undeploying, maybe same sound
        }
        foreach (GameObject part in partsToGoNegative90)
        {
            part.transform.localRotation = Quaternion.Euler(0, 0, (1-deployFactor) * -90f);
        }
        foreach (GameObject part in partsToGoPositive90)
        {
            part.transform.localRotation = Quaternion.Euler(0, 0, (1 - deployFactor) * 90f);
        }
    }

    public bool CheckFullyDeployed()
    {
        if (deployFactor == 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
