using UnityEngine;

public class EnemyFaceBase : MonoBehaviour
{
    [Header("tag do alvo (a base)")]
    public string baseTag = "Base";  // define a tag que a base usa

    [Header("ajuste de rotação")]
    public float rotationOffset = 0f;   // se o sprite ficar de lado, ajusta isso
    public bool smoothRotation = true;
    [Range(0f, 15f)] public float rotationSpeed = 8f;

    private Transform baseTarget;

    void Start()
    {
        // tenta achar o objeto com a tag informada
        GameObject found = GameObject.FindGameObjectWithTag(baseTag);
        if (found != null)
            baseTarget = found.transform;
        else
            Debug.LogWarning("Nao achei nenhum objeto com a tag: " + baseTag);
    }

    void Update()
    {
        if (baseTarget == null) return;

        Vector2 dir = (Vector2)baseTarget.position - (Vector2)transform.position;
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + rotationOffset;

        if (smoothRotation)
        {
            Quaternion alvo = Quaternion.Euler(0, 0, ang);
            transform.rotation = Quaternion.Lerp(transform.rotation, alvo, Time.deltaTime * rotationSpeed);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, ang);
        }
    }
}
