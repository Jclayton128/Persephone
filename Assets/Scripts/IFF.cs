using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class IFF : NetworkBehaviour , IComparer<IFF>
{
    //init
    [SerializeField] CircleCollider2D hiderCollider = null;

    //param
    [SyncVar]
    [SerializeField] int iffAllegiance;
    [SerializeField] int normalImportance;
    public static readonly int PlayerIFF = 0;
    int disabledImportance = 0;

    //hood
    int currentImportance;


    public Action<int> OnChangeIFF;
    public Action OnModifyImportance;

    private void Start()
    {
        currentImportance = normalImportance;
        hiderCollider.radius = currentImportance;
    }

    public void SetIFFAllegiance(int value)
    {
        iffAllegiance = value;
    }

    public int GetIFFAllegiance()
    {
        return iffAllegiance;
    }
    public int GetCurrentImportance()
    {
        return currentImportance;
    }

    public void OverrideCurrentImportance(int newImportance)
    {
        currentImportance = newImportance;
        OnModifyImportance?.Invoke();
        hiderCollider.radius = currentImportance;
    }

    [Server]
    public void SetEnabledDisabledImportance(bool isDisabled)
    {
        if (isDisabled)
        {
            currentImportance = disabledImportance;
        }
        if (!isDisabled)
        {
            currentImportance = normalImportance;
        }
        hiderCollider.radius = currentImportance;
    }

    public static int CompareByImportance(IFF iff1, IFF iff2)
    {
        return iff1.currentImportance.CompareTo(iff2.currentImportance);
    }


    public int Compare(IFF x, IFF y)
    {
        if (x == null || y == null)
        {
            //Debug.Log("other is null");
            return 0;
        }
        if (x.currentImportance > y.currentImportance)
        {
           //Debug.Log("other is more important");
            return -1;
        }
        if (x.currentImportance < y.currentImportance)
        {
            //Debug.Log("other is less important");
            return 1;
        }
        else
        {
            //Debug.Log("other is equally important");
            return 0;
        }
    }
}
