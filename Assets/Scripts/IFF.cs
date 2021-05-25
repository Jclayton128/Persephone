using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class IFF : NetworkBehaviour
{
    //init

    //param
    int iffAllegiance;
    public static readonly int PlayerIFF = 0;

    public Action<int> OnChangeIFF;


    public void SetIFFAllegiance(int value)
    {
        iffAllegiance = value;
    }

    public int GetIFFAllegiance()
    {
        return iffAllegiance;
    }

}
