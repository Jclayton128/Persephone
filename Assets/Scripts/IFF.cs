using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class IFF : NetworkBehaviour , IComparer<IFF>
{
    //init
    [SerializeField] CircleCollider2D hiderCollider = null;
    Health health;

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

    public bool IsPersephone { get; private set; } = false;

    private void Start()
    {
        if (isServer)
        {
            currentImportance = normalImportance;
            health = GetComponent<Health>();
            if (gameObject.tag == "Persephone")
            {
                IsPersephone = true;
            }
           
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
        OnModifyImportance?.Invoke();

    }

    #region Comparisons for Sorting
    public static int CompareByImportance(IFF iff1, IFF iff2)
    {
        return iff1.currentImportance.CompareTo(iff2.currentImportance);
    }

    public static int CompareByHealthLevel(IFF iff1, IFF iff2)
    {
        if (iff1 == null || iff2 == null)
        {
            return 0;
        }
        Health health1 = iff1.GetComponent<Health>();
        Health health2;
        if (iff2.TryGetComponent<Health>(out health2) == false ||  health1?.GetHealthFactor() > health2.GetHealthFactor())
        {
            return -1;
        }
        if (health1 == null || health1?.GetHealthFactor() < health2.GetHealthFactor())
        {
            return 1;
        }   
        else
        {
            return 0;
        }

    }

    public static int CompareByIonization(IFF iff1, IFF iff2)
    {
        if (iff1 == null || iff2 == null)
        {
            return 0;

        }
        Health health1;
        Health health2;
        if (iff1.TryGetComponent<Health>(out health1) == false || iff2.TryGetComponent<Health>(out health2) == false)
        {
            return 0;
        }
        if (health1.GetCurrentIonization() > health2.GetCurrentIonization())
        {
            return 1;
        }
        else
        {
            return -1;
        }

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

    #endregion

    public void UpdateHiderRadius(int v1, int v2)
    {
        hiderCollider.radius = currentImportance;
    }
}
