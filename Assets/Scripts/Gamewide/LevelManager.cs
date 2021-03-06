using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class LevelManager : NetworkBehaviour
{
    MinionMaker mm;
    UnitTracker ut;
    ArenaBounds ab;

    [SerializeField] GameObject warpPortalPrefab = null;
    [SerializeField] GameObject[] asteroidPrefabsLarge2Small = null;
    [SerializeField] GameObject persephonePrefab = null;
    [SerializeField] TextMeshProUGUI levelCounterTMP = null;
    [SerializeField] List<Level> unencounteredLevels = null;
    [SerializeField] List<Level> encounteredLevels = new List<Level>();
    [SerializeField] GameObject gameOverScreenPrefab = null;

    static Level currentLevel;
    PersephoneBrain pb;
    PersephoneHealth ph;

    [SyncVar(hook = nameof(UpdateLevelCountUI))]
    int currentLevelCount = 0;

    float timeUntilPersephoneArrives = 5f;

    public Action<int> OnLevelAdvance;
    GameObject currentWarpPortal;

    private void Awake()
    {
        foreach (GameObject asteroid in asteroidPrefabsLarge2Small)
        {
            if (!NetworkClient.prefabs.ContainsValue(asteroid))
            {
                NetworkClient.RegisterPrefab(asteroid);
            }
        }
        NetworkClient.RegisterPrefab(warpPortalPrefab);
        NetworkClient.RegisterPrefab(persephonePrefab);
        foreach (Level level in unencounteredLevels)
        {
            level.RegisterLevelMinions();
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        mm = GetComponent<MinionMaker>();
        ut = GetComponent<UnitTracker>();
        ab = FindObjectOfType<ArenaBounds>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public int GetCurrentLevelCount()
    {
        return currentLevelCount;
    }

    [Server]
    public void AdvanceToNextLevel()
    {
        if (!isServer) { return; }
        if (pb == null)
        {
            StartPersephone(); // Only really needed to start the first level
        }
        ClearOutOldLevel();
        RemoveCurrentLevelFromList();
        ChooseNextLevel();
        IncrementLevelCount();
        ResetPlayerPositions();
        SpawnNextLevelMinions();
        SpawnNextLevelAsteroids();
        SpawnNewWarpPortal();
        SetTimeUntilPersephoneArrives();
    }

    private void ClearOutOldLevel()
    {
        var weapons = FindObjectsOfType<DamageDealer>(); //clear out all weaponry.  Appears to be working.
        foreach (var weapon in weapons)
        {
            Destroy(weapon.transform.gameObject);
        }

        var scraps = GameObject.FindGameObjectsWithTag("Scrap");
        foreach (GameObject scrap in scraps)
        {
            Destroy(scrap);
        }

        var asteroids = GameObject.FindGameObjectsWithTag("Asteroid");
        foreach (GameObject asteroid in asteroids)
        {
            Destroy(asteroid);
        }
        if (currentWarpPortal)
        {
            Destroy(currentWarpPortal);
        }

        ut.DestroyAllMinions();
        
    }

    private void RemoveCurrentLevelFromList()
    {
        unencounteredLevels.Remove(currentLevel);
        encounteredLevels.Add(currentLevel);
    }

    private void ChooseNextLevel()
    {
        if (unencounteredLevels.Count == 0)
        {
            foreach (Level level in encounteredLevels)
            {
                unencounteredLevels.Add(level);
            }
            encounteredLevels.Clear();
            encounteredLevels.TrimExcess();
        }

        int rand = UnityEngine.Random.Range(0, unencounteredLevels.Count);
        currentLevel = unencounteredLevels[rand];
        
    }

    private void ResetPlayerPositions()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.transform.position = CUR.CreateRandomPointNearInputPoint(Vector2.zero, 1, 0.3f);
        }
    }

    private void IncrementLevelCount()
    {
        currentLevelCount++;
        OnLevelAdvance?.Invoke(currentLevelCount);
    }

    private void SpawnNextLevelMinions()
    {
        if (isServer)
        {
            for (int i = currentLevelCount; i > 0; i--)
            {
                Debug.Log("spawned a minion");
                GameObject minion = currentLevel.ReturnRandomEnemyFromList();
                mm.SpawnNewMinion(minion);
            }
        }

    }

    private void SpawnNextLevelAsteroids()
    {
        int quantity;
        switch (currentLevel.asteroidLevel)
        {
            case Level.AsteroidLevel.None:
                return;

            case Level.AsteroidLevel.Sparse:
                quantity = UnityEngine.Random.Range(0, 4);
                for (int i = 0; i < quantity; i++)
                {
                    int size = UnityEngine.Random.Range(2, 3);
                    GameObject asteroid = Instantiate(asteroidPrefabsLarge2Small[size], ab.CreateRandomPointWithinArena(), Quaternion.identity) as GameObject;
                    asteroid.GetComponent<Asteroid>().SetRandomStartingMotion();
                    NetworkServer.Spawn(asteroid);
                }
                return;

            case Level.AsteroidLevel.Medium:
                quantity = UnityEngine.Random.Range(2, 7);
                for (int i = 0; i < quantity; i++)
                {
                    int size = UnityEngine.Random.Range(1, 3);
                    GameObject asteroid = Instantiate(asteroidPrefabsLarge2Small[size], ab.CreateRandomPointWithinArena(), Quaternion.identity) as GameObject;
                    asteroid.GetComponent<Asteroid>().SetRandomStartingMotion();
                    NetworkServer.Spawn(asteroid);
                }
                return;

            case Level.AsteroidLevel.Heavy:
                quantity = UnityEngine.Random.Range(5, 12);
                for (int i = 0; i < quantity; i++)
                {
                    int size = UnityEngine.Random.Range(0, 2);
                    GameObject asteroid = Instantiate(asteroidPrefabsLarge2Small[size], ab.CreateRandomPointWithinArena(), Quaternion.identity) as GameObject;
                    asteroid.GetComponent<Asteroid>().SetRandomStartingMotion();
                    NetworkServer.Spawn(asteroid);
                }
                return;

        }

    }

    private void SpawnNewWarpPortal()
    {
        currentWarpPortal = Instantiate(warpPortalPrefab, Vector2.zero, Quaternion.identity) as GameObject;
        NetworkServer.Spawn(currentWarpPortal);
    }
    private void StartPersephone()
    {
        if (isServer)
        {
            Time.timeScale = 1;
            GameObject persephone = Instantiate(persephonePrefab, Vector2.zero, Quaternion.identity) as GameObject;
            pb = persephone.GetComponent<PersephoneBrain>();
            pb.StartPersephone();
            ph = persephone.GetComponent<PersephoneHealth>();
            ph.PersephoneIsDead += ReactToPersephoneDeath;
            NetworkServer.Spawn(persephone);

        }

    }

    [Server]
    private void ReactToPersephoneDeath()
    {
        RpcPushGameOverScreenOnClients(currentLevelCount);
        Time.timeScale = 0;
        pb = null;
        ph = null;
        currentLevel = null;
        currentLevelCount = 0;
        ClearOutAllPlayers();
    }

    private void ClearOutAllPlayers()
    {
        var avatars = FindObjectsOfType<PlayerInput>();
        foreach (var avatar in avatars)
        {
            Destroy(avatar.transform.gameObject);
        }
    }

    [ClientRpc]
    private void RpcPushGameOverScreenOnClients(int levelReached)
    {
        GameObject screen = Instantiate(gameOverScreenPrefab) as GameObject;
        screen.GetComponent<GameOverScreenDriver>().LevelReachedTMP.text = "You made it to level: " + levelReached.ToString();
    }

    private void SetTimeUntilPersephoneArrives()
    {
        pb.SetTimerUponLevelStart(timeUntilPersephoneArrives);
    }

    private void UpdateLevelCountUI(int oldValue, int newValue)
    {
        levelCounterTMP.text = "Level: " + currentLevelCount.ToString();
    }

    public static Level GetCurrentLevel()
    {
        return currentLevel;
    }

}
