using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
[RequireComponent (typeof(IFF))]
public class Detector : MonoBehaviour
{
    [SerializeField] CircleCollider2D detColl = null;
    [SerializeField] bool ignoreDamageDealers;
    Brain brain;
    int ownIFF;
    int enemyIFF;
    private void Start()
    {
        brain = GetComponent<Brain>();
        ownIFF = GetComponent<IFF>().GetIFFAllegiance();
    }

    public void SetDetectorRange(float radius)
    {
        detColl.radius = radius;
        HiderSpotCheck(radius);
    }

    [Server]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        IFF collIFF;
        if (collision.transform.root.TryGetComponent<IFF>(out collIFF))
        {
            if (collIFF.GetIFFAllegiance() == ownIFF) { return; }
            if (collIFF.GetCurrentImportance() <= 0) { return; }
            if (collIFF.GetIFFAllegiance() != ownIFF)
            {
                brain.CheckAddTargetToList(collIFF);
                collIFF.OnModifyImportance += brain.ResortList;
            }
        }
        if (!ignoreDamageDealers && collision.gameObject.GetComponent<DamageDealer>() )
        {
            brain.WarnOfIncomingDamageDealer(collision.gameObject);
        }

    }

    [Server]
    private void OnTriggerExit2D(Collider2D collision)
    {
        IFF collIFF;
        if (collision.transform.root.TryGetComponent<IFF>(out collIFF))
        {
            brain.RemoveTargetFromList(collIFF);
            collIFF.OnModifyImportance -= brain.ResortList;
        }
    }

    public void HiderSpotCheck(float detectionRadius)
    {
        Collider2D[] colls = Physics2D.OverlapCircleAll(transform.position, detectionRadius, 1 << 17);
        foreach (Collider2D coll in colls)
        {
            IFF collIFF;
            if (coll.transform.root.TryGetComponent<IFF>(out collIFF))
            {
                if (collIFF.GetIFFAllegiance() == ownIFF) { return; }
                if (collIFF.GetCurrentImportance() <= 0) { return; }
                if (collIFF.GetIFFAllegiance() != ownIFF)
                {
                    brain.CheckAddTargetToList(collIFF);
                    collIFF.OnModifyImportance += brain.ResortList;
                }
            }
            if (!ignoreDamageDealers && coll.gameObject.GetComponent<DamageDealer>())
            {
                brain.WarnOfIncomingDamageDealer(coll.gameObject);
            }
        }
    }


}
