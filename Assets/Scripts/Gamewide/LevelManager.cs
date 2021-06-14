using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class LevelManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(UpdateLevelCountUI))]
    int currentLevelCount = 0;
    MinionMaker mm;
    [SerializeField] TextMeshProUGUI levelCounterTMP = null;
    [SerializeField] List<Level> levelList = null;
    [SerializeField] static Level currentLevel;
    [SerializeField] PersephoneBrain pb;

    float timeUntilPersephoneArrives = 5f;

    private void Awake()
    {
        foreach (Level level in levelList)
        {
            level.RegisterLevelMinions();
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        mm = GetComponent<MinionMaker>();
    }

    private void Start()
    {
        pb = GameObject.FindGameObjectWithTag("Persephone").GetComponent<PersephoneBrain>();
    }

    public int GetCurrentLevelCount()
    {
        return currentLevelCount;
    }

    public void AdvanceToNextLevel()
    {
        StartPersephone(); // Only really needed to start the first level
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
    }

    private void RemoveCurrentLevelFromList()
    {
        levelList.Remove(currentLevel);
    }

    private void ChooseNextLevel()
    {
        int rand = UnityEngine.Random.Range(0, levelList.Count);
        currentLevel = levelList[rand];
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
        pb.StartPersephone();
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
