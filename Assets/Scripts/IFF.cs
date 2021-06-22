using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class IFF : NetworkBehaviour , IComparer<IFF>
{
    //init

    //param
    [SyncVar]
    [SerializeField] int iffAllegiance;

    [SerializeField] public int importance;
    public static readonly int PlayerIFF = 0;

    public Action<int> OnChangeIFF;
    public Action OnModifyImportance;


    public void SetIFFAllegiance(int value)
    {
        iffAllegiance = value;
    }

    public int GetIFFAllegiance()
    {
        return iffAllegiance;
    }
    public int GetImportance()
    {
        return importance;
    }

    public void ModifyImportance(int newImportance)
    {
        importance = newImportance;
        OnModifyImportance?.Invoke();
    }

    public static int CompareByImportance(IFF iff1, IFF iff2)
    {
        return iff1.importance.CompareTo(iff2.importance);
    }

    //public int CompareTo(IFF other)
    //{
    //    if (other == null)
    //    {
    //        Debug.Log("other is null");
    //        return 1;
    //    }
    //    if (other.importance > this.importance)
    //    {
    //        Debug.Log("other is more important");
    //        return -1;
    //    }
    //    if (other.importance < this.importance)
    //    {
    //        Debug.Log("other is less important");
    //        return 1;
    //    }
    //    else
    //    {
    //        Debug.Log("other is equally important");
    //        return 0;
    //    }
    //}

    public int Compare(IFF x, IFF y)
    {
        if (x == null || y == null)
        {
            Debug.Log("other is null");
            return 0;
        }
        if (x.importance > y.importance)
        {
            Debug.Log("other is more important");
            return -1;
        }
        if (x.importance < y.importance)
        {
            Debug.Log("other is less important");
            return 1;
        }
        else
        {
            Debug.Log("other is equally important");
            return 0;
        }
    }
}
