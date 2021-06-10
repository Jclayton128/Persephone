using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class LevelManager : NetworkBehaviour
{
    [SyncVar (hook = nameof(UpdateLevelCountUI))]   
    int currentLevel = 0;
    MinionMaker mm;
    [SerializeField] TextMeshProUGUI levelCounterTMP = null;

    public override void OnStartServer()
    {
        base.OnStartServer();
        mm = GetComponent<MinionMaker>();
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public void AdvanceToNextLevel()
    {
        ClearOutOldLevel();

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
        for (int i = currentLevel; i > 0; i--)
        {
            mm.SpawnNewMinion();
        }
    }

    private void IncrementLevelCount()
    {
        currentLevel++;
    }

    private void UpdateLevelCountUI(int oldValue, int newValue)
    {
        levelCounterTMP.text = "Level: " + currentLevel.ToString();
    }


}
