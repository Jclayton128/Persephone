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
        Scene currentScene = SceneManager.GetActiveScene();
        Scene arena = SceneManager.GetSceneByBuildIndex(1);
        if (currentScene == arena)
        {
            debugModeTMP = GameObject.FindGameObjectWithTag("DebugText").GetComponent<TextMeshProUGUI>();
        }
        else
        {
            SceneManager.activeSceneChanged += ReloadUI;
        }
    }

    private void ReloadUI(Scene arg0, Scene arg1)
    {
        debugModeTMP = GameObject.FindGameObjectWithTag("DebugText").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleDebugModeToggle();
        HandleDebugMinionSpawn();
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
                debugModeTMP.text = "DEBUG";
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
