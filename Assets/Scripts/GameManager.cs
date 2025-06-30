using UnityEngine;

public class GameManager : MonoBehaviour
{

    // ___ PRIVATE FIELDS ___

    // ___ INSTANCE ___
    public static GameManager Instance { get; private set; }

    // ___ UNITY METHODS ___

    private void Awake()
    {
        // Ensure that there is only one instance of GameManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    // ___ PRIVATE METHODS ___
}
