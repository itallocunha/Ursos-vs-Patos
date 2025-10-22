using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnEscByName : MonoBehaviour
{
    [Header("nome da cena q vai carregar")]
    public string sceneName = "";

    [Header("delay opcional")]
    public float delay = 0f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("nome da cena nao ta setado :(");
                return;
            }

            if (delay > 0)
                Invoke(nameof(CarregarCena), delay);
            else
                CarregarCena();
        }
    }

    void CarregarCena()
    {
        try
        {
            SceneManager.LoadScene(sceneName);
        }
        catch
        {
            Debug.LogError("erro ao carregar a cena: " + sceneName + " (confere o nome certinho no Build Settings)");
        }
    }
}
