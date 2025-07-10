using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelCard : MonoBehaviour
{

    [SerializeField] private Button levelButton;
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private GameObject[] Stars;

    private int levelIndex;

    // ___ Public Methods ___
    public void Initialize(int levelIndex)
    {
        // Make the level text be the level index
        levelNameText.text = (levelIndex + 1).ToString();
        this.levelIndex = levelIndex;
        // Load the level data
        LevelManager.Instance.LoadData(levelIndex, out var data);
        // Set the stars based on the level data
        Stars[0].SetActive(data.star1);
        Stars[1].SetActive(data.star2);
        Stars[2].SetActive(data.star3);
        // Add listener to the button
        levelButton.onClick.AddListener(LoadLevel);
    }

    /// <summary>
    /// The button of this level card has been clicked.
    /// </summary>
    public void LoadLevel()
    {
        // Load the scene based on the level index + 1
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelIndex+1);
    }

    private void OnDestroy()
    {
        // Remove the listener to prevent memory leaks
        levelButton.onClick.RemoveListener(LoadLevel);
    }
}
