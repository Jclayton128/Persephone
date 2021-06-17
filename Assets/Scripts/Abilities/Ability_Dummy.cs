using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_Dummy : Ability
{
    public AudioClip dummyAbilityAttemptedAudioClip;
    protected override void Awake()
    {        
    }

    protected override void Start()
    {
    }

    public override void MouseClickDownValidate()
    {
        MouseClickDownEffect();
    }

    public override void MouseClickUpValidate()
    {
        MouseClickUpEffect();
    }

    protected override void MouseClickDownEffect()
    {
        //TODO insert No-ability selected negative UI sound
        Debug.Log("Trying to fire the dummy ability");
    }

    protected override void MouseClickUpEffect()
    {
    }
}
