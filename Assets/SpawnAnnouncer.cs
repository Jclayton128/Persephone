using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SpawnAnnouncer : NetworkBehaviour
{

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        AnnounceSpawned();
    }

    private void AnnounceSpawned()
    {
        ClientInstance ci = ClientInstance.ReturnClientInstance();
        ci.InvokeAvatarSpawned(gameObject);
    }

    private void OnDestroy()
    {

    }
}
