using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Mirror;
using UnityEngine.SceneManagement;

public class DebugHelper : NetworkBehaviour
{
    bool isInDebugMode = false;
    [SerializeField] TextMeshProUGUI debugModeTMP = null;
    [SerializeField] GameObject[] testMinion = null;
    PersephoneBrain pb;
    LevelManager lm;

    public override void OnStartClient()
    {
        base.OnStartClient();
        debugModeTMP = GameObject.FindGameObjectWithTag("DebugText").GetComponent<TextMeshProUGUI>();

            

    }

    // Update is called once per frame
    void Update()
    {
        lm = FindObjectOfType<LevelManager>();
        if (isClient)
        {
            HandleDebugModeToggle();
            HandleDebugMinionSpawn();
            HandlePlayerDisableUndisable();
            HandlePlayerEnergyReset();
            HandlePersephoneHalt();
            HandleExperienceGain();
        }
    }

    private void HandleExperienceGain()
    {
        if (hasAuthority && isInDebugMode && Input.GetKeyDown(KeyCode.L)) 
        {
            UpgradeManager um = ClientInstance.ReturnClientInstance().CurrentAvatar.GetComponent<UpgradeManager>();
            um.GainScrap(100);
        }
    }

    private void HandlePersephoneHalt()
    {
        if (!pb && lm.GetCurrentLevelCount() >=1 )
        {
            pb = GameObject.FindGameObjectWithTag("Persephone").GetComponent<PersephoneBrain>();
        }
        if (isInDebugMode && Input.GetKeyDown(KeyCode.M) && hasAuthority && pb)
        {
            CmdTogglePersephoneHalt();
        }
    }

    [Command]
    private void CmdTogglePersephoneHalt()
    {
        if (!pb)
        {
            pb = GameObject.FindGameObjectWithTag("Persephone").GetComponent<PersephoneBrain>();
        }
        pb.DebugToggleMovementOnOff();
    }

    private void HandlePlayerEnergyReset()
    {
        if (isInDebugMode && Input.GetKeyDown(KeyCode.P) && hasAuthority)
        {
            EnergySource es = GetComponent<ClientInstance>().CurrentAvatar.GetComponent<EnergySource>();
            es.ResetPowerLevel();
        }
    }

    private void HandlePlayerDisableUndisable()
    {
        if (isInDebugMode && Input.GetKeyDown(KeyCode.H) && hasAuthority)
        {
            Health health = GetComponent<ClientInstance>().CurrentAvatar.GetComponent<Health>();
            if (health.GetCurrentHull() > 0)
            {
                health.CmdModifyHullLevelViaClientDebug(-1 * health.GetMaxHull(), false);
                return;
            }
            if (health.GetCurrentHull() <= 0)
            {
                health.CmdModifyHullLevelViaClientDebug(health.GetMaxHull()*10f, true);
                return;
            }

        }
    }

    private void HandleDebugMinionSpawn()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && isInDebugMode && isLocalPlayer)
        {
            CmdSpawnMinionForDebug(0);
        }
    }


    private void HandleDebugModeToggle()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            isInDebugMode = !isInDebugMode;
            if (isInDebugMode)
            {
                debugModeTMP.text = "DEBUG, H = heal toggle, P = reset energy, M = Toggle Pers Movement, L = Level Up";
            }
            else
            {
                debugModeTMP.text = " ";
            }
        }
    }


    [Command]
    private void CmdSpawnMinionForDebug(int index)
    {
        GameObject minion = Instantiate(testMinion[index], Vector3.zero, Quaternion.identity) as GameObject;
        NetworkServer.Spawn(minion);
    }


}
