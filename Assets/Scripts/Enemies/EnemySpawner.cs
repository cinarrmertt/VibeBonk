using UnityEngine;
using UnityEngine.AI; 
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private GameObject enemyPrefab; 
    [SerializeField] private Transform playerTransform; 
    
    [Header("Dinamik Spawn Ayarları")]
    [SerializeField] private float initialSpawnInterval = 3f; 
    [SerializeField] private float minSpawnInterval = 0.5f;   
    [SerializeField] private float reductionPerLevel = 0.2f;  
    
    [Header("Spawn Alanı")]
    [SerializeField] private float minSpawnDistance = 10f; 
    [SerializeField] private float maxSpawnDistance = 20f; 
    [SerializeField] private int maxEnemies = 50; 

    private Player playerScript; 

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) 
            {
                playerTransform = player.transform;
                playerScript = player.GetComponent<Player>();
            }
        }
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float currentInterval = initialSpawnInterval;

            if (playerScript != null)
            {
                float timeReduction = (playerScript.CurrentLevel - 1) * reductionPerLevel;
                currentInterval = Mathf.Max(initialSpawnInterval - timeReduction, minSpawnInterval);
            }

            // Sahnedeki aktif düşmanları say (GameObject.Find performanslı değildir ama basitlik için kullanılır)
            // Daha optimize bir yol: ObjectPoolManager'dan aktif sayısını istemektir.
            // Şimdilik basit tutalım:
            int activeEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

            if (activeEnemyCount < maxEnemies && playerTransform != null)
            {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(currentInterval);
        }
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPosition = GetRandomPointAroundPlayer();
        NavMeshHit hit;
        
        if (NavMesh.SamplePosition(spawnPosition, out hit, 2.0f, NavMesh.AllAreas))
        {
            // --- OBJECT POOL KULLANIMI ---
            // Instantiate yerine Manager'dan istiyoruz.
            
            GameObject newEnemy = ObjectPoolManager.Instance.GetPooledObject(enemyPrefab, hit.position, Quaternion.identity);
            
            // Düşmanı başlat (Resetle)
            Enemy enemyScript = newEnemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                // Havuzdan çıkan düşman NavMesh üzerinde değilse Warp gerekebilir
                NavMeshAgent agent = enemyScript.GetComponent<NavMeshAgent>();
                if (agent != null) agent.Warp(hit.position);
                
                enemyScript.Spawn(); // Enemy.cs içindeki Spawn (Reset) metodunu çağır
            }
        }
    }

    private Vector3 GetRandomPointAroundPlayer()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
        Vector3 offset = new Vector3(randomCircle.x, 0, randomCircle.y) * randomDistance;
        return playerTransform.position + offset;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerTransform.position, minSpawnDistance);
            Gizmos.DrawWireSphere(playerTransform.position, maxSpawnDistance);
        }
    }
}