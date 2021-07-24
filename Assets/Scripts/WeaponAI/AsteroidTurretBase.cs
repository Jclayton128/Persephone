using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidTurretBase : MonoBehaviour
{
    Transform transformToMatch;
    Health hostHealth;

    // Update is called once per frame
    void Update()
    {
        transform.position = transformToMatch.position;
        transform.rotation = transformToMatch.rotation;
    }

    public void AssignTransformToMatch(Transform targetTransform)
    {
        transformToMatch = targetTransform;
        hostHealth = targetTransform.gameObject.GetComponent<Health>();
        hostHealth.EntityIsDying += ReactToAsteroidDying;

    }

    private void ReactToAsteroidDying()
    {
        hostHealth.EntityIsDying -= ReactToAsteroidDying;
        Destroy(gameObject);
    }
}
