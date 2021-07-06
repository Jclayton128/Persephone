using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Mine_AI : NetworkBehaviour
{
    [SerializeField] GameObject shrapnelPrefab = null;
    [SerializeField] AudioClip detonationSound = null;

    //param
    [SerializeField] float shrapnelDamage = 1.0f;
    [SerializeField] float scanRange = 1.5f;
    [SerializeField] int shrapnelCount = 16;
    [SerializeField] float shrapnelLifetime = 0.25f;
    [SerializeField] float shrapnelSpeed = 10.0f;
    [SerializeField] int primaryTargetLayer;
    [SerializeField] int secondaryTargetLayer;

    float shrapnelSpread = 360f;
    float timeBetweenProximityScans = 0.2f;

    //hood
    public static bool HasRegisteredPrefab = false;
    bool isDetonating = false;
    int targetLayerMask;
    float timeForNextProximityScan;

    private void Awake()
    {
        if (!HasRegisteredPrefab)
        {
            NetworkClient.RegisterPrefab(shrapnelPrefab);
            HasRegisteredPrefab = true;
        }

    }
    public override void OnStartServer()
    {
        targetLayerMask = (1 << primaryTargetLayer) | (1 << secondaryTargetLayer);
        timeForNextProximityScan = Time.time;

    }

    private void Update()
    {
        if (isServer && Time.time >= timeForNextProximityScan)
        {
            CheckIfDetonationRequired();
            timeForNextProximityScan = Time.time + timeBetweenProximityScans;
        }
    }

    private void CheckIfDetonationRequired()
    {
        RaycastHit2D possibleTarget = Physics2D.CircleCast(transform.position, scanRange, transform.up, 0.0f, targetLayerMask);
        if (possibleTarget && !isDetonating)
        {
            Detonate();
        }
    }

    private void Detonate()
    {
        isDetonating = true;
        float circleSubdivided = shrapnelSpread / shrapnelCount;
        for (int i = 1; i <= shrapnelCount; i++)
        {
            Quaternion sector = Quaternion.Euler(0, 0, i * circleSubdivided + transform.eulerAngles.z + (shrapnelSpread / 2) + 180);
            GameObject newShrapnel = Instantiate(shrapnelPrefab, transform.position, sector) as GameObject;
            newShrapnel.layer = 20;  //20 is neutral weapon - mines hurt everything on detonation
            newShrapnel.GetComponent<Rigidbody2D>().velocity = newShrapnel.transform.up * shrapnelSpeed;
            newShrapnel.GetComponent<DamageDealer>().SetNormalDamage(shrapnelDamage);
            NetworkServer.Spawn(newShrapnel);
            Destroy(newShrapnel, shrapnelLifetime);
        }
        RpcPlayAudioForDetonation();
        Destroy(gameObject);
    }

    [ClientRpc]
    private void RpcPlayAudioForDetonation()
    {
        Debug.Log("mine goes boom");
        //AudioSource.PlayClipAtPoint(detonationSound, transform.position);
    }


    public void DyingActions()
    {
        if (isServer)
        {
            Detonate();
        }

    }




}
