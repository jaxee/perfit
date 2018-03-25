using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class StateManager : MonoBehaviour
{

    public event Action beforeLoad;
    public event Action afterLoad;

    private float fadeDuration = 1f;
    private bool isFading;

    [Header("Initial Start Scene")]
    public string startingSceneName = "_FFD";

    [Header("Initial canvas elements")]
    public CanvasGroup fadeCanvas;

    // Use this for initialization
    private IEnumerator Start()
    {
        fadeCanvas.alpha = 1f;
        yield return StartCoroutine(loadSetScene(startingSceneName));
        StartCoroutine(fade(0f));
    }

    public void loadAndFade(String sceneName)
    {
        if (!isFading)
        {
            StartCoroutine(fadeTransition(sceneName));
        }

    }

    private IEnumerator fadeTransition(String sceneName)
    {

        yield return StartCoroutine(fade(1f));//start fading scene

        if (beforeLoad != null)
        {
            beforeLoad();
        }

        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        yield return StartCoroutine(loadSetScene(sceneName));

        if (afterLoad != null)
        {
            afterLoad();
        }

        yield return StartCoroutine(fade(0f));


    }

    private IEnumerator loadSetScene(String sceneName)
    {
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        Scene newloadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        SceneManager.SetActiveScene(newloadedScene);
    }

    private IEnumerator fade(float fadeDelta)
    {
        isFading = true;

        float fadeSpeed = Mathf.Abs(fadeCanvas.alpha - fadeDelta) / fadeDuration;

        while (!Mathf.Approximately(fadeCanvas.alpha, fadeDelta))
        {
            fadeCanvas.alpha = Mathf.MoveTowards(fadeCanvas.alpha, fadeDelta,
                fadeSpeed * Time.deltaTime);
            yield return null;
        }

        isFading = false;

    }
}
