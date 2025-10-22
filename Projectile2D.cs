using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile2D : MonoBehaviour
{
    [Header("Vida �til")]
    public float maxLifetime = 3f;

    [Header("Efeitos (opcional)")]
    public GameObject hitEffect;

    private Rigidbody2D rb;
    private int damage;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        // collider deve ser IsTrigger = true
    }

    public void Launch(Vector2 direction, float speed, int damage)
    {
        this.damage = damage;
        rb.linearVelocity = direction.normalized * speed;
        CancelInvoke();
        Invoke(nameof(Despawn), maxLifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var enemy = other.GetComponent<EnemyAI>() ?? other.GetComponentInParent<EnemyAI>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            if (hitEffect) Instantiate(hitEffect, transform.position, Quaternion.identity);
            Despawn();
        }
        else
        {
            // opcional: colidir com paredes/obst�culos
            // if (((1 << other.gameObject.layer) & obstacleMask) != 0) Despawn();
        }
    }

    void Despawn()
    {
        Destroy(gameObject);
    }
}
