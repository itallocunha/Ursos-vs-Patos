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
        // S� reage a clique no ch�o. Clique em recurso � tratado pelo VillageController (SendGroupToHarvest).
        var bus = CommandBus.Instance;
        if (bus == null) return;

        if (bus.lastMovePoint.HasValue && !mover.IsHarvesting)
        {
            mover.SetTarget(bus.lastMovePoint.Value);
            // n�o limpamos o comando: todos podem seguir para o mesmo ponto
        }
    }
}
