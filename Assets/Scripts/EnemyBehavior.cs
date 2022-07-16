using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [Header("Enemy Scriptable Object")]
    [SerializeField] private Enemy enemy;

    [Header("Player Reference")]
    [SerializeField] private GameObject player;

    [Header("Gun Barrel Reference")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileBarrels;

    [Header("Swallow Particle")]
    [SerializeField] private ParticleSystem swallowParticle;

    //component
    [Header("Enemy Component")]

    [SerializeField] private Rigidbody2D enemyRb;
    [SerializeField] private SpriteRenderer enemyRender;
    [SerializeField] private Animator enemyAnim;
    [SerializeField] private AudioSource enemyAudio;

    //sfx
    [Header("Sound Effect")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip stunSound;
    [SerializeField] private AudioClip bounceSound;
    
    [Header("Variable")]
    [SerializeField] private bool hasMoved = false;
    [SerializeField] private bool hasShot = false;
    [SerializeField] private bool isStunned = false;
    [SerializeField] private bool isIdle = true;
    [SerializeField] [Range(0, 5)] private float moveCooldown = 2.0f;
    [SerializeField] [Range(1, 10)] private float shootCooldown = 2.0f;
    [SerializeField] [Range(1, 10)] private int stunCooldown = 3;
    [SerializeField] [Range(0, 5)] private int idleCooldown = 1;
    private Coroutine stunCoroutine;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(IdleCooldown());
    }

    void FixedUpdate()
    {
        if((!isStunned && !isIdle && GameManager.isGameActive) || GameManager.isInMainMenu)
        {
            MoveEnemy();
            ActEnemy();
        }
    }

    void MoveEnemy()
    {
        if(!hasMoved)
        {
            hasMoved = true;

            
            //enemyRb.velocity = GetMoveDirection() * enemy.enemySpeed;
            enemyRb.AddForce(GetMoveDirection() * enemy.enemySpeed, ForceMode2D.Impulse);
            StartCoroutine(MoveCooldown());
        }
    }

    void ActEnemy()
    {
        if(enemy.isAggressive)
        {
            //if enemy detect player based on their detect range
            if(!hasShot && enemy.detectRange > Vector2.Distance(player.transform.position, transform.position))
            {
                hasShot = true;
                
                //fire projectiles
                ShootProjectiles();
                StartCoroutine(ShootCooldown());
            }
        }
    }

    void ShootProjectiles()
    {
        for(int i = 0; i < projectileBarrels.childCount; i++)
        {
            Transform selectedProjectileBarrel = projectileBarrels.GetChild(i);
            Instantiate(projectilePrefab, selectedProjectileBarrel.position, selectedProjectileBarrel.rotation);
        }

        enemyAudio.PlayOneShot(shootSound);
    }

    Vector2 GetMoveDirection()
    {
        Vector2 moveDirection;

        //if enemy detect player based on their detect range
        if(enemy.detectRange > Vector2.Distance(player.transform.position, transform.position))
        {
            //if enemy is aggressive
            if(enemy.isAggressive)
            {
                //move toward player
                moveDirection = (player.transform.position - transform.position).normalized;
            }
            else
            {
                //move away from player
                moveDirection = (transform.position - player.transform.position).normalized;
            }
            //Debug.Log("Detect Move: " + moveDirection);
        }
        else
        {
            //move randomly
            float directionX = Random.Range(-1.0f,1.0f);
            float directionY = Random.Range(-1.0f,1.0f);
            moveDirection = new Vector2(directionX, directionY);
        }
        return moveDirection;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {     
        if(other.gameObject.CompareTag("PlayerProjectile"))
        {
            //make effect when hit
            Projectile projectile = other.gameObject.GetComponent<Projectile>();
            projectile.OnObjectHit();

            //stun enemy
            if(isStunned)
            {
                StopCoroutine(stunCoroutine);
            }
            enemyRender.color = Color.grey;
            isStunned = true;
            stunCoroutine = StartCoroutine(StunCooldown());

            enemyAudio.PlayOneShot(stunSound);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {     
        if(collision.gameObject.CompareTag("Enemy"))
        {
            EnemyBehavior enemyBehavior = collision.gameObject.GetComponent<EnemyBehavior>();

            //both get bounced off
            Rigidbody2D otherEnemyRb = enemyBehavior.GetEnemyRb();
            Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;

            enemyRb.AddForce(knockbackDirection * enemy.knockbackForce, ForceMode2D.Impulse);
            otherEnemyRb.AddForce(-knockbackDirection * enemy.knockbackForce, ForceMode2D.Impulse);

            enemyAudio.PlayOneShot(bounceSound);
        }
    }

    public Enemy GetEnemy()
    {
        return enemy;
    }

    public void SetPlayer(GameObject player)
    {
        this.player = player;
    }

    public bool IsEnemyStunned()
    {
        return isStunned;
    }

    public void GetSwallowedAnimation()
    {
        enemyAnim.SetTrigger("death_swallow");
    }

    public Rigidbody2D GetEnemyRb()
    {
        return enemyRb;
    }

    public void DestorySelf()
    {
        Destroy(gameObject);
    }

    public void ShowSwallowEffect()
    {
        swallowParticle.gameObject.SetActive(true);
    }

    IEnumerator MoveCooldown()
    {
        //Debug.Log("Enemy " + gameObject.name + " dist: " + Vector2.Distance(player.transform.position, transform.position));
        yield return new WaitForSeconds(moveCooldown);
        hasMoved = false;
    }

    IEnumerator ShootCooldown()
    {
        yield return new WaitForSeconds(shootCooldown);
        hasShot = false;
    }

    IEnumerator StunCooldown()
    {
        yield return new WaitForSeconds(stunCooldown);
        isStunned = false;
        enemyRender.color = Color.white;
    }

    IEnumerator IdleCooldown()
    {
        yield return new WaitForSeconds(idleCooldown);
        isIdle = false;
    }
}
