using UnityEngine;

public class NextLevelButton : MonoBehaviour
{

    private void OnEnable()
    {
        // Find current scene index
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        // Check if there is a next scene
        // Current scene indec is menu + levels, so in other words, currentLvl + 1
        if(currentSceneIndex > LevelManager.LevelCount - 1)
        {
            // If there is no next scene, disable the button
            gameObject.SetActive(false);
        }

        // If there is a next scene, make the button load next scene on press
        UnityEngine.UI.Button button = GetComponent<UnityEngine.UI.Button>();
        if (button != null)
        {
            button.onClick.AddListener(LoadNextLevel);
        }
    }

    public void LoadNextLevel()
    {
        // Find current scene index
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        // Load next scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex + 1);
    }

    private void OnDestroy()
    {
        // Remove listener to prevent memory leaks
        UnityEngine.UI.Button button = GetComponent<UnityEngine.UI.Button>();
        if (button != null)
        {
            button.onClick.RemoveListener(LoadNextLevel);
        }
    }
}
