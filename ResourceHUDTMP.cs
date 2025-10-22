using UnityEngine;
using TMPro;

public class ResourceHUDTMP : MonoBehaviour
{
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;

    bool subscribed;

    void OnEnable()
    {
        TrySubscribe();
        Refresh(); // tenta pintar já
    }

    void Update()
    {
        // se o Inventory nascer depois, a gente assina aqui
        if (!subscribed) TrySubscribe();
    }

    void OnDisable()
    {
        if (ResourceInventory.Instance != null)
            ResourceInventory.Instance.OnInventoryChanged -= Refresh;
        subscribed = false;
    }

    void TrySubscribe()
    {
        var inv = ResourceInventory.Instance;
        if (inv == null) return;

        inv.OnInventoryChanged -= Refresh; // evita dupla assinatura
        inv.OnInventoryChanged += Refresh;
        subscribed = true;
    }

    void Refresh()
    {
        var inv = ResourceInventory.Instance;
        if (inv == null) return;

        if (woodText != null) woodText.text = inv.GetWood().ToString();
        if (stoneText != null) stoneText.text = inv.GetStone().ToString();
    }
}
