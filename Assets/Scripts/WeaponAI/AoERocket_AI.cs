using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoERocket_AI : MonoBehaviour
{
    Rigidbody2D rb;

    //param
    float normalDamage = 3;
    float blastRadius = 2;
    float torque = 10f;
    float turnRate;
    float baseTurnRate = 70f;
    float angularCloseEnough = 10f;
    int speedTurnRateFactor = 10;
    public float speed { get; set; } //set via param1 from the launcher as Instantiation

    //hood
    Vector3 navTarget;
    Vector3 diff;
    float timeSinceStart = 0;

    void Start()
    {
        //particleExplosion.SetActive(false);
        rb = GetComponent<Rigidbody2D>();
        SpeedTurnRate();
    }

    // Update is called once per frame
    void Update()
    {
        FlyToTargetPoint();
    }

    private void SpeedTurnRate()
    {
        turnRate = (speed * speedTurnRateFactor) + baseTurnRate;
    }

    public void SetNavTarget(Vector3 pos)
    {
        navTarget = pos;
    }

    private void FlyToTargetPoint()
    {
        rb.velocity = transform.up * speed;
        diff = navTarget - transform.position;
        float distToTarget = diff.magnitude;
        float degreesOffBoresight = Vector2.SignedAngle(transform.up, diff);
        //Debug.Log(degreesOffBoresight);

        if (degreesOffBoresight > angularCloseEnough)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, turnRate, torque * Time.deltaTime);
        }
        if (degreesOffBoresight < -1 * angularCloseEnough)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, -turnRate, torque * Time.deltaTime);
        }
    }

    private void OnDestroy()
    {
        Collider2D[] splashRecipients = Physics2D.OverlapCircleAll(transform.position, blastRadius);
        foreach (Collider2D splashRecipient in splashRecipients)
        {
            Health health;
            if (splashRecipient.gameObject.TryGetComponent<Health>(out health))
            {
                health.ModifyShieldLevel(-1 * normalDamage, true);
            }
        }
        //GameObject exp = Instantiate(particleExplosion, transform.position, transform.rotation) as GameObject;
        //exp.SetActive(true);
        //Destroy(exp, 3f);
    }
}

