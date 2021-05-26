using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaBounds : MonoBehaviour
{
    [SerializeField] GameObject[] bounds;

    float minX = 0;
    float minY = 0;
    float maxX = 0 ;
    float maxY = 0;

    private void Start()
    {
        foreach (GameObject boundary in bounds)
        {
            Transform trans = boundary.transform;
            if (trans.position.x < minX)
            {
                minX = trans.position.x;
            }
            if (trans.position.x > maxX)
            {
                maxX = trans.position.x;
            }
            if (trans.position.y < minY)
            {
                minY = trans.position.y;
            }
            if (trans.position.y > maxY)
            {
                maxY = trans.position.y;
            }
        }
    }

    public bool CheckIfPointIsWithinArena(Vector2 testPos)
    {
        if (testPos.x < minX || testPos.x > maxX || testPos.y < minY || testPos.y > maxY)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
