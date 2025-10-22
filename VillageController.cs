using System.Collections.Generic;
using UnityEngine;

public class VillageController : MonoBehaviour
{
    public static VillageController Instance { get; private set; }

    [Header("Setup")]
    public Camera cam;
    public LayerMask groundLayer;
    public LayerMask resourceLayer;

    [Header("Formation")]
    public float spacing = 1.2f;
    public bool keepFormationCentered = true;

    private readonly HashSet<VillagerMover> villagerSet = new HashSet<VillagerMover>();
    private readonly List<VillagerMover> villagerList = new List<VillagerMover>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (cam == null) cam = Camera.main;

        foreach (var v in FindObjectsOfType<VillagerMover>())
            Register(v);
    }

    public void Register(VillagerMover v)
    {
        if (v != null && villagerSet.Add(v)) villagerList.Add(v);
    }

    public void Unregister(VillagerMover v)
    {
        if (v != null && villagerSet.Remove(v)) villagerList.Remove(v);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 1) Clique em recurso → manda todos para o anel ao redor
            if (TryGetResourceAtMouse(out ResourceNode node))
            {
                SendGroupToHarvest(node);
                return;
            }

            // 2) Clique no chão → move todos em formação
            if (TryGetMouseWorldOnGround(out Vector2 target))
            {
                SendAllVillagersToMove(target);
            }
        }
    }

    bool TryGetResourceAtMouse(out ResourceNode node)
    {
        node = null;
        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 p = world;

        // Raycast 2D “ponto”
        RaycastHit2D hit = Physics2D.Raycast(p, Vector2.zero, 0f, resourceLayer);
        if (hit.collider)
            node = hit.collider.GetComponent<ResourceNode>() ?? hit.collider.GetComponentInParent<ResourceNode>();

        if (node == null)
        {
            // Fallback com OverlapPoint
            Collider2D c = Physics2D.OverlapPoint(p, resourceLayer);
            if (c) node = c.GetComponent<ResourceNode>() ?? c.GetComponentInParent<ResourceNode>();
        }
        return node != null;
    }

    bool TryGetMouseWorldOnGround(out Vector2 worldPos)
    {
        worldPos = Vector2.zero;
        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 origin = world;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.zero, 0f, groundLayer);
        if (hit.collider != null) { worldPos = hit.point; return true; }

        world.z = 0f;
        worldPos = world;
        return true;
    }

    // === MOVE TODOS (exceto quem está colhendo agora) ===
    void SendAllVillagersToMove(Vector2 targetCenter)
    {
        int n = villagerList.Count;
        if (n == 0) return;

        int cols = Mathf.CeilToInt(Mathf.Sqrt(n));
        int rows = Mathf.CeilToInt((float)n / cols);

        Vector2 origin = targetCenter;
        if (keepFormationCentered)
        {
            float totalW = (cols - 1) * spacing;
            float totalH = (rows - 1) * spacing;
            origin -= new Vector2(totalW, totalH) * 0.5f;
        }

        for (int i = 0; i < n; i++)
        {
            int r = i / cols;
            int c = i % cols;
            Vector2 slot = origin + new Vector2(c * spacing, r * spacing);
            var v = villagerList[i];
            if (v != null && !v.IsHarvesting) // <<< usa a propriedade pública
                v.SetTarget(slot);
        }
    }

    // === ENVIA TODOS PARA UM ANEL EM VOLTA DO RECURSO ===
    void SendGroupToHarvest(ResourceNode node)
    {
        int n = villagerList.Count;
        if (n == 0 || node == null) return;

        float ring = Mathf.Max(node.interactionRadius, 0.5f) + 0.2f;
        float angleStep = 360f / Mathf.Max(n, 1);

        for (int i = 0; i < n; i++)
        {
            float ang = angleStep * i * Mathf.Deg2Rad;
            Vector2 slot = (Vector2)node.transform.position + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * ring;
            var v = villagerList[i];
            if (v != null && !v.IsHarvesting) // <<< idem
                v.SetHarvestTarget(node, slot);
        }
    }

    // (opcional) usado por outros sistemas para consultar os aldeões atuais
    public List<VillagerMover> GetVillagersSnapshot()
    {
        return new List<VillagerMover>(villagerList);
    }

    // Gizmo simples para debug (opcional)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        foreach (var v in villagerList)
        {
            if (v == null) continue;
            Gizmos.DrawLine(transform.position, v.transform.position);
        }
    }
}
