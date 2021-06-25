using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaBounds : MonoBehaviour
{
    CircleEdgeCollider2D cec;
    public float ArenaRadius { get; private set; } = 30f;

    private void Start()
    {
        cec = FindObjectOfType<CircleEdgeCollider2D>();
        ArenaRadius = cec.Radius;
    }


    public bool CheckIfPointIsWithinArena(Vector2 testPos)
    {
        float distFromZeroPoint = (testPos - Vector2.zero).magnitude;

        if (distFromZeroPoint >= ArenaRadius)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public Vector2 CreateValidRandomPointWithinArena()
    {
        float randDist = UnityEngine.Random.Range(0, ArenaRadius * 0.9f);
        Vector2 randPos = Random.insideUnitCircle * ArenaRadius * 0.9f;
        return randPos;
    }

    public Vector2 CreateValidRandomPointOutsideOfArena()
    {
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);
        Vector2 randPos = new Vector2(Mathf.Sin(randomAngle), Mathf.Cos(randomAngle)).normalized;
        return randPos * (ArenaRadius * 1.5f);
    }

}
