using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ShipSelectPanelDriver : NetworkBehaviour
{
    // Start is called before the first frame update
    public ClientInstance ci;
    Vector3 displayPos = Vector3.zero;
    Vector3 hidePos = new Vector3(999, 999, 0);
    public override void OnStartClient()
    {
        base.OnStartClient();
        //Populate button images with sprites from the AvatarShipyard
    }
    public void PushSelectPrefabToClient(int index)
    {
        ci.SetDesiredAvatar(index);
        HidePanel();
        //gameObject.SetActive(false);
    }

    public void DisplayPanel()
    {
        transform.position = displayPos;
    }

    public void HidePanel()
    {
        transform.position = hidePos;
    }


}
