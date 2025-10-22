using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class VillagerMover : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 0.1f;
    public float maxSpeed = 5f;

    [Header("Separation (anti-amontoado)")]
    public bool useSeparation = true;
    public float separationRadius = 0.8f;
    public float separationStrength = 2.5f;
    public LayerMask villagerLayer;

    [Header("Vida")]
    public int maxHealth = 3;
    public int currentHealth;
    public GameObject deathEffect; // opcional

    [Header("Harvest")]
    public float harvestRingRadius = 0.7f;
    public int amountPerCycle = 1;

    private Rigidbody2D rb;
    private Vector2 currentTarget;
    private bool hasTarget;

    private ResourceNode targetNode;
    private Coroutine harvestRoutine;
    private bool isHarvesting = false;
    public bool IsHarvesting => isHarvesting;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentTarget = transform.position;
        hasTarget = false;
        currentHealth = maxHealth;
    }

    void OnEnable()
    {
        currentHealth = maxHealth;
        if (VillageController.Instance != null)
            VillageController.Instance.Register(this);
    }

    void OnDisable()
    {
        if (VillageController.Instance != null)
            VillageController.Instance.Unregister(this);

        if (harvestRoutine != null) { StopCoroutine(harvestRoutine); harvestRoutine = null; }
        targetNode = null;
        isHarvesting = false;
    }

    // ===== VIDA =====
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
        else StartCoroutine(HitFlash());
    }

    void Die()
    {
        if (deathEffect) Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    IEnumerator HitFlash()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = original;
        }
    }

    // ===== MOVIMENTO =====
    public void SetTarget(Vector2 worldPos)
    {
        // libera qualquer alvo de recurso anterior (se ainda não iniciou)
        targetNode = null;
        currentTarget = worldPos;
        hasTarget = true;
    }

    public void SetHarvestTarget(ResourceNode node, Vector2 slotPosition)
    {
        targetNode = node;
        currentTarget = slotPosition;
        hasTarget = true;
    }

    void FixedUpdate()
    {
        if (!hasTarget)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 pos = rb.position;
        Vector2 toTarget = currentTarget - pos;
        float dist = toTarget.magnitude;

        Vector2 desiredVel = Vector2.zero;

        float stopDist = stoppingDistance;
        if (targetNode != null) stopDist = Mathf.Max(stoppingDistance, targetNode.interactionRadius);

        if (dist > stopDist)
        {
            desiredVel = toTarget.normalized * moveSpeed;

            if (useSeparation)
            {
                Collider2D[] neighbors = Physics2D.OverlapCircleAll(pos, separationRadius, villagerLayer);
                Vector2 separationForce = Vector2.zero; int count = 0;
                foreach (var n in neighbors)
                {
                    if (n.attachedRigidbody == null || n.attachedRigidbody == rb) continue;
                    Vector2 away = (pos - (Vector2)n.attachedRigidbody.position);
                    float d = away.magnitude;
                    if (d > 0.0001f) { separationForce += away.normalized / Mathf.Max(d, 0.1f); count++; }
                }
                if (count > 0) desiredVel += separationForce.normalized * separationStrength;
            }
        }

        if (desiredVel.magnitude > maxSpeed) desiredVel = desiredVel.normalized * maxSpeed;
        rb.linearVelocity = desiredVel;

        // chegou
        if (dist <= stopDist)
        {
            rb.linearVelocity = Vector2.zero;
            hasTarget = false;

            // se encostou num recurso e não está colhendo ainda, colhe 1x
            if (targetNode != null && !isHarvesting && !targetNode.IsDepleted)
            {
                harvestRoutine = StartCoroutine(HarvestOnce(targetNode));
            }
        }

        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            rb.SetRotation(angle);
        }
    }

    IEnumerator HarvestOnce(ResourceNode node)
    {
        isHarvesting = true;

        // registra pro tremor
        if (node != null && !node.IsDepleted)
            node.RegisterHarvester();

        // espera 2–3s (ou conforme configurado)
        float t = (node != null) ? node.GetNextHarvestDuration() : 2.5f;
        yield return new WaitForSeconds(t);

        // tenta consumir (apenas 1 aldeão terá sucesso)
        if (node != null && node.TryConsume())
        {
            // adicionar recurso
            if (ResourceInventory.Instance != null)
                ResourceInventory.Instance.Add(node.type, amountPerCycle);

            // destruir o nó
            node.DepleteAndDestroy();
        }

        // desregistra (se ainda existir)
        if (node != null) node.UnregisterHarvester();

        // limpar estado e voltar a obedecer o mouse
        targetNode = null;
        isHarvesting = false;
        harvestRoutine = null;
    }

    void OnDrawGizmosSelected()
    {
        if (hasTarget)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(currentTarget, 0.1f);
        }
    }
}
