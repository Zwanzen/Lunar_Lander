using UnityEngine;

/// <summary>
/// This class represents the moon in the game.
/// It is a singelton as it needs to be accessed from multiple scripts.
/// </summary>
public class Moon : MonoBehaviour
{
    // ___ Singelton ___
    public static Moon Instance { get; private set; }

    private void Awake()
    {
        // Ensure that there is only one instance of Moon
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keep this object across scene loads
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    // ___ Properties ___
    public Transform Transform => transform;
    public Vector3 Position => transform.position;

}
