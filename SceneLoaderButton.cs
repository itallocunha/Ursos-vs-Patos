using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoaderButton : MonoBehaviour
{
    [Header("Como carregar")]
    [Tooltip("Deixe vazio para usar o Build Index.")]
    public string sceneName;
    [Tooltip("Use -1 para ignorar e usar o nome.")]
    public int buildIndex = -1;

    [Header("Modo de carregamento")]
    public LoadSceneMode loadMode = LoadSceneMode.Single;
    public bool useAsync = false;

    [Header("Delay antes de carregar")]
    public float delayBeforeLoad = 0f;
    [Tooltip("Se true, usa tempo real (funciona mesmo com Time.timeScale = 0).")]
    public bool delayIsRealtime = true;

    public void LoadScene()
    {
        StartCoroutine(DoLoad());
    }

    IEnumerator DoLoad()
    {
        // Delay (suporta jogo pausado)
        if (delayBeforeLoad > 0f)
        {
            if (delayIsRealtime)
                yield return new WaitForSecondsRealtime(delayBeforeLoad);
            else
                yield return new WaitForSeconds(delayBeforeLoad);
        }

        // Decide cena por índice ou nome
        if (!string.IsNullOrEmpty(sceneName))
        {
            if (useAsync)
            {
                var op = SceneManager.LoadSceneAsync(sceneName, loadMode);
                if (op == null) { Debug.LogError($"[SceneLoaderButton] Falha ao carregar cena (nome): {sceneName}. Verifique Build Settings."); yield break; }
            }
            else
            {
                try { SceneManager.LoadScene(sceneName, loadMode); }
                catch { Debug.LogError($"[SceneLoaderButton] Cena '{sceneName}' não encontrada no Build Settings."); }
            }
            yield break;
        }

        if (buildIndex >= 0)
        {
            if (buildIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError($"[SceneLoaderButton] Build Index {buildIndex} fora do range. Adicione a cena no Build Settings.");
                yield break;
            }

            if (useAsync)
            {
                var op = SceneManager.LoadSceneAsync(buildIndex, loadMode);
                if (op == null) { Debug.LogError($"[SceneLoaderButton] Falha ao carregar cena (índice): {buildIndex}."); yield break; }
            }
            else
            {
                try { SceneManager.LoadScene(buildIndex, loadMode); }
                catch { Debug.LogError($"[SceneLoaderButton] Cena de índice {buildIndex} não pôde ser carregada."); }
            }
            yield break;
        }

        Debug.LogWarning("[SceneLoaderButton] Configure sceneName ou buildIndex no Inspector.");
    }
}
