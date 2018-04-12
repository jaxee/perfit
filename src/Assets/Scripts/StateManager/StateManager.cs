using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class StateManager : MonoBehaviour
{

    public event Action BeforeLoad;
    public event Action AfterLoad;

    private float fadeDuration = 1f;
    private bool isFading;
    public string startingSceneName;
    public CanvasGroup fadeCanvas;

	public SaveManager bodyData;
    UIManager ui;//ui call 

    private IEnumerator Start()
    {
        ui = GameObject.FindObjectOfType<UIManager>(); 
        fadeCanvas.alpha = 1f;

		BodyscanSave.Body data = new BodyscanSave.Body ();

		data.Bust = 37.4f; 
		data.Waist = 32f;
        data.Height = 64f;
        data.Hip = 32f;

        bodyData.Save ("bodyScan", data);

        yield return StartCoroutine(loadSetScene(startingSceneName));
        StartCoroutine(fade(0f));
    }

    public void loadAndFade(String sceneName)
    {
        if (!isFading)
        {
            StartCoroutine(fadeTransition(sceneName));
            ui.ResetAll();

        }

    }

    private IEnumerator fadeTransition(String sceneName)
    {

        yield return StartCoroutine(fade(1f));//start fading scene

		if (BeforeLoad != null)
        {
			BeforeLoad();
        }

        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        yield return StartCoroutine(loadSetScene(sceneName));

        if (AfterLoad != null)
        {
			AfterLoad();
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
