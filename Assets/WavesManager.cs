using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Internal;

public class WavesManager : MonoBehaviour
{
    [Header("Player Reference")]
    public Transform player;

    [Header("Enemy Prefabs")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();
    public float[] enemyWeights;

    [Header("Spawn Settings")]
    public float spawnRadius = 20f;
    public float minSpawnDistance = 15f;
    public int enemiesPerWave = 5;
    public float timeBetweenWaves = 5f;
    public LayerMask groundLayer;

    [Header("Wave Progression")]
    public float enemyIncreasePerWave = 2f;
    public float difficultyMultiplier = 1.1f;
    public int maxEnemiesPerWave = 50;

    [Header("Current Wave Info")]
    public int currentWave = 0;
    public int enemiesAlive = 0;
    public bool waveInProgress = false;

    private List<GameObject> activeEnemies = new List<GameObject>();
    
    
    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            Debug.LogError("Player Has not been assigned dawg");
        }
        StartCoroutine(WaveSystem());
    }

    // Update is called once per frame
    void Update()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);
        enemiesAlive = activeEnemies.Count;
    }
    private IEnumerator WaveSystem()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenWaves);

            currentWave++;
            waveInProgress = true;

            int enemiesToSpawn = CalculateEnemiesForWave();

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(0.5f);
            }
            while (enemiesAlive > 0)
            {
                yield return new WaitForSeconds(0.5f);
            }
            waveInProgress = false;

        }
    }
    private int CalculateEnemiesForWave()
    {
        int enemies = Mathf.RoundToInt(enemiesPerWave + (currentWave - 1) * enemyIncreasePerWave);
        return Mathf.Min(enemies, maxEnemiesPerWave);
    }
    private void SpawnEnemy()
    {
        if (player != null && enemyPrefabs.Count > 0)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            GameObject enemyPrefab = SelectRandomEnemy();

            if (enemyPrefab != null)
            {
                GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                activeEnemies.Add(enemy);
            }
        }
    }
    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 spawnPos = Vector3.zero;
        int maxAttempts = 30;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minSpawnDistance, spawnRadius);

            Vector3 offset = new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);

            spawnPos = player.position + offset;

            RaycastHit hit;
            if (Physics.Raycast(spawnPos + Vector3.up * 50f, Vector3.down, out hit, 100f, groundLayer))
            {
                spawnPos = hit.point + Vector3.up * 0.5f;
                return spawnPos;
            }
            attempts++;
        }
        return new Vector3(spawnPos.x, player.position.y, spawnPos.z);
    }
    private GameObject SelectRandomEnemy()
    {

        if (enemyPrefabs.Count == 0) return null;
        if (enemyPrefabs.Count == 1) return enemyPrefabs[0];

        float totalWeight = 0f;
        for (int i = 0; i < Mathf.Min(enemyPrefabs.Count, enemyWeights.Length); i++)
        {
            totalWeight += enemyWeights[i];
        }
        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        for (int i = 0; i < Mathf.Min(enemyPrefabs.Count, enemyWeights.Length); i++)
        {
            cumulativeWeight += enemyWeights[i];
            if (randomValue <= cumulativeWeight)
            {
                return enemyPrefabs[i];
            }
        }
        return enemyPrefabs[0]; //Just to get the compiler to shut up
    }
    void OnDrawGizmosSelected()
    {
        if(player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, minSpawnDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, spawnRadius);
        }
    }
}
