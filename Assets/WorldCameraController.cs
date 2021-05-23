
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class WorldCameraController : MonoBehaviour
{
    private void Awake()
    {
        ClientInstance.OnAvatarSpawned += ClientInstance_OnAvatarSpawned;
        //Subscribe to "On Avatar Spawned", thereby firing the "CliInst_OnAvaSpa" script with a GameObject reference attached to the event.
    }

    private void OnDestroy()
    {
        ClientInstance.OnAvatarSpawned -= ClientInstance_OnAvatarSpawned;
    }

    private void ClientInstance_OnAvatarSpawned(GameObject go)
    {
        GetComponentInChildren<CinemachineVirtualCamera>().Follow = go.transform;
    }
}
