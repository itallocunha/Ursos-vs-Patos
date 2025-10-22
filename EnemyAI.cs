using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 2.2f;
    public float turnToVelocity = 720f;

    [Header("Dano na base")]
    public int collisionDamage = 5;

    [Header("Vida do inimigo")]
    public int maxHealth = 3;
    public GameObject deathEffect;

    [Header("Feedback de hit")]
    public float hitFlashTime = 0.08f;
    public Color hitColor = Color.red;

    // caminho
    private EnemyPath2D path;
    private int wpIndex = 0;

    // internals
    private int currentHealth;
    private bool dying;
    private bool hasCollidedWithBase;

    private Rigidbody2D rb;
    private Transform baseTarget;
    private SpriteRenderer sr;
    private Color srOriginalColor;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr) srOriginalColor = sr.color;
    }

    void OnEnable()
    {
        currentHealth = maxHealth;
        dying = false;
        hasCollidedWithBase = false;

        baseTarget = BaseHealth.Instance ? BaseHealth.Instance.transform : null;

       
        rb.gravityScale = 0f;
        rb.isKinematic = false; // Dynamic
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void OnDisable()
    {
        if (rb) rb.linearVelocity = Vector2.zero;
        if (sr) sr.color = srOriginalColor;
        dying = false;
        hasCollidedWithBase = false;
    }

    public void SetPath(EnemyPath2D pathToFollow)
    {
        path = pathToFollow;
        wpIndex = 0;
    }

    void FixedUpdate()
    {
        if (dying)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (BaseHealth.Instance != null && BaseHealth.Instance.currentHealth <= 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 targetPos;

        // Segue waypoints do Path
        if (path != null && path.Count > 0 && wpIndex < path.Count)
        {
            targetPos = path.GetWaypoint(wpIndex);
            Vector2 toWp = targetPos - rb.position;

            // chegou no waypoint?
            if (toWp.magnitude <= Mathf.Max(0.01f, path.arriveRadius))
            {
                wpIndex++;
                // se ainda tem próximo, recalcula target, senão cai pro passo 2 
                if (wpIndex < path.Count)
                    targetPos = path.GetWaypoint(wpIndex);
                else
                    targetPos = (baseTarget != null) ? (Vector2)baseTarget.position : rb.position;
            }
        }
        else
        {
            // Sem path vai direto à base
            targetPos = (baseTarget != null) ? (Vector2)baseTarget.position : rb.position;
        }

       
        Vector2 toTarget = targetPos - rb.position;
        Vector2 vel = toTarget.normalized * moveSpeed;
        rb.linearVelocity = vel;

        
        if (vel.sqrMagnitude > 0.001f)
        {
            float ang = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
            rb.SetRotation(Mathf.MoveTowardsAngle(rb.rotation, ang, turnToVelocity * Time.fixedDeltaTime));
        }
    }

    // Dano e morte
    public void TakeDamage(int amount)
    {
        if (dying) return;
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
        else StartCoroutine(HitFlash());
    }

    IEnumerator HitFlash()
    {
        if (!sr) yield break;
        sr.color = hitColor;
        yield return new WaitForSeconds(hitFlashTime);
        sr.color = srOriginalColor;
    }

    void Die()
    {
        if (dying) return;
        dying = true;
        if (deathEffect) Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    // Colisão com a Base 
    void OnCollisionEnter2D(Collision2D col)
    {
        if (hasCollidedWithBase) return;
        var baseHealth = col.collider.GetComponent<BaseHealth>() ?? col.collider.GetComponentInParent<BaseHealth>();
        if (baseHealth != null)
        {
            hasCollidedWithBase = true;
            baseHealth.TakeDamage(collisionDamage);
            Die();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasCollidedWithBase) return;
        var baseHealth = other.GetComponent<BaseHealth>() ?? other.GetComponentInParent<BaseHealth>();
        if (baseHealth != null)
        {
            hasCollidedWithBase = true;
            baseHealth.TakeDamage(collisionDamage);
            Die();
        }
    }
}
