using System;
using UnityEngine;

[Serializable]
public class EnemySpawnEntry
{
    public EnemyApproach prefab;
    public int weight = 1;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Setup")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private CombatDirector combatDirector;
    [SerializeField] private SpawnDirector spawnDirector;
    [SerializeField] private EnemySpawnEntry[] enemyPool;

    private EnemyApproach currentEnemy;
    private int lastSpawnIndex = -1;
    private int sameSpawnStreak = 0;
    private const int MaxSameSpawnStreak = 2;

    private void Awake()
    {
        if (combatDirector == null)
            combatDirector = FindFirstObjectByType<CombatDirector>();

        if (spawnDirector == null)
            spawnDirector = FindFirstObjectByType<SpawnDirector>();
    }

    private void Start()
    {
        SpawnEnemy();
    }

    public void SpawnEnemy()
    {
        ApplyDirectorWeights();

        int selectedIndex = GetRandomEnemyIndex();
        if (selectedIndex < 0)
        {
            Debug.LogError("[EnemySpawner] Spawn edilecek uygun enemy bulunamadi!");
            return;
        }

        EnemyApproach prefabToSpawn = enemyPool[selectedIndex].prefab;

        if (currentEnemy != null)
            Destroy(currentEnemy.gameObject, 0.01f);

        currentEnemy = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);

        if (selectedIndex == lastSpawnIndex)
            sameSpawnStreak++;
        else
            sameSpawnStreak = 1;

        lastSpawnIndex = selectedIndex;

        if (combatDirector != null)
            combatDirector.SetCurrentEnemy(currentEnemy);
    }

    public void OnEnemyKilledImmediate()
    {
        spawnDirector?.RegisterKill();
        SpawnEnemy();
    }

    private void ApplyDirectorWeights()
    {
        if (spawnDirector == null) return;
        if (enemyPool == null) return;

        for (int i = 0; i < enemyPool.Length; i++)
        {
            if (enemyPool[i] == null) continue;
            if (enemyPool[i].prefab == null) continue;

            string enemyName = enemyPool[i].prefab.name;

            if (enemyName.Contains("Common"))
                enemyPool[i].weight = spawnDirector.CommonWeight;
            else if (enemyName.Contains("Fast"))
                enemyPool[i].weight = spawnDirector.FastWeight;
            else if (enemyName.Contains("Elite"))
                enemyPool[i].weight = spawnDirector.EliteWeight;
        }
    }

    private int GetRandomEnemyIndex()
    {
        if (enemyPool == null || enemyPool.Length == 0)
            return -1;

        int totalWeight = 0;

        for (int i = 0; i < enemyPool.Length; i++)
        {
            if (enemyPool[i] == null) continue;
            if (enemyPool[i].prefab == null) continue;
            if (enemyPool[i].weight <= 0) continue;

            bool blockedByStreak =
                enemyPool.Length > 1 &&
                i == lastSpawnIndex &&
                sameSpawnStreak >= MaxSameSpawnStreak;

            if (blockedByStreak) continue;

            totalWeight += enemyPool[i].weight;
        }

        if (totalWeight <= 0)
            return -1;

        int roll = UnityEngine.Random.Range(0, totalWeight);
        int current = 0;

        for (int i = 0; i < enemyPool.Length; i++)
        {
            if (enemyPool[i] == null) continue;
            if (enemyPool[i].prefab == null) continue;
            if (enemyPool[i].weight <= 0) continue;

            bool blockedByStreak =
                enemyPool.Length > 1 &&
                i == lastSpawnIndex &&
                sameSpawnStreak >= MaxSameSpawnStreak;

            if (blockedByStreak) continue;

            current += enemyPool[i].weight;

            if (roll < current)
                return i;
        }

        return -1;
    }
}