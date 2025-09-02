using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int enemyCount = 5;
    public int spawnAreaSize = 10;

    void Awake()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("An enemy prefab has not been assigned in the inspector");
        }
    }

    void Start()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 randomPos = new Vector3(Random.Range(-spawnAreaSize, spawnAreaSize), 0, Random.Range(-spawnAreaSize, spawnAreaSize));
            Instantiate(enemyPrefab, randomPos, Quaternion.identity);
        }
    }
}