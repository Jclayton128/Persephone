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
    [SerializeField] string nullOptionExplanation;

    float lerpSpeed = 400f;
    public bool IsExtended { get; private set; } = false;

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

    public void SetSelectorKnob(int position0thru3, UpgradeOption selectedOption)
    {
        upgradeSelector.value = position0thru3;
        explainerTMP.text = selectedOption.Explanation;
    }

    public void SetSelectorKnob(int position0thru3)
    {
        upgradeSelector.value = position0thru3;
        explainerTMP.text = nullOptionExplanation;
    }

    public void TogglePanelPosition()
    {
        IsExtended = !IsExtended;
        if (!IsExtended)
        {
            upgradeSelector.value = 0;
        }
    }

    public void UpdateOptions(UpgradeOption option1, UpgradeOption option2, UpgradeOption option3 )
    {
        option1Image.sprite = option1.UpgradeIcon;
        optionCountTMP1.text = option1.PurchaseCount.ToString();

        option2Image.sprite = option2.UpgradeIcon;
        optionCountTMP2.text = option2.PurchaseCount.ToString();

        option3Image.sprite = option3.UpgradeIcon;
        optionCountTMP3.text = option3.PurchaseCount.ToString();
    }



}
