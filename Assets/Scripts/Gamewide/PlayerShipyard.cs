using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerShipyard : MonoBehaviour
{
    
    [SerializeField] public List<GameObject> allAvatarPrefabs = null;
    public List<GameObject> availableAvatarPrefabs = new List<GameObject>();

    private void Awake()
    {
        RegisterAvatarPrefabs();
    }

    public void RegisterAvatarPrefabs()
    {
        foreach (GameObject avatar in allAvatarPrefabs)
        {
            NetworkClient.RegisterPrefab(avatar);
            if (!NetworkClient.prefabs.ContainsValue(avatar))
            {
                //Debug.Log($"Registered Avatar Prefab {avatar.name}");
                //NetworkClient.RegisterPrefab(avatar);
            }

        }
    }


    public bool CheckClaimStatus(int index)
    {
        //TODO implement a way to check on the status of a ship before spawning. 
        return true;
    }

    public void ClaimPrefab(GameObject claimedAvatar)
    {
        //TODO implement this
    }
}
