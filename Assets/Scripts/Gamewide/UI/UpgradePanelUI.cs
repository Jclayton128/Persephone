using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;

public class UpgradePanelUI : MonoBehaviour
{
    //init
    RectTransform thisRect;
    [SerializeField] Slider upgradeSelector = null;
    [SerializeField] Image option1Image = null;
    [SerializeField] TextMeshProUGUI optionCountTMP1 = null;
    [SerializeField] Image option2Image = null;
    [SerializeField] TextMeshProUGUI optionCountTMP2 = null;
    [SerializeField] Image option3Image = null;
    [SerializeField] TextMeshProUGUI optionCountTMP3 = null;
    [SerializeField] TextMeshProUGUI explainerTMP = null;
    [SerializeField] RectTransform extendedPosition = null;
    [SerializeField] RectTransform retractedPosition = null;

    float lerpSpeed = 400f;
    public bool IsExtended { get; private set; } = false;
    int selectionIndex;

    private void Start()
    {
        thisRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (IsExtended)
        {
            float x = Mathf.MoveTowards(thisRect.position.x, extendedPosition.position.x, lerpSpeed * Time.deltaTime);
            thisRect.position = new Vector2(x, thisRect.position.y);
        }
        if (!IsExtended)
        {
            float x = Mathf.MoveTowards(thisRect.position.x, retractedPosition.position.x, lerpSpeed * Time.deltaTime);
            thisRect.position = new Vector2(x, thisRect.position.y);
        }
    }

    public void SetSelectorKnob(int position1thru3)
    {
        //if (position1thru3 != 1 || position1thru3 != 2 || position1thru3 != 3)
        //{
        //    Debug.Log("UI only has places for 1, 2, or 3 for the selection");
        //    return;
        //}
        upgradeSelector.value = position1thru3;
    }

    public void TogglePanelPosition()
    {
        IsExtended = !IsExtended;
    }


}
