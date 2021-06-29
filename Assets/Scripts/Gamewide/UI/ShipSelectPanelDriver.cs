using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;
using TMPro;

public class ShipSelectPanelDriver : NetworkBehaviour
{
    // Start is called before the first frame update
    public ClientInstance ci;
    Vector3 displayPos = Vector3.zero;
    Vector3 hidePos = new Vector3(999, 999, 0);
    [SerializeField] GameObject[] avatarPrefabs = null;
    [SerializeField] Button[] avatarButtons = null;
    [SerializeField] TextMeshProUGUI choiceName = null;
    [SerializeField] TextMeshProUGUI choiceDescription = null;

    GameObject chosenAvatar;
    void Start()
    {
        if (isClient)
        {
            foreach (GameObject avatar in avatarPrefabs)
            {
                NetworkClient.RegisterPrefab(avatar);
            }

        }
        SetButtonImagesToAvatarIcons();
    }

    private void SetButtonImagesToAvatarIcons()
    {
        if (avatarPrefabs.Length > avatarButtons.Length)
        {
            Debug.Log("more avatar choices than buttons !");
            return;
        }
        for (int i = 0; i < avatarPrefabs.Length; i++)
        {
            avatarButtons[i].GetComponent<Image>().sprite = avatarPrefabs[i].GetComponent<SpriteRenderer>().sprite;
        }       

    }

    public void FillDataFieldsOnHoverEnter(int index)
    {
        ShipyardInfo syi = avatarPrefabs[index].GetComponent<ShipyardInfo>();
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
        if (index > avatarPrefabs.Length - 1)
        {
            Debug.Log("Invalid selection, exceeds array");
            return;
        }
        else
        {
            chosenAvatar = avatarPrefabs[index];
            Debug.Log($"player selected the {chosenAvatar.GetComponent<ShipyardInfo>().ShipName}");
        }

    }
    public void PushSelectPrefabToClient(int index)
    {
        ci.SetDesiredAvatar(index);
        HidePanel();
        //gameObject.SetActive(false);
    }

    public void DisplayPanel()
    {
        transform.position = displayPos;

    }

    public void HidePanel()
    {
        transform.position = hidePos;
    }


}
