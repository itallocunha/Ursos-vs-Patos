using System;
using UnityEngine;
using UnityEngine.Events;

public class BaseHealth : MonoBehaviour
{
    public static BaseHealth Instance { get; private set; }

    [Header("Vida da Base")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Eventos")]
    public UnityEvent onBaseDamaged;
    public UnityEvent onBaseDestroyed;

    [Header("Debug")]
    public bool enableDebugDamageKey = false;
    public KeyCode damageKey = KeyCode.B;
    public int debugDamageAmount = 10;

    // avisa quem quiser: (atual, max)
    public event Action<int, int> OnHealthChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        currentHealth = Mathf.Clamp(currentHealth <= 0 ? maxHealth : currentHealth, 0, maxHealth);
    }

    void Update()
    {
        if (enableDebugDamageKey && Input.GetKeyDown(damageKey))
            TakeDamage(debugDamageAmount); // testee
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        onBaseDamaged?.Invoke();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth == 0)
            HandleDestroyed(); // chegou no zero, ja
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || currentHealth <= 0) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth); // atualiza hud
    }

    void HandleDestroyed()
    {
        onBaseDestroyed?.Invoke();

        // reseta jogo aqui via outro script (se nao tiver, avisa no log)
        var resetter = FindObjectOfType<GameResetter>();
        if (resetter != null)
            resetter.ResetGame();
        else
            Debug.LogWarning("GameResetter nao encontrado na cena. Adiciona pra reiniciar quando a base morrer.");
    }
}
