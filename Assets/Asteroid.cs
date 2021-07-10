using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Asteroid : NetworkBehaviour
{
    [SerializeField] Sprite[] giantAsteroidSprites = null;
    [SerializeField] Sprite[] largeAsteroidSprites = null;
    [SerializeField] Sprite[] mediumAsteroidSprites = null;
    [SerializeField] Sprite[] smallAsteroidSprites = null;
    [SerializeField] float[] asteroidRadii;

    float randomSpeedMax = 4f;
    float randomSpinMax = 30;
    float radiusToHealthMassCoefficient = 10f; // for every xx of Collider Radius, gain YY health points and mass.

    Rigidbody2D rb;
    CircleCollider2D coll;
    SpriteRenderer sr;
    Health health;


    public enum AsteroidSize { Giant, Large, Medium, Small};

    [SyncVar]
    [SerializeField] public AsteroidSize asteroidSize;

    public void InitializeAsteroid()
    {
        Debug.Log("okay this at least was called");
        if (isClient)
        {
            Debug.Log("this was called");
            sr = GetComponent<SpriteRenderer>();
            SelectStartingSprite();
        }
        if (isServer)
        {
            Debug.Log("this too was called");
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();
            coll = GetComponent<CircleCollider2D>();
            SelectStartingColliderSizeAndMass();
            SelectStartingHealth();
            CreateRandomStartingMotion();
            health.EntityIsDying += HandleAsteroidDying;
        }
        sr = GetComponent<SpriteRenderer>();
        SelectStartingSprite();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        coll = GetComponent<CircleCollider2D>();
        SelectStartingColliderSizeAndMass();
        SelectStartingHealth();
        CreateRandomStartingMotion();
        health.EntityIsDying += HandleAsteroidDying;

    }

    private void SelectStartingSprite()
    {
        int rand;
        switch (asteroidSize)
        {

            case AsteroidSize.Giant:
                rand = UnityEngine.Random.Range(0, giantAsteroidSprites.Length);
                sr.sprite = giantAsteroidSprites[rand];
                return;

            case AsteroidSize.Large:
                rand = UnityEngine.Random.Range(0, largeAsteroidSprites.Length);
                sr.sprite = largeAsteroidSprites[rand];
                return;

            case AsteroidSize.Medium:
                rand = UnityEngine.Random.Range(0, mediumAsteroidSprites.Length);
                sr.sprite = mediumAsteroidSprites[rand];
                return;

            case AsteroidSize.Small:
                rand = UnityEngine.Random.Range(0, smallAsteroidSprites.Length);
                sr.sprite = smallAsteroidSprites[rand];
                return;

        }

    }

    private void SelectStartingColliderSizeAndMass()
    {
        coll.radius = asteroidRadii[(int)asteroidSize];
        rb.mass = coll.radius * radiusToHealthMassCoefficient;
    }
    private void SelectStartingHealth()
    {
        health.SetMaxHullAndHealToIt(coll.radius * radiusToHealthMassCoefficient);
    }
    public void CreateRandomStartingMotion()
    {
        rb.velocity = CUR.GetPointOnUnitCircleCircumference() * UnityEngine.Random.Range(0,randomSpeedMax);
        rb.angularVelocity = UnityEngine.Random.Range(-randomSpinMax, randomSpinMax);
    }


    private void HandleAsteroidDying()
    {
        if (asteroidSize != AsteroidSize.Small)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 randOffset = CUR.GetPointOnUnitCircleCircumference() * coll.radius + new Vector2(transform.position.x, transform.position.y);
                Quaternion randRot = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-179f, 179f));
                GameObject newAsteroid = Instantiate(gameObject, randOffset, randRot) as GameObject;
                Asteroid asteroid = newAsteroid.GetComponent<Asteroid>();
                asteroid.asteroidSize = asteroidSize++;
                asteroid.InitializeAsteroid();

                NetworkServer.Spawn(newAsteroid);
            }
        }
    }

    private void OnDestroy()
    {
        if (isServer)
        {
            health.EntityIsDying -= HandleAsteroidDying;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
