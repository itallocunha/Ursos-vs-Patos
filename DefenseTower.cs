using UnityEngine;

public class DefenseTower : MonoBehaviour
{
    [Header("Alvo e Raio")]
    public float range = 5f;
    public float targetRefreshRate = 0.2f;
    public LayerMask enemyLayer;

    [Header("Tiro")]
    public Projectile2D projectilePrefab;
    public Transform shootPoint;
    public float fireRate = 1.0f;
    public float projectileSpeed = 10f;
    public int projectileDamage = 1;

    [Header("Som do tiro")]
    public AudioClip shootSound;
    [Range(0f, 1f)] public float shootVolume = 0.7f;
    public float pitchRandom = 0.05f; // variação leve pra não soar robótico

    [Header("Mira (opcional)")]
    public bool rotateTowardsTarget = true;
    public float rotateSpeed = 720f;

    private EnemyAI currentTarget;
    private float fireCooldown;
    private float scanTimer;
    private AudioSource audioSrc;

    void Awake()
    {
        
        audioSrc = GetComponent<AudioSource>();
        if (audioSrc == null)
        {
            audioSrc = gameObject.AddComponent<AudioSource>();
            audioSrc.playOnAwake = false;
            audioSrc.spatialBlend = 0f; // 2D
        }
    }

    void Update()
    {
        scanTimer -= Time.deltaTime;
        if (scanTimer <= 0f)
        {
            scanTimer = targetRefreshRate;
            RefreshTarget();
        }

        if (rotateTowardsTarget && currentTarget != null)
        {
            Vector2 dir = (currentTarget.transform.position - transform.position);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            var q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, rotateSpeed * Time.deltaTime);
        }

        fireCooldown -= Time.deltaTime;
        if (currentTarget != null && fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = 1f / Mathf.Max(0.01f, fireRate);
        }
    }

    void RefreshTarget()
    {
        if (currentTarget != null)
        {
            float d = Vector2.Distance(transform.position, currentTarget.transform.position);
            if (d > range || !currentTarget.gameObject.activeInHierarchy)
                currentTarget = null;
        }

        if (currentTarget != null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        float best = float.MaxValue;
        EnemyAI bestEnemy = null;

        foreach (var h in hits)
        {
            var enemy = h.GetComponent<EnemyAI>() ?? h.GetComponentInParent<EnemyAI>();
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

            float d = (enemy.transform.position - transform.position).sqrMagnitude;
            if (d < best)
            {
                best = d;
                bestEnemy = enemy;
            }
        }

        currentTarget = bestEnemy;
    }

    void Shoot()
    {
        if (projectilePrefab == null || shootPoint == null || currentTarget == null) return;

        var p = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
        Vector2 dir = (currentTarget.transform.position - shootPoint.position).normalized;
        p.Launch(dir, projectileSpeed, projectileDamage);

        
        if (shootSound != null && audioSrc != null)
        {
            audioSrc.pitch = 1f + Random.Range(-pitchRandom, pitchRandom);
            audioSrc.PlayOneShot(shootSound, shootVolume);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
