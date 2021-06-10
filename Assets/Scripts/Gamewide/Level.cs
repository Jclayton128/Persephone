using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level")]
public class Level : ScriptableObject
{
    [SerializeField] List<GameObject> enemyPrefabs = new List<GameObject>();
    [SerializeField] List<Vector2> spawnPoints = new List<Vector2>();
    [SerializeField] Vector2 portalLocation = Vector2.up;
    [SerializeField] GameObject[] asteroids = null;
    [SerializeField] Vector3 playerEntryPoint = new Vector3(0, 0, 0);
    public float fogLevel = 0; // must be between 0 and 1;
    public bool isBossLevel = false;

    public GameObject ReturnRandomEnemyFromList()
    {
        int rand = UnityEngine.Random.Range(0, enemyPrefabs.Count);
        GameObject minionToReturn = enemyPrefabs[rand];
        return minionToReturn;
    }

    public List<GameObject> GetEnemyPrefabs()
    {
        return enemyPrefabs;
    }

    public Vector3 GetPlayerEntryPoint()
    {
        return playerEntryPoint;
    }

    public GameObject[] GetAsteroids()
    {
        return asteroids;
    }

    public List<Vector2> GetSpawnPoints()
    {
        return spawnPoints;
    }

    public Vector2 GetPortalLocation()
    {
        return portalLocation;
    }


}
