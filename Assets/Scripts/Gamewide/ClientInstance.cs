﻿using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class ClientInstance : NetworkBehaviour
{
    [SerializeField] GameObject shipPrefab = null;
    public static ClientInstance Instance;
    Camera cam;
    public GameObject currentAvatar;
    Scene scene;

    public static Action<GameObject> OnAvatarSpawned; //Anytime an observer to this event hears it, they get passed a reference Game Object

    #region EventResponse

    public void InvokeAvatarSpawned(GameObject go)
    //This fires or dispatches the OnAvatarSpawned event, along with the GameObject reference of the thing that just spawned
    {
        OnAvatarSpawned?.Invoke(go);
        currentAvatar = go;

    }


    #endregion

    #region Client
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        GameObject.DontDestroyOnLoad(gameObject);
        scene = SceneManager.GetSceneByBuildIndex(1);
        Instance = this;
        cam = Camera.main;
        if (!isLocalPlayer)
        {
            cam.enabled = false;
        }

        if (shipPrefab && isLocalPlayer)
        {
            CmdRequestSpawn();
        }

        //FindObjectOfType<UIManager>().SetLocalPlayerForUI(this);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        GameObject.DontDestroyOnLoad(gameObject);
    }

    public void SetupAvatarRespawn()
    {
        Destroy(currentAvatar);
        CmdRequestSpawn();    
    }

    public void SetChosenAvatarPrefab(GameObject go)
    {
        shipPrefab = go;
        Debug.Log("just set the prefab as " + go);
        CmdRequestSpawn();
        //De
    }

    [Command]
    public void CmdRequestSpawn()
    {
        NetworkSpawnAvatar();
    }
    #endregion


    #region Server
    [Server]
    private void NetworkSpawnAvatar()
    {
        //Vector3 randomPos = MapHelper.CreateRandomValidStartPoint(); For random start position
        GameObject go = Instantiate(shipPrefab, transform.position, Quaternion.identity);
        go.GetComponent<IFF>().SetIFFAllegiance(IFF.PlayerIFF);
        NetworkServer.Spawn(go, base.connectionToClient);
    }


    #endregion

    public static ClientInstance ReturnClientInstance(NetworkConnection conn = null)
    {
        if (NetworkServer.active && conn != null)
        {
            NetworkIdentity localPlayer;
            if (PersNetworkManager.LocalPlayers.TryGetValue(conn, out localPlayer))
                return localPlayer.GetComponent<ClientInstance>();
            else
                return null;
        }
        else
        {
            return Instance;
        }
    }
}
