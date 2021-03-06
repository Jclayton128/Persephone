//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ProtoScrap : NetworkBehaviour
{
    //init
    [SerializeField] Sprite[] scrapSprites = null;
    [SerializeField] CircleCollider2D trigColl = null;
    SpriteRenderer sr;

    //parameter
    float lifetime = 20;  //20
    float lifetimeRandomFactor = 3.0f; //3

    //hood
    float actualLifetime;
    float fadeTime;
    float deathTime;
    bool isFading = false;
    void Start()
    {
        actualLifetime = lifetime + Random.Range(-lifetimeRandomFactor, lifetimeRandomFactor);
        fadeTime = Time.time + (actualLifetime * .85f);
        deathTime = Time.time + actualLifetime;
        Sprite selectedSprite = SelectARandomSprite();
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = selectedSprite;
        if (isClient)
        {
            trigColl.enabled = false;
        }
    }

    private Sprite SelectARandomSprite()
    {
        int randomInt = UnityEngine.Random.Range(0, scrapSprites.Length);
        Sprite chosen = scrapSprites[randomInt];
        return chosen;
    }

    private void Update()
    {
        if (Time.time >= fadeTime )
        {
            if (!isFading)
            {
                isFading = true;
                StartCoroutine(nameof(FadeOut));
            }

            if (Time.time >= deathTime)
            {
                StopAllCoroutines();
                Destroy(gameObject);
            }
        }
    }

    IEnumerator FadeOut()
    {

        float timeSpentFading = 0;
        float fadeoutDuration = actualLifetime * .15f;
        float factor = 1 ;
        while (true)
        {
            timeSpentFading += Time.deltaTime;
            factor = (fadeoutDuration - timeSpentFading);
            sr.color = new Color(1, 1, 1, factor);
            yield return new WaitForEndOfFrame();
        }
    } 



}
