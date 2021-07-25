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
    UpgradeManager um;

    [SerializeField] float catchDistance;
    [SerializeField] CircleCollider2D scrapVacuum = null;
    [SerializeField] float scrapVacuumSize;

    public Action OnScrapPickup;
    private void Start()
    {
        scrapVacuum.radius = scrapVacuumSize;
        um = GetComponent<UpgradeManager>();
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.transform.gameObject.GetComponent<ProtoScrap>())
        {
            float dist = (collision.transform.position - transform.position).magnitude;
            if (dist < catchDistance)
            {
                //TODO play picked up scrap audioclip
                OnScrapPickup?.Invoke();
                um?.GainScrap(1);
                NetworkServer.UnSpawn(collision.gameObject);
                Destroy(collision.gameObject);
            }
        }
    }

    public void IncreaseScrapVacRange(float amount)
    {
        scrapVacuumSize += amount;
        scrapVacuum.radius = scrapVacuumSize;
    }

}
