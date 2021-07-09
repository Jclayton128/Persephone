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
    PlayerShipyard ps;

    public static Action<GameObject> OnAvatarSpawned; //Anytime an observer to this event hears it, they get passed a reference Game Object


    private void Awake()
    {
        if (isClient)
        {
            FindObjectOfType<PlayerShipyard>().RegisterAvatarPrefabs();
        }
    }

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
        ps = FindObjectOfType<PlayerShipyard>();
        if (isLocalPlayer)
        {
            GameObject.DontDestroyOnLoad(gameObject);
            Instance = this;
            cam = Camera.main;
            cam.enabled = true;

            GameObject panel = Instantiate(shipSelectPanel, Vector2.zero, Quaternion.identity) as GameObject;
            sspd = panel.GetComponent<ShipSelectPanelDriver>();


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

    public void SetPlayerName(string newName)
    {
        PlayerName = newName;
    }

    public void LaunchGame()
    {
        CmdNetworkSpawnAvatarAndStartGame(sspd.chosenAvatarIndex);
    }

    #endregion


    #region Server

    


    [Command]
    public void CmdNetworkSpawnAvatarAndStartGame(int indexOfChoiceFromShipyard)
    {
        if (!CurrentAvatar)
        {
            GameObject prefab = ps.allAvatarPrefabs[indexOfChoiceFromShipyard];
            Vector2 startPoint = FindObjectOfType<ArenaBounds>().CreateRandomPointWithinArena(Vector2.zero, 3.0f, ArenaBounds.DestinationMode.noFartherThan);
            GameObject go = Instantiate(prefab, startPoint, Quaternion.identity);
            go.GetComponent<IFF>().SetIFFAllegiance(IFF.PlayerIFF);
            NetworkServer.Spawn(go, base.connectionToClient);

            RequestStartingLevelIfFirstPlayer();
        }
        else
        {
            Debug.Log("player alread has a current avatar");
        }

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
