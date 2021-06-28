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

    public enum DestinationMode { noCloserThan, noFartherThan};

    public Vector2 CreateRandomPointWithinArena(Vector2 origin, float distanceFromOrigin, DestinationMode mode)
    {
        Vector2 randPos;

        if (mode == DestinationMode.noCloserThan)
        {
            do
            {
                float randDist = UnityEngine.Random.Range(0, ArenaRadius * 0.9f);
                randPos = Random.insideUnitCircle * ArenaRadius * 0.9f;
            }
            while ((randPos - origin).magnitude < distanceFromOrigin);
            return randPos;
        }

        if (mode == DestinationMode.noFartherThan)
        {
            do
            {
                float randDist = UnityEngine.Random.Range(0, ArenaRadius * 0.9f);
                randPos = Random.insideUnitCircle * ArenaRadius * 0.9f;
            }
            while ((randPos - origin).magnitude > distanceFromOrigin);
            return randPos;
        }
        else
        {
            return Vector2.zero;
        }
    }

    public Vector2 CreateValidRandomPointOutsideOfArena()
    {
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);
        Vector2 randPos = new Vector2(Mathf.Sin(randomAngle), Mathf.Cos(randomAngle)).normalized;
        return randPos * (ArenaRadius * 3f);
    }

    public Vector2 CheckPoint_CreateReflection(Vector2 origin, Vector2 testPoint)  // NEEDS WORK!
    {
        if (CheckIfPointIsWithinArena(testPoint))
        {
            return testPoint;
        }
        else
        {
            Vector2 dir = (origin - testPoint) * 2f;
            Debug.Log($"reflecting {testPoint}. dir {dir}. new point: {testPoint -  dir}");
            return (testPoint - dir);
        }
    }

    public Vector2 CheckPoint_CreateMoreCenteredPoint(Vector2 testPoint)
    {
        if (CheckIfPointIsWithinArena(testPoint))
        {
            return testPoint;
        }
        else
        {
            Vector2 dir = (testPoint - Vector2.zero).normalized * (ArenaRadius / 5f);
            
            Debug.Log($"getting more centered point {testPoint}. dir {dir}. new point: {testPoint - dir}");
            return (testPoint - dir);
        }
    }

}
