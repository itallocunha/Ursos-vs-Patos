using UnityEngine;

[RequireComponent(typeof(VillagerMover))]
public class VillagerBrainBT : MonoBehaviour
{
    private VillagerMover mover;

    void Awake()
    {
        mover = GetComponent<VillagerMover>();
    }

    void Update()
    {
        // Só reage a clique no chão. Clique em recurso é tratado pelo VillageController (SendGroupToHarvest).
        var bus = CommandBus.Instance;
        if (bus == null) return;

        if (bus.lastMovePoint.HasValue && !mover.IsHarvesting)
        {
            mover.SetTarget(bus.lastMovePoint.Value);
            // não limpamos o comando: todos podem seguir para o mesmo ponto
        }
    }
}
