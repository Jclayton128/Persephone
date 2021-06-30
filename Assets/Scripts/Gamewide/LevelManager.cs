﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class LevelManager : NetworkBehaviour
{
    MinionMaker mm;
    UnitTracker ut;

    [SerializeField] GameObject persephonePrefab = null;
    [SerializeField] TextMeshProUGUI levelCounterTMP = null;
    [SerializeField] List<Level> unencounteredLevels = null;
    List<Level> encounteredLevels = new List<Level>();

    static Level currentLevel;
    PersephoneBrain pb;

    [SyncVar(hook = nameof(UpdateLevelCountUI))]
    int currentLevelCount = 0;

    float timeUntilPersephoneArrives = 5f;

    public Action<int> OnLevelAdvance;

    private void Awake()
    {
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
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        GameObject warpPortal = GameObject.FindGameObjectWithTag("WarpPortal");
        warpPortal.GetComponent<Rigidbody2D>().angularVelocity = 10f;
    }

    public int GetCurrentLevelCount()
    {
        return currentLevelCount;
    }

    [Server]
    public void AdvanceToNextLevel()
    {
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
        for (int i = currentLevelCount; i > 0; i--)
        {
            GameObject minion = currentLevel.ReturnRandomEnemyFromList();
            mm.SpawnNewMinion(minion);
        }
    }

    private void StartPersephone()
    {
        if (isServer)
        {
            GameObject persephone = Instantiate(persephonePrefab, Vector2.zero, Quaternion.identity) as GameObject;
            pb = persephone.GetComponent<PersephoneBrain>();
            pb.StartPersephone();
            NetworkServer.Spawn(persephone);

        }

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
