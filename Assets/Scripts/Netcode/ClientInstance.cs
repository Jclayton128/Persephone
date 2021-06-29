using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ClientInstance : NetworkBehaviour
{
    public string PlayerName { get; private set; }
    [SerializeField] GameObject shipSelectPanel = null;
    public static ClientInstance Instance;
    [SerializeField] Camera cam;
    //[SerializeField] GameObject desiredAvatar;
    ShipSelectPanelDriver sspd;
    int desiredAvatar;
    public GameObject CurrentAvatar;
    LevelManager lm;

    public static Action<GameObject> OnAvatarSpawned; //Anytime an observer to this event hears it, they get passed a reference Game Object




    #region EventResponse

    public void InvokeAvatarSpawned(GameObject go)
    //This fires or dispatches the OnAvatarSpawned event, along with the GameObject reference of the thing that just spawned
    {
        OnAvatarSpawned?.Invoke(go);
        CurrentAvatar = go;

    }


    #endregion

    #region Client

    private void Start()
    {

        GameObject panel = Instantiate(shipSelectPanel, Vector2.zero, Quaternion.identity) as GameObject;
        sspd = panel.GetComponent<ShipSelectPanelDriver>();
        if (isLocalPlayer)
        {
            GameObject.DontDestroyOnLoad(gameObject);
            Instance = this;
            cam = Camera.main;
            cam.enabled = true;

            HookIntoLocalShipSelectPanel();
            FindObjectOfType<UIManager>().SetLocalPlayerForUI(this);

        }
        if (isServer)
        {
            lm = FindObjectOfType<LevelManager>();
        }


    }


    private void HookIntoLocalShipSelectPanel()
    {
        sspd.ci = this;
        sspd.DisplayPanel();        
    }
    public void SetDesiredAvatar(int indexForShipyard)
    {
        //desiredAvatar = avatarShipyard.ReturnPrefabAtIndex(indexForShipyard); //doesn't update on the server here.
        CmdRequestSpawnDesiredAvatar(indexForShipyard);
    }

    public void SetPlayerName(string newName)
    {
        PlayerName = newName;
    }




    #endregion


    #region Server

    [Command]
    private void CmdRequestSpawnDesiredAvatar(int index)
    {
        //desiredAvatar = avatarShipyard.ReturnPrefabAtIndex(index);
        NetworkSpawnAvatar(index);

    }

    [Server]
    private void NetworkSpawnAvatar(int index)
    {
        //Vector3 randomPos = MapHelper.CreateRandomValidStartPoint(); For random start position
        GameObject test = FindObjectOfType<PersNetworkManager>().spawnPrefabs[index];
        GameObject go = Instantiate(test, transform.position, Quaternion.identity);
        go.GetComponent<IFF>().SetIFFAllegiance(IFF.PlayerIFF);
        NetworkServer.Spawn(go, base.connectionToClient);

        RequestStartingLevelIfFirstPlayer();
    }

    private void RequestStartingLevelIfFirstPlayer()
    {
        if (lm.GetCurrentLevelCount() == 0)
        {
            lm.AdvanceToNextLevel();
        }
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
