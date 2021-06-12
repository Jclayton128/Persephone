﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapDropper : MonoBehaviour
{
    //init
    [SerializeField] GameObject scrapPrefab = null;
    [SerializeField] GameObject[] asteroidPrefabs = null;


    //param
    [SerializeField] int numberOfScraps = 1;
    [SerializeField] int numberOfAsteroids = 0;

    float driftSpeed = 1.5f;
    float maxAngularVelocity = 20;

    //hood



    void Start()
    {
        Health health = GetComponent<Health>();
        health.EntityIsDying += SpawnScrapAtDeath;
        // TODO maybe have some minions or weapons where scrap flies off if damaged? //health.EntityWasDamaged += 

    }
    public void ModifyScrapLevel(int amount)
    {
        if (numberOfScraps > 0)
        {
            numberOfScraps += amount;
        }
    }

    public void SpawnScrapAtDeath()
    {
        for (int i = 0; i < numberOfScraps; i++)
        {
            Vector3 randomPos = CUR.CreateRandomPointNearInputPoint(transform.position, 0.5f, 0.2f);
            Vector3 driftDir = randomPos - transform.position;
            GameObject scrap = Instantiate(scrapPrefab, randomPos, Quaternion.identity);
            Rigidbody2D scrapRB = scrap.GetComponent<Rigidbody2D>();
            scrapRB.velocity = driftDir.normalized * driftSpeed;
            //Debug.Log("drift dir: " + driftDir + " velocity: " + scrapRB.velocity);
            scrapRB.angularVelocity = Random.Range(-maxAngularVelocity, maxAngularVelocity);
        }
        if (asteroidPrefabs.Length > 0 && numberOfAsteroids > 0)
        {
            for (int i = 0; i < numberOfAsteroids; i++)
            {
                Vector3 randomPos = CUR.CreateRandomPointNearInputPoint(transform.position, 0.5f, 0.2f);
                Vector3 driftDir = randomPos - transform.position;
                int randomInt = UnityEngine.Random.Range(0, asteroidPrefabs.Length);
                GameObject newRock = Instantiate(asteroidPrefabs[randomInt], randomPos, Quaternion.identity);
                Rigidbody2D newRockRB = newRock.GetComponent<Rigidbody2D>();
                newRockRB.velocity = driftDir.normalized * driftSpeed;
                //Debug.Log("drift dir: " + driftDir + " velocity: " + scrapRB.velocity);
                newRockRB.angularVelocity = Random.Range(-maxAngularVelocity, maxAngularVelocity);
            }
        }
    }


}
