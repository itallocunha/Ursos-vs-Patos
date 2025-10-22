using UnityEngine;
using BTMini;

[RequireComponent(typeof(VillagerMover))]
public class VillagerAutoBrainBT : MonoBehaviour
{
    [Header("percepção")]
    public LayerMask resourceLayer;
    public float scanRadius = 6f;

    [Header("idle")]
    public float idleTime = 1f;

    [Header("patrulha")]
    public float wanderRadius = 3f;
    public float minMoveDist = 0.5f; // evita target mt perto

    VillagerMover mover;
    Node root;

    // estado interno
    ResourceNode targetNode;
    Vector2 targetSlot;
    bool hasPatrol;
    Vector2 patrolPoint;

    void Awake()
    {
        mover = GetComponent<VillagerMover>();
        root = BuildTree();
    }

    void Update()
    {
        root?.Tick();
    }

    Node BuildTree()
    {
        // ordem de prioridade:
        // 1) se está colhendo -> espera terminar
        // 2) se achou recurso perto -> vai lá e colhe
        // 3) idle 1s -> patrulhar (andar aleatorio)
        return new Selector(
            new ActionNode(IfHarvesting),
            new Sequence(
                new ActionNode(ScanNearestResource),
                new ActionNode(GoAndHarvest)
            ),
            new Sequence(
                new WaitSeconds(idleTime),
                new ActionNode(DoPatrol)
            )
        );
    }

    State IfHarvesting()
    {
        // se ja ta colhendo, deixa rolar
        return mover.IsHarvesting ? State.Running : State.Failure;
    }

    State ScanNearestResource()
    {
        // procura o ResourceNode + perto no raio
        targetNode = null;

        var hits = Physics2D.OverlapCircleAll(transform.position, scanRadius, resourceLayer);
        float best = float.MaxValue;
        ResourceNode bestNode = null;

        foreach (var h in hits)
        {
            if (h == null) continue;
            var rn = h.GetComponent<ResourceNode>() ?? h.GetComponentInParent<ResourceNode>();
            if (rn == null || rn.IsDepleted) continue;

            float d = ((Vector2)rn.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (d < best) { best = d; bestNode = rn; }
        }

        if (bestNode == null) return State.Failure;

        // calcula slot em volta do recurso (simples)
        float ring = Mathf.Max(bestNode.interactionRadius, 0.5f) + 0.2f;
        Vector2 dir = ((Vector2)transform.position - (Vector2)bestNode.transform.position).normalized;
        if (dir == Vector2.zero) dir = Vector2.right;
        targetSlot = (Vector2)bestNode.transform.position + dir * ring;

        targetNode = bestNode;
        return State.Success;
    }

    State GoAndHarvest()
    {
        if (targetNode == null || targetNode.IsDepleted) return State.Failure;

        // manda ir pra o slot (VillagerMover cuida do harvest quando encostar)
        mover.SetHarvestTarget(targetNode, targetSlot);

        // fica em Running até começar colher (ou recurso sumir)
        if (mover.IsHarvesting) return State.Success;
        if (targetNode == null || targetNode.IsDepleted) return State.Failure;
        return State.Running;
    }

    State DoPatrol()
    {
        // se nao tem ponto, cria um novo
        if (!hasPatrol)
        {
            Vector2 basePos = transform.position;
            // tenta achar um ponto afastado minimamente
            for (int i = 0; i < 8; i++)
            {
                Vector2 rnd = Random.insideUnitCircle * wanderRadius;
                if (rnd.magnitude < minMoveDist) rnd = rnd.normalized * minMoveDist;
                patrolPoint = basePos + rnd;
                hasPatrol = true;
                break;
            }
            mover.SetTarget(patrolPoint);
            return State.Running;
        }

        // já tem destino: espera chegar
        float dist = Vector2.Distance(transform.position, patrolPoint);
        if (dist <= 0.25f)
        {
            hasPatrol = false; // chegou, finaliza patrulha
            return State.Success;
        }

        // se no meio do caminho aparecer um recurso, o Selector vai priorizar no proximo tick
        return State.Running;
    }

    // gizmo pra ver raio de scan
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
        if (hasPatrol)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(patrolPoint, 0.15f);
        }
    }
}
