using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Ability_RepellingPulse : Ability
{
    [SerializeField] float repelRadius;
    [SerializeField] float repelForce;
    int primaryLayerToRepel = 10;
    int secondaryLayerToRepel = 18;
    int tertiaryLayerToRepel = 19;
    protected override void MouseClickDownEffect()
    {
        CmdRequestFirePulse();

        //TODO audio
    }

    [Command]
    private void CmdRequestFirePulse()
    {
        if (es.CheckSpendEnergy(costToActivate))
        {
            FirePulse();
        }
    }

    private void FirePulse()
    {
        int layerMask_enemies = (1 << primaryLayerToRepel) | (1 << secondaryLayerToRepel) | (1 << tertiaryLayerToRepel);
        int layerMask_scrap = 1 << 14;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, repelRadius, layerMask_enemies);
        foreach (Collider2D hit in hits)
        {
            Vector2 dir = (hit.transform.position - transform.position);
            float distance = dir.magnitude;
            float factor = 1 - (distance / repelRadius);

            hit.GetComponentInParent<Rigidbody2D>().AddForce(dir * repelForce * factor, ForceMode2D.Impulse);
        }

        Collider2D[] scraps = Physics2D.OverlapCircleAll(transform.position, repelRadius, layerMask_scrap);
        foreach (Collider2D scrap in scraps)
        {
            Vector2 dir = (scrap.transform.position - transform.position);
            float distance = dir.magnitude;
            float factor = 1 - (distance / repelRadius);

            scrap.GetComponentInParent<Rigidbody2D>().AddForce(-1/10f * dir * repelForce * factor, ForceMode2D.Impulse);
        }
    }

    protected override void MouseClickUpEffect()
    {

    }
}
