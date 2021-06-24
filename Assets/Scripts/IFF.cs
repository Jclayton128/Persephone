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
    [SyncVar(hook = nameof(UpdateHiderRadius))]
    int currentImportance;


    public Action<int> OnChangeIFF;
    public Action OnModifyImportance;

    private void Start()
    {
        if (isServer)
        {
            currentImportance = normalImportance;
        }

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

    [Server]
    public void OverrideCurrentImportance(int newImportance)
    {
        Debug.Log($"is Server{isServer} calling override importance to {newImportance}");
        currentImportance = newImportance;
        OnModifyImportance?.Invoke();
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

    public void UpdateHiderRadius(int v1, int v2)
    {
        hiderCollider.radius = currentImportance;
    }
}
