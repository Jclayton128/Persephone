using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WreckerDroneBrain : MonoBehaviour
{
    //init
    GameObject repairTarget;



    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"here I am, ready to fix {repairTarget}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetRepairTarget(GameObject target)
    {
        repairTarget = target;
    }
}
