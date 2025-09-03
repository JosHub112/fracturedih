using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class Menu : MonoBehaviour
{
    [Tooltip("Optional: full-screen black Image used for fade. Leave empty for instant switch.")]
    [SerializeField] private Image fadeImage;
    [Tooltip("Seconds for the fade out")]
    [SerializeField] private float fadeDuration = 0.8f;

    // Set this in the Inspector or change the value below
    [SerializeField] private int targetSceneIndex = 1;

    // Hook this to your Button's OnClick()
    public void GoToSelect()
    {
        if (fadeImage != null)
            StartCoroutine(FadeAndLoad(targetSceneIndex));
        else
            SceneManager.LoadScene(targetSceneIndex);
    }

    private IEnumerator FadeAndLoad(int sceneIndex)
    {
        // Ensure overlay exists, is enabled, and starts fully transparent
        fadeImage.gameObject.SetActive(true);
        var c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime; // works even if timeScale == 0
            float blend = Mathf.Clamp01(t / fadeDuration);
            c.a = Mathf.Lerp(0f, 1f, blend);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 1f;
        fadeImage.color = c;

        yield return SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);
    }
}
