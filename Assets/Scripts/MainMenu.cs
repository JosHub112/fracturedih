using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public Image fadeImage; // Assign FadePanel’s Image here in Inspector
    public float fadeDuration = 1f;

    public void PlayLevel1() => StartCoroutine(LoadSceneWithFade(1));
    public void PlayLevel2() => StartCoroutine(LoadSceneWithFade(2));
    public void PlayLevel3() => StartCoroutine(LoadSceneWithFade(3));

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    private IEnumerator LoadSceneWithFade(int sceneIndex)
    {
        // Fade to black
        yield return StartCoroutine(Fade(0f, 1f));

        // Load scene
        SceneManager.LoadScene(sceneIndex);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        Color c = fadeImage.color;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float blend = Mathf.Clamp01(t / fadeDuration);
            c.a = Mathf.Lerp(startAlpha, endAlpha, blend);
            fadeImage.color = c;
            yield return null;
        }
    }
}
