using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Asteroid : NetworkBehaviour
{
    [SerializeField] GameObject[] componentAsteroids = null;
    [SerializeField] Sprite[] asteroidSprites = null;
    [SerializeField] float asteroidRadius;
    public Vector2 startingVel { get; set; }

    float randomSpeedMax = 4f;
    float randomSpinMax = 30;
    float radiusToHealthMassCoefficient = 10f; // for every xx of Collider Radius, gain YY health points and mass.

    Rigidbody2D rb;
    CircleCollider2D coll;
    SpriteRenderer sr;
    Health health;

    public override void OnStartServer()
    {
        base.OnStartServer();
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = startingVel;
        health = GetComponent<Health>();
        coll = GetComponent<CircleCollider2D>();
        SelectStartingColliderSizeMassHealth();
        health.EntityIsDying += HandleAsteroidDying;

    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        sr = GetComponent<SpriteRenderer>();
        SelectStartingSprite();
    }

    public void SelectStartingSprite()
    {
        int rand;
        rand = UnityEngine.Random.Range(0, asteroidSprites.Length);
        sr.sprite = asteroidSprites[rand];
    }

    private void SelectStartingColliderSizeMassHealth()
    {
        coll.radius = asteroidRadius;
        rb.mass = coll.radius * radiusToHealthMassCoefficient;
        health.SetMaxHullAndHealToIt(coll.radius * radiusToHealthMassCoefficient);
    }

    public void SetRandomStartingMotion()
    {
        startingVel = CUR.GetPointOnUnitCircleCircumference() * UnityEngine.Random.Range(0,randomSpeedMax);
        //rb.angularVelocity = UnityEngine.Random.Range(-randomSpinMax, randomSpinMax);
    }

    public void SetOutwardStartingMotion(int total, int indexWithinTotal)
    {
        float circleSubdivided = 360f / total;
        float middleDeg = indexWithinTotal * circleSubdivided + transform.eulerAngles.z + (360f / 2f) + 180f;

        startingVel = CUR.GetPointOnUnitCircleCircumference(middleDeg - 30f, middleDeg + 30f);

    }


    private void HandleAsteroidDying()
    {
        if (componentAsteroids.Length > 0)
        {
            for (int i = 0; i < componentAsteroids.Length; i++)
            {
                Vector2 randOffset = CUR.GetPointOnUnitCircleCircumference() * coll.radius + new Vector2(transform.position.x, transform.position.y);
                Quaternion randRot = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-179f, 179f));
                GameObject newAsteroid = Instantiate(componentAsteroids[i], randOffset, randRot) as GameObject;
                Asteroid asteroid = newAsteroid.GetComponent<Asteroid>();
                asteroid.SetOutwardStartingMotion(componentAsteroids.Length, i);
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
