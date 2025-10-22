using UnityEngine;
using UnityEngine.Events;

public class BaseUpgradeTower : MonoBehaviour
{
    [Header("Custo por torre")]
    public int woodCost = 2;   // 2 madeira
    public int stoneCost = 1;  // 1 pedra

    [Header("Torres (desative todas no inicio)")]
    public GameObject[] towers;

    [Header("Feedback")]
    public UnityEvent onTowerActivated;
    public UnityEvent onNotEnoughResources;

    [Tooltip("Loga msgs de debug pra entender pq nao ativou")]
    public bool verboseLog = true;

    void Start()
    {
        
        
        /*
        for (int i = 0; i < towers.Length; i++)
        {
            if (towers[i] != null) towers[i].SetActive(false);
        }
        */
    }

    
    void OnMouseDown()
    {
        TryActivateNextTower();
    }

    public void TryActivateNextTower()
    {
        var inv = ResourceInventory.Instance;
        if (inv == null)
        {
            if (verboseLog) Debug.LogWarning("ResourceInventory nao encontrado.");
            return;
        }

        int next = FindNextInactiveIndex();
        if (next == -1)
        {
            if (verboseLog) Debug.Log("todas as torres ja foram ativadas :)");
            onNotEnoughResources?.Invoke(); // usa como 'fim de fila' tbm
            return;
        }

        if (!HasResources(inv, woodCost, stoneCost))
        {
            if (verboseLog)
                Debug.Log($"sem recurso: precisa {woodCost} madeira e {stoneCost} pedra. Voce tem {inv.wood}/{inv.stone}.");
            onNotEnoughResources?.Invoke();
            return;
        }

        Spend(inv, woodCost, stoneCost);

        var go = towers[next];
        if (go == null)
        {
            if (verboseLog) Debug.LogWarning($"torre no indice {next} esta nula. pulando…");
        }
        else
        {
            go.SetActive(true);
            if (verboseLog) Debug.Log($"torre ativada: {go.name} (idx {next})");
            onTowerActivated?.Invoke();
        }
    }

    int FindNextInactiveIndex()
    {
        if (towers == null || towers.Length == 0) return -1;

        // procura a primeira torre desativada no array
        for (int i = 0; i < towers.Length; i++)
        {
            var go = towers[i];
            if (go == null) continue;
            if (!go.activeSelf) return i;
        }
        return -1; // nenhuma desativada => acabou
    }

    bool HasResources(ResourceInventory inv, int woodNeed, int stoneNeed)
    {
        return inv.wood >= woodNeed && inv.stone >= stoneNeed;
    }

    void Spend(ResourceInventory inv, int woodNeed, int stoneNeed)
    {
        inv.wood -= woodNeed;
        inv.stone -= stoneNeed;

        
    }
}
