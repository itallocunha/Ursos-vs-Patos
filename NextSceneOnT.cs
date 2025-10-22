using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneOnT : MonoBehaviour
{
    [Header("nome da cena (igual no Build Settings)")]
    public string sceneName = "";

    [Header("delay opcional em segundos")]
    public float delay = 0f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("nome da cena ta vazio :/");
                return;
            }

            if (delay > 0f)
                Invoke(nameof(Go), delay); // espera um cadin
            else
                Go();
        }
    }

    void Go()
    {
        // tenta carregar pelo nome (sem index, sem firula)
        try
        {
            SceneManager.LoadScene(sceneName);
        }
        catch
        {
            Debug.LogError("nao consegui carregar a cena: " + sceneName + " (confere o nome no Build Settings)");
        }
    }
}
