using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;
using TMPro;

public class ShipSelectPanelDriver : MonoBehaviour
{
    // Start is called before the first frame update
    public ClientInstance ci;
    PlayerShipyard ps;
    Vector3 displayPos = Vector3.zero;
    Vector3 hidePos = new Vector3(999, 999, 0);

    [SerializeField] Button[] avatarButtons = null;
    [SerializeField] TextMeshProUGUI choiceName = null;
    [SerializeField] TextMeshProUGUI choiceDescription = null;

    public int chosenAvatarIndex { get; private set; } = -1;
    void Start()
    {
        ps = FindObjectOfType<PlayerShipyard>();
        SetButtonImagesToAvatarIcons();
    }

    private void SetButtonImagesToAvatarIcons()
    {
        if (ps.allAvatarPrefabs.Count > avatarButtons.Length)
        {
            Debug.Log("more avatar choices than buttons !");
            return;
        }
        for (int i = 0; i < ps.allAvatarPrefabs.Count; i++)
        {
            avatarButtons[i].GetComponent<Image>().sprite = ps.allAvatarPrefabs[i].GetComponent<ShipyardInfo>().shipyardSprite;
        }       

    }

    public void FillDataFieldsOnHoverEnter(int index)
    {
        ShipyardInfo syi = ps.allAvatarPrefabs[index].GetComponent<ShipyardInfo>();
        choiceName.text = syi.ShipName;
        choiceDescription.text = syi.ShipDescription;
    }

    public void ClearDataFieldsOnHoverExit()
    {
        choiceName.text = " ";
        choiceDescription.text = " ";
    }

    public void SelectAvatar(int index)
    {
        if (index > ps.allAvatarPrefabs.Count - 1)
        {
            Debug.Log("Invalid selection, exceeds array");
            return;
        }
        else
        {
            chosenAvatarIndex = index;  // chosenAvatar is set on client side 
            Debug.Log($"player selected the {ps.allAvatarPrefabs[index].GetComponent<ShipyardInfo>().ShipName}");

        }

    }

    public void LaunchIntoGame()
    {
        if (chosenAvatarIndex != -1)
        {
            ci.LaunchGame();         
            HidePanel();
        }
    }

    public void DisplayPanel()
    {
        transform.position = displayPos;

    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
        //transform.position = hidePos;
    }


}
