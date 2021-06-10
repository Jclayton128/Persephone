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
    [SerializeField] Level currentLevel;

    public override void OnStartServer()
    {
        base.OnStartServer();
        mm = GetComponent<MinionMaker>();
    }

    public int GetCurrentLevelCount()
    {
        return currentLevelCount;
    }

    public void AdvanceToNextLevel()
    {
        Debug.Log("Advancing to next level");
        ClearOutOldLevel();
        RemoveCurrentLevelFromList();
        ChooseNextLevel();
        IncrementLevelCount();
        ResetPlayerPositions();
        SpawnNextLevelMinions();
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

    private void SpawnNextLevelMinions()
    {

        for (int i = currentLevelCount; i > 0; i--)
        {
            GameObject minion = currentLevel.ReturnRandomEnemyFromList();
            mm.SpawnNewMinion(minion);
        }
    }

    private void IncrementLevelCount()
    {
        currentLevelCount++;
    }

    private void UpdateLevelCountUI(int oldValue, int newValue)
    {
        levelCounterTMP.text = "Level: " + currentLevelCount.ToString();
    }


}
