using System.Collections;
using UnityEngine;

public class VillagerSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public VillagerMover villagerPrefab;   // Prefab com Rigidbody2D + Collider + VillagerMover

    [Header("Spawn Settings")]
    [Tooltip("Segundos entre spawns.")]
    public float spawnInterval = 5f;
    [Tooltip("Quantidade m�xima de villagers vivos gerados por este spawner. 0 = sem limite.")]
    public int maxAlive = 0;
    [Tooltip("Quantos villagers gerar imediatamente ao iniciar.")]
    public int initialBurst = 0;

    [Header("Posicionamento")]
    [Tooltip("Raio aleat�rio ao redor do spawner para nascer (evita empilhar).")]
    public float spawnRadius = 0.8f;
    [Tooltip("Camadas a evitar para n�o nascer em cima de paredes/alde�es, etc.")]
    public LayerMask blockingLayers;
    [Tooltip("Raio para checar sobreposi��o antes de instanciar.")]
    public float overlapCheckRadius = 0.35f;
    [Tooltip("N�mero de tentativas de encontrar posi��o livre antes de desistir neste ciclo.")]
    public int maxPositionAttempts = 8;

    [Header("Controle")]
    public bool autoStart = true;          // Come�a a spawnear no Start
    public bool useUnscaledTime = false;   // Ignora Time.timeScale (ex.: pausa do jogo)

    private int aliveCount = 0;
    private Coroutine loop;

    void Start()
    {
        if (autoStart) StartSpawning();
    }

    public void StartSpawning()
    {
        if (loop != null) return;

        // Burst inicial (opcional)
        for (int i = 0; i < initialBurst; i++)
            TrySpawnOnce();

        loop = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (loop != null)
        {
            StopCoroutine(loop);
            loop = null;
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Respeita limite de vivos
            if (maxAlive <= 0 || aliveCount < maxAlive)
                TrySpawnOnce();

            // Espera pelo pr�ximo ciclo
            if (useUnscaledTime)
                yield return WaitForSecondsUnscaled(spawnInterval);
            else
                yield return new WaitForSeconds(spawnInterval);
        }
    }

    void TrySpawnOnce()
    {
        if (villagerPrefab == null) return;

        // Respeita limite de vivos
        if (maxAlive > 0 && aliveCount >= maxAlive) return;

        // Tenta achar uma posi��o livre em at� N tentativas
        Vector2 spawnPos;
        if (!FindFreePosition(out spawnPos))
            return;

        // Instancia
        var v = Instantiate(villagerPrefab, spawnPos, Quaternion.identity);

        // Opcional: assinar evento de morte/disable para decrementar aliveCount
        aliveCount++;
        var lifeTracker = v.gameObject.AddComponent<VillagerLifeHook>();
        lifeTracker.onGone += () => aliveCount--;
    }

    bool FindFreePosition(out Vector2 posOut)
    {
        for (int i = 0; i < maxPositionAttempts; i++)
        {
            Vector2 center = (Vector2)transform.position;
            Vector2 candidate = center + Random.insideUnitCircle * spawnRadius;

            // Checa se j� h� algo ocupando
            Collider2D hit = Physics2D.OverlapCircle(candidate, overlapCheckRadius, blockingLayers);
            if (hit == null)
            {
                posOut = candidate;
                return true;
            }
        }

        posOut = transform.position;
        return false;
    }

    // Espera em tempo n�o escalado (ignora pause)
    IEnumerator WaitForSecondsUnscaled(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end)
            yield return null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, overlapCheckRadius);
    }

    /// <summary>
    /// Componente auxiliar para detectar quando o villager for destru�do/desativado
    /// e atualizar a contagem viva do spawner.
    /// </summary>
    private class VillagerLifeHook : MonoBehaviour
    {
        public System.Action onGone;
        void OnDisable() { onGone?.Invoke(); onGone = null; }
        void OnDestroy() { onGone?.Invoke(); onGone = null; }
    }
}
