using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneOnClick : MonoBehaviour
{
    void Update()
    {
        // Check for left mouse click or screen tap
        if (Input.GetMouseButtonDown(0))
        {
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.LogWarning("NextSceneOnClick: No more scenes in Build Settings!");
        }
    }
}
