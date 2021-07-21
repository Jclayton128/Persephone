using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[CreateAssetMenu(fileName = "Level")]
public class Level : ScriptableObject
{
    [SerializeField] List<GameObject> minionPrefabs = new List<GameObject>();
    [SerializeField] List<Vector2> spawnPoints = new List<Vector2>();
    [SerializeField] Vector2 portalLocation = Vector2.up;
    public enum AsteroidLevel { Heavy, Medium, Sparse, None};
    [SerializeField] public AsteroidLevel asteroidLevel;
    [SerializeField] Vector3 playerEntryPoint = new Vector3(0, 0, 0);
    public float fogLevel = 0; // must be between 0 and 1;
    public bool isBossLevel = false;

    public GameObject ReturnRandomEnemyFromList()
    {
        int rand = Random.Range(0, minionPrefabs.Count);
        GameObject minionToReturn = minionPrefabs[rand];
        return minionToReturn;
    }

    public void RegisterLevelMinions()
    {

        foreach (GameObject prefab in minionPrefabs)
        {
            NetworkClient.RegisterPrefab(prefab);
        }

    }

    public List<GameObject> GetEnemyPrefabs()
    {
        return minionPrefabs;
    }

    public GameObject GetMinionAtLevelIndex(int index)
    {
        return minionPrefabs[index];
    }

    public Vector3 GetPlayerEntryPoint()
    {
        return playerEntryPoint;
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
