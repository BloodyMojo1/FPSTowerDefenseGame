using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FlockingSystem;

public class WaveSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject spawnLocation;
    [SerializeField] private List<Enemy> enemies = new List<Enemy>();
    [SerializeField] private FlockingManager flockingManager;

    [Header("Attributes")]
    [SerializeField] private int baseEnemies;
    [SerializeField] private int minEnemiesPerSpawn;
    [SerializeField] private int maxEnemiesPerSpawn;

    [SerializeField] private float enemiesPerSecond;
    [SerializeField] private float timeBetweenWaves;
    [SerializeField] private float difficultyScalingFator;

    [SerializeField] private Vector3 spawnAreaSize;
    [SerializeField] private float spawnCollisionCheckRadius;
    [SerializeField] private float spawnAreaGap;

    [Header("Events")]
    public static UnityEvent onEnemyDestory = new UnityEvent();

    private List<GameObject> enemiesToSpawn = new List<GameObject>();

    private int currentWave = 1;
    private int waveValue;
    private float timeSinceLastSpawn;
    private int enemiesAlive;
    private bool isSpawning;

    [System.Serializable]
    public class Enemy
    {
        public GameObject enemyPrefab;
        public int cost;
        public int droptChance;
    }
    public Enemy droppedEnemy;

    [SerializeField] private LayerMask enemyLayer;

    private void Awake()
    {
        onEnemyDestory.AddListener(EnemyDestoryed);
        flockingManager = GameObject.FindWithTag("FlockingManager").GetComponent<FlockingManager>();
    }

    private void Start()
    {
        StartCoroutine(StartWave());
    }

    private void Update()
    {
        if (!isSpawning) return;

        timeSinceLastSpawn += Time.deltaTime;

        if (timeSinceLastSpawn >= (1f / enemiesPerSecond) && enemiesToSpawn.Count > 0)
        {
            int maxEnemySpawn = Mathf.RoundToInt(maxEnemiesPerSpawn * Mathf.Pow(currentWave, difficultyScalingFator));

            int randomNumber = Random.Range(minEnemiesPerSpawn, maxEnemySpawn);
            if (randomNumber > enemiesToSpawn.Count) randomNumber = enemiesToSpawn.Count;
            for (int i = 0; i < randomNumber; i++)
            {
                SpawnEnemy();
                enemiesToSpawn.RemoveAt(0);
                enemiesAlive++;
                timeSinceLastSpawn = 0f;
            }
            //Generate random number between 2-Random/MaxValue
            //Loop through this code untill loop has reached max value
            //When Spawning Mutlple Enemies make sure they dont spawn inside each other
        }

        if (enemiesAlive == 0 && enemiesToSpawn.Count == 0)
        {
            EndWave();
        }
    }

    private void EnemyDestoryed()
    {
        enemiesAlive--;
    }

    private IEnumerator StartWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        isSpawning = true;
        waveValue = WaveValueAmt();
        GenerateEnemies();
    }

    private void EndWave()
    {
        isSpawning = false;
        timeSinceLastSpawn = 0f;
        currentWave++;
        StartCoroutine(StartWave());
    }

    Enemy GetDroppedItem()
    {
        int randomNumber = Random.Range(1, 101);
        List<Enemy> possibleItems = new List<Enemy>();

        foreach (Enemy item in enemies)
        {
            if (randomNumber <= item.droptChance)
            {
                possibleItems.Add(item);
            }
        }
        if (possibleItems.Count > 0)
        {
            Enemy droppedItem = possibleItems[Random.Range(0, possibleItems.Count)];
            return droppedItem;
        }
        return null;
    }

    private void GenerateEnemies()
    {
        List<GameObject> generatedEnemies = new List<GameObject>();
        while (waveValue > 0)
        {
            //int randomEnemyId = Random.Range(0, enemies.Count);
            //int randomEnemyCost = enemies[randomEnemyId].cost;
            droppedEnemy = GetDroppedItem();


            if (droppedEnemy != null)
            {
                if (waveValue - droppedEnemy.cost >= 0)
                {
                    generatedEnemies.Add(droppedEnemy.enemyPrefab);
                    waveValue -= droppedEnemy.cost;
                }
                else if (waveValue <= 0)
                {
                    break;
                }
            }

        }
        enemiesToSpawn.Clear();
        enemiesToSpawn = generatedEnemies;
    }

    Vector3 spawnPoint;

    private void SpawnEnemy()
    {
        bool enemySpawned = false;
        //Random Spawning System Here
        while (enemySpawned == false)
        {
            spawnPoint = spawnLocation.transform.position + new Vector3(Random.Range(spawnAreaSize.x, -spawnAreaSize.x) / spawnAreaGap, -spawnAreaSize.y / spawnAreaGap,
            Random.Range(spawnAreaSize.z, -spawnAreaSize.z) / spawnAreaGap);

            if (!Physics.CheckSphere(spawnPoint, spawnCollisionCheckRadius, enemyLayer))
            {
                Instantiate(enemiesToSpawn[0], spawnPoint, Quaternion.identity);
                enemySpawned = true;

                if (flockingManager != null && flockingManager.flocks.Count > 0)
                {
                    Flock lastFlock = flockingManager.flocks[flockingManager.flocks.Count - 1];
                    flockingManager.UpdateWaypointsForFlock(lastFlock);
                }

                // After spawning, initialize flocks if needed
                flockingManager.InitializeFlocks();

                break;
            }
        }
    }

    private int WaveValueAmt()
    {
        return Mathf.RoundToInt(baseEnemies * Mathf.Pow(currentWave, difficultyScalingFator));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.25f);
        Gizmos.DrawCube(spawnLocation.transform.position, spawnAreaSize);
        Gizmos.DrawWireSphere(spawnPoint, spawnCollisionCheckRadius);
    }
}

