using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// This class helps manage and create moons and landing points in the game.
/// </summary>
public class Moon : MonoBehaviour
{
    // ___ NOTES ___
    // Structure:
    // Child 0: MoonTransform
    // Child 1: LandingPointTransform

    // ___ SERIALIZED FIELDS ___
    [Header("Moon Refrences")]
    [SerializeField] private GameObject moonPrefab;
    [SerializeField] private GameObject landingPointPrefab;
    [Space(10)]
    [Header("Moon Settings")]
    [SerializeField] float moonGravity = 2f;
    [SerializeField] float moonRange = 100f;
    [SerializeField] float maxPoint = 0.4f; // Point in range [0-1] where gravity is maximum (0 = at source, 1 = at edge)
    [Space(10)]
    [Header("Moon Lading")]
    [SerializeField] private float landingPointAngle = 0f;
    [SerializeField] private float moonSize = 100f; // Used to raycast the surface of the moon.
    [Space(20)]
    [Header("Debug")]
    [SerializeField] private bool reGenerateMoon = false;
    [SerializeField] private bool displayRay = false;

    // ___ PRIVATE VARIABLES ___
    private Transform moonTransform;

    // ___ PROPERTIES ___
    public Transform MoonTransform => moonTransform;
    public Vector3 Position => moonTransform.position;
    public Transform LandingPoint { get; private set; }

    // ___ UNITY METHODS ___
    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        // If reGenerateMoon is true, reset the moon
        if(reGenerateMoon)
        {
            reGenerateMoon = false; // Reset the flag
            if(PhysicsManager.Instance != null)
            {
                PhysicsManager.Instance.ClearGravitySources();
            }
            Destroy(moonTransform?.gameObject); // Destroy the old moon transform
            Destroy(LandingPoint?.gameObject); // Destroy the old landing point transform
        }

        // Make sure the angle is not above 360 degrees or below 0 degrees.
        landingPointAngle = Mathf.Repeat(landingPointAngle, 360f);

        ValidateMoonTransform();
        ManageLandingPoint();
        RegisterGravitySource();
    }

    private void Awake()
    {
        // Get moontransform from the first child if it exists.
        if (transform.childCount > 0)
        {
            moonTransform = transform.GetChild(0);
        }
        else
        {
            Debug.LogError("Moon Transform is not set. Please validate the moon transform.");
        }
        // Get landing point transform from the second child if it exists.
        if (transform.childCount > 1)
        {
            LandingPoint = transform.GetChild(1);
        }
        else
        {
            Debug.LogError("Landing Point Transform is not set. Please validate the landing point transform.");
        }
    }

    private void Start()
    {
        ValidateMoonTransform();
        ManageLandingPoint();
        RegisterGravitySource();
    }

    // ___ PRIVATE METHODS ___
    private void ValidateMoonTransform()
    {
        // Ensure this has a child transform as the moon transform at child index 0.
        if (transform.childCount > 0)
        {
            moonTransform = transform.GetChild(0);
        }
        else
        {
            // If there is no prefab, use default moon prefab.
            if (moonPrefab == null)
            {
                // Get the default moon prefab from the resources folder.
                moonPrefab = Resources.Load<GameObject>("DefaultMoon");

                // If the default moon prefab is not found, log an error.
                if (moonPrefab == null)
                {
                    Debug.LogError("Default moon prefab not found in Resources folder. Please ensure it exists.");
                    return;
                }
            }

            // If no child exists, instantiate a new moon prefab and set it as the first child.
            // We need to add this rotation with the moon prefab's rotation to ensure it is correctly oriented.
            Quaternion moonRotation = transform.rotation * moonPrefab.transform.rotation;
            moonTransform = Instantiate(moonPrefab, transform.position, moonRotation, transform).transform;
            // Rename the instantiated moon transform.
            moonTransform.name = "MoonTransform";
            // Make sure it is child index 0.
            moonTransform.SetSiblingIndex(0);
            // Set the moon tag to "Moon" for identification.
            moonTransform.tag = "Moon";
            // Set the moons layer to 3 (Ground Layer)
            moonTransform.gameObject.layer = LayerMask.NameToLayer("Ground");
        }
    }

    private RaycastHit[] moonSurfaceColliders = new RaycastHit[10];
    private void ManageLandingPoint()
    {
        // Place the landing point at the surface of the moon,
        // from the angle specified and the moon's position.
        if (moonTransform == null)
        {
            Debug.LogError("Moon Transform is not set. Please validate the moon transform.");
            return;
        }
        // Ensure this has a child transform as the landing point transform at child index 1.
        if (transform.childCount > 1)
        {
            LandingPoint = transform.GetChild(1);
        }
        else
        {
            // If there is no prefab, load the default landing point prefab from resources.
            if (landingPointPrefab == null)
            {
                // Get the default landing point prefab from the resources folder.
                landingPointPrefab = Resources.Load<GameObject>("DefaultPoint");
                // If the default landing point prefab is not found, log an error.
                if (landingPointPrefab == null)
                {
                    Debug.LogError("Default landing point prefab not found in Resources folder. Please ensure it exists.");
                    return;
                }
            }
            // If no child exists, instantiate a new landing point prefab and set it as the second child.
            LandingPoint = Instantiate(landingPointPrefab, transform.position, landingPointPrefab.transform.rotation, transform).transform;
            // Rename the instantiated landing point transform.
            LandingPoint.name = "LandingPointTransform";
            // Make sure it is child index 1.
            LandingPoint.SetSiblingIndex(1);
        }

        // Calculate the ray direction based on the landing point angle.
        Vector3 rayDirection = Quaternion.Euler(0f, 0f, landingPointAngle) * Vector3.up;
        Vector3 rayOrigin = moonTransform.position + rayDirection * moonSize;

        if (displayRay)
        {
            // Debug the ray with two colors
            Vector3 midpoint = rayOrigin - rayDirection * moonSize; // Midpoint of the ray
            Debug.DrawRay(rayOrigin, -rayDirection * moonSize, Color.green, 2f); // First half in green
            Debug.DrawRay(midpoint, -rayDirection * moonSize, Color.yellow, 2f); // Second half in yellow
        }

        // Cast a ray from the moon's position to find the surface.
        int hitCount = Physics.RaycastNonAlloc(rayOrigin, -rayDirection, moonSurfaceColliders, moonSize * 2f);
        if(hitCount == 0)
        {
            Debug.LogWarning("No surface found.");
            return;
        }
        // Set the landing point position to the first hit point that hits this moon.
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = moonSurfaceColliders[i];
            if (hit.collider != null && hit.collider.transform == moonTransform)
            {
                // Set the landing point position to the hit point.
                LandingPoint.position = hit.point;
                // Set the landing point rotation to face upwards.
                LandingPoint.rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
                return;
            }
        }

        // If no valid hit was found, log a warning.
        Debug.LogWarning("No valid surface found for landing point.");
    }

    private void RegisterGravitySource()
    {
        // Register this moon as a gravity source in the PhysicsManager.
        if (PhysicsManager.Instance == null)
        {
            return;
        }
        GravitySource gravitySource = new GravitySource
        {
            source = moonTransform,
            gravity = moonGravity,
            range = moonRange,
            maxGravityPoint = maxPoint
        };
        PhysicsManager.Instance.AddGravitySource(gravitySource);
    }

    private void UnregisterGravitySource()
    {
        // Unregister this moon as a gravity source in the PhysicsManager.
        if (PhysicsManager.Instance == null)
        {
            return;
        }
        PhysicsManager.Instance.RemoveGravitySource(moonTransform);
    }

    private void OnDestroy()
    {
        UnregisterGravitySource();
    }

}
