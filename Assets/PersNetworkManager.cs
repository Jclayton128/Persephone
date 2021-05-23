using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PersNetworkManager : NetworkManager
{
    public static Dictionary<NetworkConnection, NetworkIdentity> LocalPlayers = new Dictionary<NetworkConnection, NetworkIdentity>();

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        //base.OnServerAddPlayer();
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        LocalPlayers[conn] = player.GetComponent<NetworkIdentity>();
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        LocalPlayers.Remove(conn);
    }
}
