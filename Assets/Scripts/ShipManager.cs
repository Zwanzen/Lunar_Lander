using UnityEngine;

/// <summary>
/// This controls the ship the player deploys from, gets additional fuel, and returns to after missions.
/// This can be a singelton as it needs to be accessed from multiple scripts.
/// </summary>
public class ShipManager : MonoBehaviour
{
    // ___ Variables ___
    [Header("Ship Settings")]
    [SerializeField] private float orbitalSpeed = 5f; // Speed at which the ship orbits the moon
    [SerializeField] private float orbitalRadius = 10f; // Distance from the moon to the ship
    [SerializeField] private float angle = 0f; // Current angle of the ship in its orbit


    // ___ References ___
    [SerializeField]
    private Transform moonTransform; // Reference to the moon's transform
    private Transform playerTransform; // Reference to the ship's transform

    // ___ Singelton ___
    public static ShipManager _instance { get; private set; }

    private void Awake()
    {
        // Ensure that there is only one instance of ShipManager
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keep this object across scene loads
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize references
        moonTransform = Moon.Instance.Transform;
        playerTransform = PlayerController.Instance.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        // Think we should handle it here to try and keep the ship from being framerate dependent
        float deltaTime = Time.fixedDeltaTime;
        HandleOrbit(deltaTime);
    }

    // ___ Private Methods ___
    /// <summary>
    /// Handles the ship's orbit around the moon based on orbital speed and radius.
    /// </summary>
    private void HandleOrbit(float delta)
    {
        // Move the ship in a circular orbit around the moon starting from the top
        angle += orbitalSpeed * delta; // Increment the angle based on speed and time

        UpdateShipLocation(); // Update the ship's position based on the new angle
    }

    /// <summary>
    /// Updates the ship's location based on changes in the ship radius.
    /// This is put in OnValidate so that it can be updated in the editor when the radius is changed.
    /// </summary>
    private void UpdateShipLocation()
    {
        if (CalculateShipOffset(out Vector3 pos, out Vector3 up))
        {
            // Update the ship's position and up direction
            transform.position = pos;
            transform.up = up;
        }
        else
        {
            Debug.LogError("Failed to calculate ship offset.");
        }
    }

    private bool CalculateShipOffset(out Vector3 pos, out Vector3 up)
    {
        pos = Vector3.zero;
        up = Vector3.zero;
        if (moonTransform == null)
        {
            Debug.LogError("Moon transform is not set.");
            return false;
        }

        // Calculate the position with angle adjusted to start from top (90 degrees offset)
        pos = new Vector3(
            moonTransform.position.x + orbitalRadius * Mathf.Cos(angle - Mathf.PI * 1.5f),
            moonTransform.position.y + orbitalRadius * Mathf.Sin(angle - Mathf.PI * 1.5f),
            moonTransform.position.z
        );

        // Ensure the ship up direction is always away from the moon
        Vector3 directionToMoon = (moonTransform.position - pos).normalized;
        up = -directionToMoon;
        return true;
    }


    private void OnValidate()
    {
        UpdateShipLocation();
    }
}
