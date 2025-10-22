using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class DuckQuackRandom : MonoBehaviour
{
    [Header("som do pato (curto)")]
    public AudioClip quackClip;

    [Header("chance de tocar (0–1)")]
    [Range(0f, 1f)] public float minChance = 0.1f; // 10%
    [Range(0f, 1f)] public float maxChance = 0.3f; // 30%

    [Header("duração máxima do som (segundos)")]
    public float playDuration = 0.5f;

    [Header("intervalo entre tentativas")]
    public float minDelay = 1f;
    public float maxDelay = 3f;

    private AudioSource audioSrc;

    void Awake()
    {
        audioSrc = GetComponent<AudioSource>();
        audioSrc.playOnAwake = false;
        audioSrc.loop = false;
    }

    void OnEnable()
    {
        StartCoroutine(QuackRoutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator QuackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            float chance = Random.Range(minChance, maxChance);
            if (Random.value <= chance && quackClip != null)
            {
                audioSrc.clip = quackClip;
                audioSrc.time = 0f;
                audioSrc.Play();
                yield return new WaitForSeconds(playDuration);
                audioSrc.Stop();
            }
        }
    }
}
