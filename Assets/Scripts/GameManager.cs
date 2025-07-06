using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ___ PRIVATE FIELDS ___
    [Header("Game Settings")]
    [SerializeField] private Moon[] moons;


    // ___ PRIVATE VARIABLES ___
    private int currentLandingPointIndex = 0;

    // ___ PROPERTIES ___
    public Transform CurrentPoint { get; private set; }
    public Moon CurrentMoon => moons[currentLandingPointIndex];

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

        // Important to keep in awake. Other Scripts tries to get access at start.
        if (moons.Length > 0)
        {
            CurrentPoint = moons[currentLandingPointIndex].LandingPoint;
        }
    }

    private void Start()
    {
        
    }

    // ___ PRIVATE METHODS ___
    private float ValidateLandingQuality(LandingData data)
    {
        float quality = 0.0f;

        return quality;
    }

    // ___ PUBLIC METHODS ___
    public struct LandingData
    {
        public float InitialAngle; 
        public float InitialSpeed;
        public float DistanceToLandingPoint; 
        public float LandingTime; // Time taken to land from grounded state
    }

    /// <summary>
    /// Handles the landing event. Determines the quality of the landing, and progresses the game state accordingly.
    /// </summary>
    public void Landed(LandingData data)
    {

    }


}
