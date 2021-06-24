using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IFF collIFF;
        if (collision.gameObject.TryGetComponent<IFF>(out collIFF))
        {
            if (collIFF.GetIFFAllegiance() == ownIFF) { return; }
            if (collIFF.GetCurrentImportance() <= 0) { return; }
            if (collIFF.GetIFFAllegiance() != ownIFF)
            {
                brain.AddTargetToList(collIFF);
                collIFF.OnModifyImportance += brain.ResortListBasedOnImportance;
            }
        }
        if (collision.gameObject.GetComponent<DamageDealer>())
        {
            brain.WarnOfIncomingDamageDealer(collision.gameObject);
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IFF collIFF;
        if (collision.gameObject.TryGetComponent<IFF>(out collIFF))
        {
            brain.RemoveTargetFromList(collIFF);
            collIFF.OnModifyImportance -= brain.ResortListBasedOnImportance;
        }


    }


}
