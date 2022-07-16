using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{

    [Header("Variable")]
    [SerializeField] private ParticleSystem collideParticle;

    public void OnObjectHit()
    {
        collideParticle.transform.parent = null;
        collideParticle.gameObject.SetActive(true);

        Destroy(gameObject);
    }
}
