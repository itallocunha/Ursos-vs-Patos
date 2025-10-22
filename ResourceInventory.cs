using UnityEngine;
using System;

[DefaultExecutionOrder(-100)] // acorda antes da UI
public class ResourceInventory : MonoBehaviour
{
    public static ResourceInventory Instance { get; private set; }

    public int wood;
    public int stone;

    public event Action OnInventoryChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject); // nao some qdo trocar cena
    }

    public void Add(ResourceType type, int amount)
    {
        if (amount <= 0) return;

        if (type == ResourceType.Wood) wood += amount;
        else stone += amount;

        OnInventoryChanged?.Invoke();
    }

    // opcional: chamar pra forçar refresh manual
    public void ForceNotify() => OnInventoryChanged?.Invoke();

    public int GetWood() => wood;
    public int GetStone() => stone;
}
