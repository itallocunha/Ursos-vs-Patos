using UnityEngine;
using System.Collections;

public enum ResourceType { Wood, Stone }

public class ResourceNode : MonoBehaviour
{
    public ResourceType type = ResourceType.Wood;

    [Header("Tempo por colheita")]
    public float minDuration = 2f;
    public float maxDuration = 3f;

    [Header("Interação")]
    public float interactionRadius = 0.6f;

    [Header("Efeitos")]
    public GameObject depletedEffect;
    public float shakeIntensity = 0.05f;
    public float shakeSpeed = 25f;

    [Header("Respawn")]
    public bool respawn = true;
    public float respawnMin = 5f;  
    public float respawnMax = 10f;

    // estado interno
    private bool isBeingHarvested = false;
    private bool consumed = false;
    private bool destroying = false;          // aqui significa “sumindo” p/ respawn
    private bool applicationQuitting = false;
    private int activeHarvesters = 0;

    private Coroutine shakeRoutine;
    private Vector3 originalPos;

    // cache p/ desligar/ligar rapido
    SpriteRenderer[] renderers;
    Collider2D[] colliders;

    public bool IsDepleted => consumed || destroying;

    void Awake()
    {
        originalPos = transform.localPosition;
        renderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        colliders = GetComponentsInChildren<Collider2D>(includeInactive: true);
    }

    void OnApplicationQuit() { applicationQuitting = true; }
    void OnDisable() { SafeStopShaking(false); }
    void OnDestroy() { SafeStopShaking(false); }

    public float GetNextHarvestDuration()
    {
        if (maxDuration < minDuration) (minDuration, maxDuration) = (maxDuration, minDuration);
        return Random.Range(minDuration, maxDuration);
    }

    public bool TryConsume()
    {
        if (consumed || destroying) return false;
        consumed = true;
        return true;
    }

    public void RegisterHarvester()
    {
        activeHarvesters++;
        if (!isBeingHarvested) { isBeingHarvested = true; StartShaking(); }
    }

    public void UnregisterHarvester()
    {
        activeHarvesters = Mathf.Max(0, activeHarvesters - 1);
        if (activeHarvesters == 0)
        {
            isBeingHarvested = false;
            SafeStopShaking(true);
        }
    }

    // continua com o mesmo nome pra nao quebrar quem chama
    public void DepleteAndDestroy()
    {
        if (destroying) return;
        if (depletedEffect) Instantiate(depletedEffect, transform.position, Quaternion.identity);

        if (respawn && !applicationQuitting)
        {
            StartCoroutine(RespawnRoutine());
        }
        else
        {
            Destroy(gameObject); // se quiser desativar respawn
        }
    }

    IEnumerator RespawnRoutine()
    {
        destroying = true;     
        SafeStopShaking(false);

        // esconde o recurso
        SetVisible(false);
        SetColliders(false);

        // espera entre 5 e 10 (ou os valores do inspetor)
        float wait = Random.Range(Mathf.Min(respawnMin, respawnMax), Mathf.Max(respawnMin, respawnMax));
        yield return new WaitForSeconds(wait);

        // “renasce” no mesmo ponto
        transform.localPosition = originalPos; 
        consumed = false;
        isBeingHarvested = false;
        activeHarvesters = 0;
        destroying = false;

        SetColliders(true);
        SetVisible(true);
    }

    void SetVisible(bool v)
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        foreach (var r in renderers) if (r != null) r.enabled = v;
    }

    void SetColliders(bool v)
    {
        if (colliders == null || colliders.Length == 0)
            colliders = GetComponentsInChildren<Collider2D>(includeInactive: true);
        foreach (var c in colliders) if (c != null) c.enabled = v;
    }

    void StartShaking()
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeLoop());
    }

    void SafeStopShaking(bool restorePosition)
    {
        if (shakeRoutine != null) { StopCoroutine(shakeRoutine); shakeRoutine = null; }
        if (!restorePosition || applicationQuitting || this == null) return;
        if (transform != null) transform.localPosition = originalPos;
    }

    IEnumerator ShakeLoop()
    {
        float t = 0f;
        while (isBeingHarvested && !destroying && !applicationQuitting)
        {
            t += Time.deltaTime * shakeSpeed;
            Vector2 offset = new Vector2(
                Mathf.PerlinNoise(t, 0f) - 0.5f,
                Mathf.PerlinNoise(0f, t) - 0.5f
            ) * (shakeIntensity * 2f);

            if (this == null) yield break;
            transform.localPosition = originalPos + (Vector3)offset;
            yield return null;
        }

        if (!destroying && !applicationQuitting && this != null)
        {
            float elapsed = 0f; Vector3 start = transform.localPosition;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                if (this == null) yield break;
                transform.localPosition = Vector3.Lerp(start, originalPos, elapsed / 0.2f);
                yield return null;
            }
            transform.localPosition = originalPos;
        }

        shakeRoutine = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
