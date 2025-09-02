using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class Menu : MonoBehaviour
{
    [Tooltip("Optional: full-screen black Image used for fade. Leave empty for instant switch.")]
    public Image fadeImage;
    [Tooltip("Seconds for the fade out")]
    public float fadeDuration = 0.8f;

    // Called by your button OnClick()
    public void GoToSelect()
    {
        if (fadeImage != null)
            StartCoroutine(FadeAndLoad("Select"));
        else
            SceneManager.LoadScene("Select");
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        // ensure image is enabled and starts transparent
        fadeImage.gameObject.SetActive(true);
        Color c = fadeImage.color;
        float t = 0f;
        float start = c.a;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float blend = Mathf.Clamp01(t / fadeDuration);
            c.a = Mathf.Lerp(start, 1f, blend);
            fadeImage.color = c;
            yield return null;
        }

        // final ensure
        c.a = 1f;
        fadeImage.color = c;

        SceneManager.LoadScene(sceneName);
    }
}
