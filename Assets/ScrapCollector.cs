using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Mirror;

public class ScrapCollector : NetworkBehaviour
{
    //init
    [SerializeField] AudioClip scrapPickupSound = null;
    //[SerializeField] TextMeshProUGUI scrapDisplayer = null;

    //param
    [SyncVar]
    int scrapCollected = 0;

    [SerializeField] float catchDistance;
    [SerializeField] CircleCollider2D scrapVacuum = null;
    [SerializeField] float scrapVacuumSize;

    private void Start()
    {
        scrapVacuum.radius = scrapVacuumSize;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isServer) { return; }
        if (collision.transform.gameObject.GetComponent<ProtoScrap>())
        {
            float dist = (collision.transform.position - transform.position).magnitude;
            if (dist < catchDistance)
            {
                scrapCollected++;
                //TODO play picked up scrap audioclip
                Destroy(collision.transform.gameObject);
            }
        }
    }

}
