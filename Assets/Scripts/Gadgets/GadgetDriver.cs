using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GadgetDriver : NetworkBehaviour
{
    //init
    [SerializeField] List<Gadget> secondaryGadgetsOnboard = new List<Gadget>();

    //param
    [SerializeField] int gadgetSlots;

    //hood
    public Gadget primaryGadget { get; private set; }
    public Gadget secondaryGadget { get; private set; }
    [SerializeField] int secondaryGadgetIndex = 1;

    void Start()
    {
        PopulateGadgetList();
        secondaryGadgetIndex = 0;
        if (hasAuthority)
        {
            HookIntoLocalUI();
        }
    }

    private void PopulateGadgetList()
    {
        Gadget[] gadgets = GetComponentsInChildren<Gadget>();
        foreach (Gadget gadget in gadgets)
        {
            if (gadget.IsPrimaryGadget)
            {
                primaryGadget = gadget;
            }
            else
            {
                secondaryGadgetsOnboard.Add(gadget);
            }
        }
    
    }

    private void HookIntoLocalUI()
    {
        //hook into UI;
    }

    public void IncrementGadgetSelection()
    {
        secondaryGadgetIndex--;
        if (secondaryGadgetIndex < 0)
        {
            secondaryGadgetIndex = secondaryGadgetsOnboard.Count - 1;
        }
        UpdateUI();
    }

    public void DecrementGadgetSelection()
    {
        secondaryGadgetIndex++;
        if (secondaryGadgetIndex > secondaryGadgetsOnboard.Count - 1)
        {
            secondaryGadgetIndex = 0;
        }
        UpdateUI();
    }
    
    private void UpdateUI()
    {

    }





}
