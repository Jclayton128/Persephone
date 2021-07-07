using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class ReanimatorPayload : NetworkBehaviour
{
    [SerializeField] GameObject[] coreClumps = null;
    CircleCollider2D coll;
    GameObject reanimatedThing;

    //param
    [SerializeField] public float lifetime = 20;
    int scrapsRequiredForBonusClump = 3;
    float radius = 10;
    float catchDistance = 2;

    //hood
    int scrapCollected = 0;
    float deathTime;

    private void Awake()
    {
        foreach (GameObject clump in coreClumps)
        {
            if (!NetworkClient.prefabs.ContainsValue(clump))
            {
                NetworkClient.RegisterPrefab(clump);
            }
        }
    }

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        base.OnStartServer();
        coll = GetComponent<CircleCollider2D>();
        coll.radius = radius;
        deathTime = Time.time + lifetime;

    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            if (Time.time >= deathTime)
            {
                ActivateNewThing();
                Destroy(gameObject);
            }
        }
    }

    private void ActivateNewThing()
    {
        if (reanimatedThing)
        {
            reanimatedThing.GetComponent<ReanimatedBrain>().ActivateReanimatedThing();

        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isServer)
        {
            if (collision.transform.gameObject.GetComponent<ProtoScrap>())
            {
                float dist = (collision.transform.position - transform.position).magnitude;
                if (dist < catchDistance)
                {
                    scrapCollected++;
                    CheckScrapLevelForClump();
                    Destroy(collision.gameObject);
                }
            }
        }
    }

    private void CheckScrapLevelForClump()
    {
        if (scrapCollected % scrapsRequiredForBonusClump == 0)
        {
            GenerateClump();
        }
    }

    private void GenerateClump()
    {
        if (!reanimatedThing)
        {
            int rand = UnityEngine.Random.Range(0, coreClumps.Length);
            Quaternion randQuat = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-180, 180));
            reanimatedThing = Instantiate(coreClumps[rand], transform.position, randQuat) as GameObject;
            NetworkServer.Spawn(reanimatedThing);
        }   
        if (reanimatedThing)
        {
            reanimatedThing.GetComponent<ReanimatedBrain>().AddNewClump();
        }
    }
}
