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
    public override void OnStartClient()
    {

            debugModeTMP = GameObject.FindGameObjectWithTag("DebugText").GetComponent<TextMeshProUGUI>();

    }

    // Update is called once per frame
    void Update()
    {
        HandleDebugModeToggle();
        HandleDebugMinionSpawn();
        HandlePlayerDisableUndisable();
        HandlePlayerEnergyReset();
    }

    private void HandlePlayerEnergyReset()
    {
        if (isInDebugMode && Input.GetKeyDown(KeyCode.P))
        {
            EnergySource es = GetComponent<ClientInstance>().currentAvatar.GetComponent<EnergySource>();
            es.ResetPowerLevel();
            Debug.Log("debug reset energy");
        }
    }

    private void HandlePlayerDisableUndisable()
    {
        if (isInDebugMode && Input.GetKeyDown(KeyCode.H))
        {
            Health health = GetComponent<ClientInstance>().currentAvatar.GetComponent<Health>();
            if (health.GetCurrentHull() > 0)
            {
                Debug.Log("debug disable");
                health.ModifyHullLevel(-1 * health.GetMaxHull(), false);
                return;
            }
            if (health.GetCurrentHull() <= 0)
            {
                health.ModifyHullLevel(health.GetMaxHull()*10f, true);
                Debug.Log("debug repair");
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
                debugModeTMP.text = "DEBUG, H = heal toggle, P = reset energy";
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
