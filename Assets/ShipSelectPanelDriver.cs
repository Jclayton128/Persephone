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
        //Populate button images with sprites from the AvatarShipyard


    }
    public void PushSelectPrefabToClient(int index)
    {
        ci.SetDesiredAvatar(index);
        gameObject.SetActive(false);
        Debug.Log($"Pushed {index} to {ci} ");
    }


}
