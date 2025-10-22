using UnityEngine;

public class WalkSwing : MonoBehaviour
{
    [Header("Configura��o do balan�o")]
    [Tooltip("�ngulo m�ximo de rota��o (em graus) para cada lado.")]
    public float swingAngle = 15f;

    [Tooltip("Velocidade da oscila��o (quantas vezes por segundo).")]
    public float swingSpeed = 2f;

    [Tooltip("Se verdadeiro, o balan�o s� acontece enquanto est� andando.")]
    public bool onlyWhenMoving = true;

    [Header("Movimento opcional (para teste)")]
    [Tooltip("Define automaticamente 'isMoving' enquanto o objeto anda com velocity > 0.1.")]
    public bool autoDetectMovement = true;

    private bool isMoving = true;
    private float initialZ;

    Rigidbody2D rb;

    void Start()
    {
        initialZ = transform.localEulerAngles.z;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (onlyWhenMoving && autoDetectMovement && rb != null)
        {
            // se tiver um Rigidbody2D, detecta se est� andando
            isMoving = rb.linearVelocity.sqrMagnitude > 0.05f;
        }

        if (onlyWhenMoving && !isMoving)
        {
            // volta suavemente � rota��o original quando parar
            Quaternion targetRot = Quaternion.Euler(0, 0, initialZ);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, Time.deltaTime * 5f);
            return;
        }

        // calcula �ngulo de oscila��o
        float angle = Mathf.Sin(Time.time * swingSpeed) * swingAngle;
        transform.localRotation = Quaternion.Euler(0, 0, initialZ + angle);
    }

    // permite ativar/desativar manualmente via script externo
    public void SetMoving(bool value)
    {
        isMoving = value;
    }
}
