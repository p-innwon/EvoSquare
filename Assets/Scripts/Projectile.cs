using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Variable")]
    [SerializeField] private ParticleSystem collideParticle;
    [SerializeField] private float projectileSpeed = 4.0f;
    [SerializeField] private float timeToLive = 2f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, timeToLive);
    }

    void FixedUpdate()
    {
        transform.Translate(Vector3.up * projectileSpeed * Time.deltaTime);
    }

    public void OnObjectHit()
    {
        ParticleSystem particle = Instantiate(collideParticle, transform.position, transform.rotation);

        Destroy(gameObject);
    }
}
