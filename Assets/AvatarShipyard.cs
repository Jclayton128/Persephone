using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AvatarShipyard : MonoBehaviour
{
    //init
    [SerializeField] GameObject[] avatarMenu = null;
    PersNetworkManager pnm;
    void Start()
    {
        //pnm = FindObjectOfType<PersNetworkManager>();

        //foreach (GameObject go in avatarMenu)
        //{
        //    if (go.GetComponent<NetworkIdentity>())
        //    {
        //        pnm.spawnPrefabs.Add(go);
        //    }
        //}        
    }

    public GameObject ReturnPrefabAtIndex(int index)
    {
        return avatarMenu[index];
    }



}
