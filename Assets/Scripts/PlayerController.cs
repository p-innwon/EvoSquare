using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //gameobject reference
    [Header("Game Object Reference")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject gunIndicator;
    [SerializeField] private Slider gunCooldownSilder;

    //gun barrel
    [Header("Gun Barrel Reference")]
    [SerializeField] private Transform projectileBarrels;
    [SerializeField] private Transform projectileVerticalBarrels;

    //component
    [Header("Player Component")]
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private AudioSource playerAudio;
    [SerializeField] private AudioListener audioListener;

    //color with powerup
    [Header("Player Color")]
    [SerializeField] private Material defaultColorMaterial;
    [SerializeField] private Material movePowerupColorMaterial;

    //trail effect
    [Header("Player trail")]
    [SerializeField] private TrailRenderer trailRenderer;

    //sfx
    [Header("Sound Effect")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip getPowerupSound;
    [SerializeField] private AudioClip bounceSound;
    [SerializeField] private AudioClip swallowSound;

    //input
    [Header("Player Input")]
    [SerializeField] private float horizontalInput;
    [SerializeField] private float verticalInput;

    //variable
    [Header("Variable")]
    [SerializeField] private int playerSize = 0;
    [SerializeField] [Range(5, 50)] private float playerSpeed = 20.0f;
    [SerializeField] [Range(0, 10)] private float swallowForce = 6.0f;
    [SerializeField] private bool hasShot = false;
    [SerializeField] private bool hasBoostMoved = false;
    [SerializeField] private bool hasBoostGun = false;   
    [SerializeField] [Range(0, 50)] private float boostMoveSpeed = 20.0f;
    [SerializeField] [Range(0, 5)] private float boostGunSpeed = 0.5f;
    [SerializeField] [Range(0, 5)] private float shootCooldown = 1.5f;
    [SerializeField] [Range(0, 20)] private float boostDuration = 5.0f;
    [SerializeField] private bool isGameClear = false;
    private Coroutine boostMoveCoroutine;
    private Coroutine boostGunCoroutine;

    void Update()
    {
        if(GameManager.isGameActive)
        {
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");

            //fire projectile
            if(Input.GetKey(KeyCode.Space) && !hasShot)
            {
                hasShot = true;
                
                //fire projectiles
                ShootProjectiles();
                StartCoroutine(ShootCooldown());
            }
        }
    }
    void FixedUpdate()
    {
        if(GameManager.isGameActive)
        {
            MovePlayer();
        }
    }

    void ShootProjectiles()
    {
        for(int i = 0; i < projectileBarrels.childCount; i++)
        {
            Transform selectedProjectileBarrel = projectileBarrels.GetChild(i);
            Instantiate(projectilePrefab, selectedProjectileBarrel.position, selectedProjectileBarrel.rotation);
        }

        if(hasBoostGun)
        {
            for(int i = 0; i < projectileVerticalBarrels.childCount; i++)
            {
                Transform selectedProjectileBarrel = projectileVerticalBarrels.GetChild(i);
                Instantiate(projectilePrefab, selectedProjectileBarrel.position, selectedProjectileBarrel.rotation);
            }
        }

        playerAudio.PlayOneShot(shootSound);
    }

    void MovePlayer()
    {
        // playerRb.velocity = new Vector2(horizontalInput, verticalInput).normalized * 5.0f;
        playerRb.AddForce(new Vector2(horizontalInput, verticalInput).normalized * playerSpeed);
    }

    void UpdatePlayerSize(int valueToAdd)
    {
        if(GameManager.isGameActive)
        {
            playerSize += valueToAdd;
            if(playerSize < 0)
            {
                playerSize = 0;
            }
            else if(playerSize > gameManager.GetMaxPlayerSize())
            {
                playerSize = gameManager.GetMaxPlayerSize();
            }

            ChangePlayerScale();
            gameManager.SetProgressSlider(playerSize);
            gameManager.CheckAndSetProgressLevel(playerSize);

            //if player reach the goal
            if(playerSize == gameManager.GetMaxPlayerSize() && !isGameClear)
            {
                isGameClear = true;
                gameManager.GameOver();
            }
        }
    }

    void ChangePlayerScale()
    {
        float increasedPlayerScale = 1.0f + ((float) playerSize / (float) gameManager.GetMaxPlayerSize() * (float) gameManager.GetMaxIncreasedScale());
        transform.localScale = new Vector3(increasedPlayerScale, increasedPlayerScale, 1.0f);

        ChangeTrailSize(increasedPlayerScale);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Enemy"))
        {
            EnemyBehavior enemyBehavior = collision.gameObject.GetComponent<EnemyBehavior>();
            Enemy enemy = enemyBehavior.GetEnemy();
            //eat enemy if it is stunned
            if(enemyBehavior.IsEnemyStunned())
            {
                Vector2 swallowDirection = (collision.transform.position - transform.position).normalized;
                playerRb.AddForce(swallowDirection * swallowForce, ForceMode2D.Impulse);

                UpdatePlayerSize(enemy.enemySize);

                enemyBehavior.GetSwallowedAnimation();
                spawnManager.DecreaseCurrentEnemies();

                playerAudio.PlayOneShot(swallowSound);
            }
            else
            {
                //both get bounced off
                Rigidbody2D enemyRb = enemyBehavior.GetEnemyRb();
                Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;

                playerRb.AddForce(knockbackDirection * enemy.knockbackForce, ForceMode2D.Impulse);
                enemyRb.AddForce(-knockbackDirection * enemy.knockbackForce, ForceMode2D.Impulse);

                playerAudio.PlayOneShot(bounceSound);
            }
        }   
    }

    private void OnTriggerEnter2D(Collider2D other)
    {     
        if(other.gameObject.CompareTag("Particle"))
        {
            //make effect when hit
            Particle particle = other.gameObject.GetComponent<Particle>();
            particle.OnObjectHit();

            spawnManager.DecreaseCurrentParticles();

            UpdatePlayerSize(gameManager.GetParticleValue());
        }
        else if(other.gameObject.CompareTag("MovePowerup") || other.gameObject.CompareTag("GunPowerup"))
        {
            if(other.gameObject.CompareTag("MovePowerup"))
            {
                BoostMovePlayer();
            }
            else if(other.gameObject.CompareTag("GunPowerup"))
            {
                BoostGunPlayer();               
            }

            Destroy(other.gameObject);
            spawnManager.DecreaseCurrentPowerups();

            playerAudio.PlayOneShot(getPowerupSound);
        }
        else if(other.gameObject.CompareTag("EnemyProjectile"))
        {
            //make effect when hit
            Projectile projectile = other.gameObject.GetComponent<Projectile>();
            projectile.OnObjectHit();

            UpdatePlayerSize(gameManager.GetDamageHitValue());

            playerAudio.PlayOneShot(hurtSound);
        }
    }

    void BoostMovePlayer()
    {
        if(hasBoostMoved)
        {
            StopCoroutine(boostMoveCoroutine);
        }
        else
        {
            playerSpeed += boostMoveSpeed;
        }
        hasBoostMoved = true;
        playerSpriteRenderer.material = movePowerupColorMaterial;
        boostMoveCoroutine = StartCoroutine(BoostMoveDuration());

        TurnOnTrail();
    }

    void BoostGunPlayer()
    {
        if(hasBoostGun)
        {
            StopCoroutine(boostGunCoroutine);
        }
        else
        {
            shootCooldown -= boostGunSpeed;
        }
        hasBoostGun = true;
        gunIndicator.gameObject.SetActive(true);
        boostGunCoroutine = StartCoroutine(BoostGunDuration());
    }  

    IEnumerator ShootCooldown()
    {
        gunCooldownSilder.gameObject.SetActive(true);
        float currentShootCooldown = 0.0f;

        while(currentShootCooldown < shootCooldown)
        {
            currentShootCooldown += Time.deltaTime;
            gunCooldownSilder.value = currentShootCooldown / shootCooldown;

            yield return null;
        }

        gunCooldownSilder.gameObject.SetActive(false);
        hasShot = false;
    }

    IEnumerator BoostMoveDuration()
    {
        yield return new WaitForSeconds(boostDuration);
        hasBoostMoved = false;
        playerSpriteRenderer.material = defaultColorMaterial;
        playerSpeed -= boostMoveSpeed;

        TurnOffTrail();
    }

    IEnumerator BoostGunDuration()
    {
        yield return new WaitForSeconds(boostDuration);
        hasBoostGun = false;
        gunIndicator.gameObject.SetActive(false);
        shootCooldown += boostGunSpeed;        
    }

    void TurnOnTrail()
    {
        trailRenderer.emitting = true;
    }

    void TurnOffTrail()
    {
        trailRenderer.emitting = false;
    }

    void ChangeTrailSize(float playerScale)
    {
        trailRenderer.startWidth = playerScale;
    }

    public void EnablePlayerAudioListener()
    {
        audioListener.enabled = true;
    }
}
