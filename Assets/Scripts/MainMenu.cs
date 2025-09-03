using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("Full-screen Image used for fade. Assign FadePanel’s Image here in Inspector.")]
    [SerializeField] private Image fadeImage;
    [Tooltip("Seconds for the fade in/out.")]
    [SerializeField] private float fadeDuration = 0.8f;

    [Header("Optional behavior")]
    [Tooltip("If true, after the new scene loads the image will fade from black back to transparent.")]
    [SerializeField] private bool fadeInAfterLoad = false;
    [Tooltip("If true the fadeImage GameObject will be kept across scenes (required for fadeInAfterLoad).")]
    [SerializeField] private bool keepFadeObjectAcrossScenes = false;

    // Example scene indices (set to your build settings indices or use the name overloads)
    [SerializeField] private int level1Index = 2;
    [SerializeField] private int level2Index = 3;
    [SerializeField] private int level3Index = 4;
    [SerializeField] private int selectionIndex = 1; // optional index-based selection

    // --- Button hooks ---
    public void PlayLevel1() => StartCoroutine(LoadSceneWithFade(level1Index));
    public void PlayLevel2() => StartCoroutine(LoadSceneWithFade(level2Index));
    public void PlayLevel3() => StartCoroutine(LoadSceneWithFade(level3Index));

    // index or name versions for "Selection"
    public void GoToSelectByIndex() => StartCoroutine(LoadSceneWithFade(selectionIndex));
    public void GoToSelectionByName() => StartCoroutine(LoadSceneWithFade("Selection"));

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    private IEnumerator LoadSceneWithFade(int sceneIndex)
    {
        if (fadeImage == null)
        {
            yield return SceneManager.LoadSceneAsync(sceneIndex);
            yield break;
        }

        if (fadeInAfterLoad && !keepFadeObjectAcrossScenes)
        {
            keepFadeObjectAcrossScenes = true;
        }

        if (keepFadeObjectAcrossScenes)
            DontDestroyOnLoad(fadeImage.gameObject);

        fadeImage.gameObject.SetActive(true);
        var c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;

        yield return StartCoroutine(Fade(0f, 1f));

        var op = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);
        while (!op.isDone)
            yield return null;

        if (fadeInAfterLoad)
        {
            yield return null;
            yield return StartCoroutine(Fade(1f, 0f));
        }
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        if (fadeImage == null)
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield break;
        }

        if (fadeInAfterLoad && !keepFadeObjectAcrossScenes)
        {
            keepFadeObjectAcrossScenes = true;
        }

        if (keepFadeObjectAcrossScenes)
            DontDestroyOnLoad(fadeImage.gameObject);

        fadeImage.gameObject.SetActive(true);
        var c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;

        yield return StartCoroutine(Fade(0f, 1f));

        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!op.isDone)
            yield return null;

        if (fadeInAfterLoad)
        {
            yield return null;
            yield return StartCoroutine(Fade(1f, 0f));
        }
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        Color c = fadeImage.color;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float blend = Mathf.Clamp01(t / fadeDuration);
            c.a = Mathf.Lerp(startAlpha, endAlpha, blend);
            fadeImage.color = c;
            yield return null;
        }

        c.a = endAlpha;
        fadeImage.color = c;
    }
}
