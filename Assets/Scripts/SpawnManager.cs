using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Script Reference")]
    [SerializeField] private GameManager gameManager;

    [Header("Game Object Reference")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject[] enemyPrefabList;
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private GameObject[] powerupPrefabList;
    [SerializeField] private GameObject menuSpawner;

    [Header("Spawn Boundary")]
    [SerializeField] private float xSpawnBound = 17.0f;
    [SerializeField] private float ySpawnBound = 17.0f;
    
    [Header("Current Game object In-Game")]
    [SerializeField] private int currentEnemies = 0;
    [SerializeField] private int currentParticles = 0;
    [SerializeField] private int currentPowerups = 0;

    [Header("Spawn Borundary in Main Menu")]
    [SerializeField] private float xSpawnMenuBound = 7.0f;
    [SerializeField] private float ySpawnMenuBound = 4.0f;

    void SpawnEnemy()
    {
        float randPosX = Random.Range(-xSpawnBound, xSpawnBound);
        float randPosY = Random.Range(-ySpawnBound, ySpawnBound);
        Vector2 spawnPos = new Vector2(randPosX, randPosY);

        
        float[] enemyChance = gameManager.GetEnemyChance();
        GameObject enemyPrefab = null;
        float randEnemyProb = Random.Range(0.0f, 100.0f);
        float cumulativeProb = 0.0f;

        for(int i = 0; i < enemyChance.Length; i++)
        {
            cumulativeProb += enemyChance[i];
            if(randEnemyProb < cumulativeProb)
            {
                enemyPrefab = enemyPrefabList[i];
                break;
            }
        }

        //int randIndex = Random.Range(0, enemyPrefabList.Length);

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, enemyPrefab.gameObject.transform.rotation);
        EnemyBehavior newEnemyBehavior = newEnemy.GetComponent<EnemyBehavior>();
        newEnemyBehavior.SetPlayer(player);
    }

    void SpawnParticle()
    {
        float randPosX = Random.Range(-xSpawnBound, xSpawnBound);
        float randPosY = Random.Range(-ySpawnBound, ySpawnBound);
        Vector2 spawnPos = new Vector2(randPosX, randPosY);

        Instantiate(particlePrefab, spawnPos, particlePrefab.gameObject.transform.rotation);
    }

    void SpawnPowerup()
    {
        float randPosX = Random.Range(-xSpawnBound, xSpawnBound);
        float randPosY = Random.Range(-ySpawnBound, ySpawnBound);
        Vector2 spawnPos = new Vector2(randPosX, randPosY);

        int randIndex = Random.Range(0, powerupPrefabList.Length);

        Instantiate(powerupPrefabList[randIndex], spawnPos, powerupPrefabList[randIndex].gameObject.transform.rotation);
    }

    public void DecreaseCurrentEnemies()
    {
        currentEnemies--;
    }

    public void DecreaseCurrentParticles()
    {
        currentParticles--;
    }

    public void DecreaseCurrentPowerups()
    {
        currentPowerups--;
    }

    public IEnumerator SpawnObjects()
    {
        while(GameManager.isGameActive)
        {
            //spawn enemy
            int numOfSpawnEnemies = gameManager.GetMaxEnemies() - currentEnemies;
            for(int i = 0 ; i < numOfSpawnEnemies; i++)
            {
                SpawnEnemy();
                currentEnemies++;
            }

            //spawn particle
            int numOfSpawnParticles = gameManager.GetMaxParticles() - currentParticles;
            for(int i = 0 ; i < numOfSpawnParticles; i++)
            {
                SpawnParticle();
                currentParticles++;
            }

            //spawn powerup
            int numOfSpawnPowerups = gameManager.GetMaxPowerups() - currentPowerups;
            for(int i = 0 ; i < numOfSpawnPowerups; i++)
            {
                SpawnPowerup();
                currentPowerups++;
            }

            gameManager.PlaySpawnSound();
            yield return new WaitForSeconds(gameManager.GetDelaySpawn());
        }
    }

    public void SpawnObjectsInMainMenu()
    {
        for(int i = 0; i < enemyPrefabList.Length; i++)
        {
            int enemyCount = Random.Range(1, gameManager.GetSameEnemySpawnCount() + 1);
            for(int j = 0; j < enemyCount; j++)
            {
                float randPosX = Random.Range(-xSpawnMenuBound + menuSpawner.transform.position.x, xSpawnMenuBound + menuSpawner.transform.position.x);
                float randPosY = Random.Range(-ySpawnMenuBound + menuSpawner.transform.position.y, ySpawnMenuBound + menuSpawner.transform.position.y);
                Vector2 spawnPos = new Vector2(randPosX, randPosY);

                GameObject newEnemy = Instantiate(enemyPrefabList[i], spawnPos, enemyPrefabList[i].gameObject.transform.rotation);
                EnemyBehavior newEnemyBehavior = newEnemy.GetComponent<EnemyBehavior>();
                newEnemyBehavior.SetPlayer(player);
                newEnemy.transform.SetParent(menuSpawner.transform, true);
            }
        }
    }

    public void DeleteObjectsInMainMenu()
    {
        Destroy(menuSpawner);
    }
}
