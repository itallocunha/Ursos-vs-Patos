using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyWaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public EnemyAI enemyPrefab;
        public int count = 5;
        public float spawnInterval = 0.75f;
        public float startDelay = 2f;
    }

    [Header("Spawn Points")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Tooltip("Caminho para os inimigos até a base")]
    public EnemyPath2D pathForThisSpawner;

    [Header("Configurações de Ondas")]
    public List<Wave> waves = new List<Wave>();
    public float timeBetweenWaves = 3f;
    public bool loopWaves = false;
    public bool autoStart = true;
    public bool randomizeSpawnPointOrder = true;

    bool isRunning;
    int _lastIndex = 0;

    void Start()
    {
        if (autoStart) StartWaves();
    }

    public void StartWaves()
    {
        if (isRunning) return;
        if (waves.Count == 0 || spawnPoints.Count == 0)
        {
            Debug.LogWarning("Enemy");
            return;
        }
        isRunning = true;
        StartCoroutine(WaveRoutine());
    }

    IEnumerator WaveRoutine()
    {
        do
        {
            for (int i = 0; i < waves.Count; i++)
            {
                var w = waves[i];

                if (w.startDelay > 0) yield return new WaitForSeconds(w.startDelay);

                for (int n = 0; n < w.count; n++)
                {
                    if (BaseHealth.Instance != null && BaseHealth.Instance.currentHealth <= 0)
                        yield break;

                    SpawnOne(w.enemyPrefab);
                    if (w.spawnInterval > 0) yield return new WaitForSeconds(w.spawnInterval);
                }

                if (i < waves.Count - 1 && timeBetweenWaves > 0)
                    yield return new WaitForSeconds(timeBetweenWaves);
            }
        }
        while (loopWaves && BaseHealth.Instance != null && BaseHealth.Instance.currentHealth > 0);

        isRunning = false;
    }

    void SpawnOne(EnemyAI enemyPrefab)
    {
        if (!enemyPrefab || spawnPoints.Count == 0) return;
        Transform sp = PickSpawnPoint();
        var enemy = Instantiate(enemyPrefab, sp.position, Quaternion.identity);

        // Passa a trilha deste spawner para o inimigo
        if (pathForThisSpawner != null)
            enemy.SetPath(pathForThisSpawner);
    }

    Transform PickSpawnPoint()
    {
        if (randomizeSpawnPointOrder)
        {
            int idx = Random.Range(0, spawnPoints.Count);
            return spawnPoints[idx];
        }
        if (_lastIndex >= spawnPoints.Count) _lastIndex = 0;
        return spawnPoints[_lastIndex++];
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        foreach (var t in spawnPoints)
        {
            if (!t) continue;
            Gizmos.DrawSphere(t.position, 0.1f);
            Gizmos.DrawWireSphere(t.position, 0.25f);
        }
    }
}
