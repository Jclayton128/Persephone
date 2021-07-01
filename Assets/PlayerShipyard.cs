using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerShipyard : NetworkBehaviour
{
    
    [SerializeField] public List<GameObject> allAvatarPrefabs = null;
    public List<GameObject> availableAvatarPrefabs = new List<GameObject>();

    void Start()
    {
        // Is a sort of the prefab list required to ensure sync between client and server versions?
        if (isClient)
        {
            foreach (GameObject avatar in allAvatarPrefabs)
            {
                if (!NetworkClient.prefabs.ContainsValue(avatar))
                { 
                    NetworkClient.RegisterPrefab(avatar);
                }

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
