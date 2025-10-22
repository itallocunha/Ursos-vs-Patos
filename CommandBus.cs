using UnityEngine;

public class CommandBus : MonoBehaviour
{
    public static CommandBus Instance { get; private set; }

    // publicado pelo VillageController ao clicar
    public Vector2? lastMovePoint;        // clique no chão
    public ResourceNode lastResourceNode; // clique em recurso

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ClearMove() => lastMovePoint = null;
    public void ClearResource() => lastResourceNode = null;
}
