using UnityEngine;
using System.Collections.Generic;

public class InfiniteClouds2D : MonoBehaviour
{
    [Header("Referências")]
    public Camera cam;

    [Header("Prefabs de nuvem (com SpriteRenderer)")]
    public GameObject[] cloudPrefabs;

    [Header("Pool / Spawn")]
    [Min(1)] public int poolSize = 12;
    public float spawnInterval = 1.2f;
    public float spawnIntervalJitter = 0.4f;

    [Tooltip("Se true, até o primeiro spawn sempre nasce fora da direita. (Recomendado p/ cruzar a tela inteira)")]
    public bool startFromRightOnly = true;
    [Tooltip("Se true e startFromRightOnly também true, cria um pequeno burst inicial espaçado à direita.")]
    public bool warmupBurst = true;
    [Range(0, 8)] public int warmupCount = 4;
    public float warmupSpacing = 1.5f; // espaçamento em larguras das nuvenss

    [Header("Movimento")]
    public Vector2 speedRange = new Vector2(0.3f, 1.0f);
    public Vector2 yRange = new Vector2(-2f, 3f);
    public Vector2 scaleRange = new Vector2(0.8f, 1.6f);

    [Header("Renderização")]
    public float cloudsZ = 0f;
    public string sortingLayerName = "Background";
    public int sortingOrder = -10;

    [Header("Parallax (opcional)")]
    public bool parallaxByScale = true;
    [Range(0f, 1f)] public float parallaxMultiplier = 0.35f;

    [Header("Limites")]
    public float horizontalMargin = 2f;

    class Cloud
    {
        public GameObject go;
        public Transform tf;
        public float speed;
        public float halfWidth;
        public float baseScale;
        public bool active;
    }

    readonly List<Cloud> pool = new List<Cloud>();
    float _nextSpawnAt;
    float _leftX, _rightX, _zDist;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!cam)
        {
            Debug.LogError("[InfiniteClouds2D] Nenhuma câmera encontrada.");
            enabled = false; return;
        }
        if (cloudPrefabs == null || cloudPrefabs.Length == 0)
        {
            Debug.LogError("[InfiniteClouds2D] Adicione prefabs em cloudPrefabs.");
            enabled = false; return;
        }
    }

    void Start()
    {
        RecalcBounds();
        BuildPool();

        // para já ver nuvens entrando em sequência, mas todas vão cruzar a tela inteira.
        if (startFromRightOnly && warmupBurst)
            DoWarmupBurst();

        ScheduleNextSpawn();
    }

    void Update()
    {
        RecalcBounds();

     
        for (int i = 0; i < pool.Count; i++)
        {
            var c = pool[i];
            if (!c.active) continue;

            float parallaxFactor = 1f;
            if (parallaxByScale)
                parallaxFactor = Mathf.Lerp(1f, 1f - parallaxMultiplier,
                                            Mathf.InverseLerp(scaleRange.x, scaleRange.y, c.baseScale));

            c.tf.position += Vector3.left * (c.speed * parallaxFactor) * Time.deltaTime;

            // terminou a travessia? (com margem e largura do prefab)
            if (c.tf.position.x + c.halfWidth < _leftX - horizontalMargin)
            {
                DeactivateToPool(c);
            }
        }

        if (Time.time >= _nextSpawnAt)
        {
            TrySpawnCloud();
            ScheduleNextSpawn();
        }
    }

    void RecalcBounds()
    {
        _zDist = Mathf.Abs(cam.transform.position.z - cloudsZ);
        Vector3 leftWorld = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, _zDist));
        Vector3 rightWorld = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, _zDist));
        _leftX = leftWorld.x;
        _rightX = rightWorld.x;
    }

    void BuildPool()
    {
        pool.Clear();
        for (int i = 0; i < poolSize; i++)
        {
            var prefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];
            var go = Instantiate(prefab, transform);
            go.name = $"Cloud_{i}";
            go.transform.position = new Vector3(99999, 99999, cloudsZ);

            ApplySorting(go);

            var c = new Cloud
            {
                go = go,
                tf = go.transform,
                speed = 0f,
                halfWidth = 0f,
                baseScale = 1f,
                active = false
            };
            pool.Add(c);

            go.SetActive(false);
        }
    }

    void ApplySorting(GameObject go)
    {
        var srs = go.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in srs)
        {
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = sortingOrder;
        }
    }

    Cloud GetInactiveFromPool()
    {
        for (int i = 0; i < pool.Count; i++)
            if (!pool[i].active) return pool[i];
        return null; 
    }

    void TrySpawnCloud()
    {
        var c = GetInactiveFromPool();
        if (c == null) return;

        c.go.SetActive(true);

        // randomiza aparência ou movimento delas
        float scale = Random.Range(scaleRange.x, scaleRange.y);
        c.tf.localScale = Vector3.one * scale;
        c.baseScale = scale;

        c.speed = Random.Range(speedRange.x, speedRange.y);
        c.halfWidth = GetRendererBoundsXHalf(c.go);

        float y = Random.Range(yRange.x, yRange.y);

        float startX;
        if (startFromRightOnly)
        {
            startX = _rightX + horizontalMargin + c.halfWidth;
        }
        else
        {
           
            startX = Random.Range(_leftX - horizontalMargin, _rightX + horizontalMargin);
        }

        c.tf.position = new Vector3(startX, y, cloudsZ);
        c.active = true;
    }

    void DeactivateToPool(Cloud c)
    {
        c.active = false;
        c.go.SetActive(false);
        c.tf.position = new Vector3(99999, 99999, cloudsZ);
    }

    float GetRendererBoundsXHalf(GameObject go)
    {
        var srs = go.GetComponentsInChildren<SpriteRenderer>(true);
        if (srs.Length > 0)
        {
            Bounds b = srs[0].bounds;
            for (int i = 1; i < srs.Length; i++)
                b.Encapsulate(srs[i].bounds);
            return b.size.x * 0.5f;
        }
        var rs = go.GetComponentsInChildren<Renderer>(true);
        if (rs.Length > 0)
        {
            Bounds b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++)
                b.Encapsulate(rs[i].bounds);
            return b.size.x * 0.5f;
        }
        return 0.5f;
    }

    void ScheduleNextSpawn()
    {
        float jitter = (spawnIntervalJitter <= 0f) ? 0f : Random.Range(-spawnIntervalJitter, spawnIntervalJitter);
        _nextSpawnAt = Time.time + Mathf.Max(0.05f, spawnInterval + jitter);
    }

    void DoWarmupBurst()
    {
        // instancia a nuvens já enfileiradas à direita, que vão até a esquerda do mapa
        float cursorX = _rightX + horizontalMargin;
        int created = 0;
        while (created < warmupCount)
        {
            var c = GetInactiveFromPool();
            if (c == null) break;

            c.go.SetActive(true);

            float scale = Random.Range(scaleRange.x, scaleRange.y);
            c.tf.localScale = Vector3.one * scale;
            c.baseScale = scale;

            c.speed = Random.Range(speedRange.x, speedRange.y);
            c.halfWidth = GetRendererBoundsXHalf(c.go);
            float y = Random.Range(yRange.x, yRange.y);

            cursorX += c.halfWidth * (created == 0 ? 1f : warmupSpacing) + c.halfWidth;
            c.tf.position = new Vector3(cursorX, y, cloudsZ);
            c.active = true;

            created++;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!cam) cam = Camera.main;
        if (!cam) return;

        float zDist = Mathf.Abs(cam.transform.position.z - cloudsZ);
        Vector3 leftWorld = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, zDist));
        Vector3 rightWorld = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, zDist));

        Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
        Gizmos.DrawLine(new Vector3(leftWorld.x, cam.transform.position.y - 100, cloudsZ),
                        new Vector3(leftWorld.x, cam.transform.position.y + 100, cloudsZ));
        Gizmos.DrawLine(new Vector3(rightWorld.x, cam.transform.position.y - 100, cloudsZ),
                        new Vector3(rightWorld.x, cam.transform.position.y + 100, cloudsZ));
    }
#endif
}
