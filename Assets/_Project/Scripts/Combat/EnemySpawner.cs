using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Setup")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject enemyPrefab;

    private GameObject currentEnemy;

    private void Start()
    {
        SpawnEnemy();
    }

    public void SpawnEnemy()
    {
        if (currentEnemy != null)
            Destroy(currentEnemy);

        currentEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}