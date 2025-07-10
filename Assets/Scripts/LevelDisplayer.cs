using UnityEngine;

public class LevelDisplayer : MonoBehaviour
{
    [SerializeField] private Transform levelHolder;
    private GameObject levelPrefab;

    private void Start()
    {
        levelPrefab = Resources.Load<GameObject>("LevelCard");
        PopulateLevelHolder();
    }

    private void PopulateLevelHolder()
    {
        // Remove all existing children in the level holder
        foreach (Transform child in levelHolder)
        {
            Destroy(child.gameObject);
        }

        // Based on the level count, instantiate level prefabs
        int levelCount = LevelManager.LevelCount;
        for (int i = 0; i < levelCount; i++)
        {
            // Instantiate a new level card
            GameObject levelCard = Instantiate(levelPrefab, levelHolder);
            // Get the LevelCard component and initialize it
            LevelCard cardComponent = levelCard.GetComponent<LevelCard>();
            if (cardComponent != null)
            {
                cardComponent.Initialize(i);
            }
            else
            {
                Debug.LogError("LevelCard component not found on the instantiated prefab.");
            }
        }
    }
}
