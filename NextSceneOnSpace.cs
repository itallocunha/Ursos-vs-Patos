using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneOnSpace : MonoBehaviour
{
   
    public bool loadNextByBuildIndex = true;
    public string nextSceneName = "";
    public float delayBeforeLoad = 0f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (loadNextByBuildIndex)
                LoadNextSceneByIndex();
            else
                LoadSceneByName();
        }
    }

    void LoadNextSceneByIndex()
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        int next = current + 1;

        if (next < SceneManager.sceneCountInBuildSettings)
        {
            if (delayBeforeLoad > 0)
                Invoke(nameof(LoadNext), delayBeforeLoad);
            else
                LoadNext();
        }
        else
        {
            Debug.LogWarning(" Não há próxima cena");
        }
    }

    void LoadNext()
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        int next = current + 1;
        SceneManager.LoadScene(next);
    }

    void LoadSceneByName()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("...nenhuma cena");
            return;
        }

        if (delayBeforeLoad > 0)
            Invoke(nameof(LoadByName), delayBeforeLoad);
        else
            LoadByName();
    }

    void LoadByName()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
