using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverScreenDriver : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI LevelReachedTMP = null;


    public void TryAgainButtonPress()
    {
        ShipSelectPanelDriver sspd = ClientInstance.ReturnClientInstance().sspd;

        sspd.gameObject.SetActive(true);
        sspd.chosenAvatarIndex = -1;
        Destroy(gameObject);
    }
}
