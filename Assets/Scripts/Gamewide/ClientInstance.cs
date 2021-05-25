using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class ClientInstance : NetworkBehaviour
{
    [SerializeField] AvatarShipyard avatarShipyard;
    public static ClientInstance Instance;
    Camera cam;
    [SerializeField] GameObject desiredAvatar;
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
        avatarShipyard = FindObjectOfType<AvatarShipyard>();
        if (!isLocalPlayer)
        {
            cam.enabled = false;
        }

        if (isLocalPlayer)
        {
            HookIntoLocalShipSelectPanel();

        }


        //FindObjectOfType<UIManager>().SetLocalPlayerForUI(this);
    }

    private void HookIntoLocalShipSelectPanel()
    {
        FindObjectOfType<ShipSelectPanelDriver>().ci = this;
    }
    public void SetDesiredAvatar(int indexForShipyard)
    {

        desiredAvatar = avatarShipyard.ReturnPrefabAtIndex(indexForShipyard); //doesn't update on the server here.
        CmdRequestSpawnDesiredAvatar(indexForShipyard);

    }


    #endregion


    #region Server

    public override void OnStartServer()
    {
        base.OnStartServer();
        GameObject.DontDestroyOnLoad(gameObject);
        avatarShipyard = FindObjectOfType<AvatarShipyard>();
    }

    [Command]
    private void CmdRequestSpawnDesiredAvatar(int index)
    {
        desiredAvatar = avatarShipyard.ReturnPrefabAtIndex(index);
        NetworkSpawnAvatar();

    }

    [Server]
    private void NetworkSpawnAvatar()
    {
        //Vector3 randomPos = MapHelper.CreateRandomValidStartPoint(); For random start position
        Debug.Log($"trying to spawn {desiredAvatar} on server");
        GameObject test = FindObjectOfType<PersNetworkManager>().spawnPrefabs[0];
        GameObject go = Instantiate(test, transform.position, Quaternion.identity);
        Debug.Log($"break 2");
        go.GetComponent<IFF>().SetIFFAllegiance(IFF.PlayerIFF);
        Debug.Log($"break 3");
        NetworkServer.Spawn(go, base.connectionToClient);
        Debug.Log($"break 4");
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
