using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameResetter : MonoBehaviour
{
    [Header("Reset")]
    public float resetDelay = 1.25f;

    public void ResetGame()
    {
        StartCoroutine(CoReset());
    }

    IEnumerator CoReset()
    {
        yield return new WaitForSeconds(resetDelay);
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }
}
