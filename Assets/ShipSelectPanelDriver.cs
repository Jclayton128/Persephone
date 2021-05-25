using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ShipSelectPanelDriver : NetworkBehaviour
{
    // Start is called before the first frame update
    public ClientInstance ci;
    public override void OnStartClient()
    {
        base.OnStartClient();
        FindLocalClientInstance();

    }

    private void FindLocalClientInstance()
    {
        ClientInstance[] allCI = FindObjectsOfType<ClientInstance>();
        Debug.Log($"Found {allCI.Length} CIs");
        foreach (ClientInstance possCI in allCI)
        {
            if (possCI.isLocalPlayer)
            {
                ci = possCI;
            }
        }
    }

    public void PushSelectPrefabToClient(GameObject go)
    {
        if (!ci)
        {
            FindLocalClientInstance();
        }

        ci.SetChosenAvatarPrefab(go);
        Debug.Log($"CI is {ci.connectionToServer} and wants to be {go}");
        gameObject.SetActive(false);
    }

}
