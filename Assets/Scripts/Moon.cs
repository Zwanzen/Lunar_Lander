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
    [SerializeField, Range(0f, 360f)] private float landingPointAngle = 0f;
    [SerializeField] private float moonSize = 1f; // Used to raycast the surface of the moon.

    // ___ PRIVATE VARIABLES ___
    private Transform moonTransform;
    private Transform landingPointTransform;

    // ___ UNITY METHODS ___
    private void OnValidate()
    {
        ValidateMoonTransform();
        ManageLandingPoint();
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
            // If there is no prefab, return false.
            if (moonPrefab == null)
            {
                return;
            }

            // If no child exists, instantiate a new moon prefab and set it as the first child.
            moonTransform = Instantiate(moonPrefab, transform.position, moonPrefab.transform.rotation, transform).transform;
            // Rename the instantiated moon transform.
            moonTransform.name = "MoonTransform";
            // Make sure it is child index 0.
            moonTransform.SetSiblingIndex(0);
            // Set the moon tag to "Moon" for identification.
            moonTransform.tag = "Moon";
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
            landingPointTransform = transform.GetChild(1);
        }
        else
        {
            // If there is no prefab, return.
            if (landingPointPrefab == null)
            {
                return;
            }
            // If no child exists, instantiate a new landing point prefab and set it as the second child.
            landingPointTransform = Instantiate(landingPointPrefab, transform.position, landingPointPrefab.transform.rotation, transform).transform;
            // Rename the instantiated landing point transform.
            landingPointTransform.name = "LandingPointTransform";
            // Make sure it is child index 1.
            landingPointTransform.SetSiblingIndex(1);
        }

        // Calculate the ray direction based on the landing point angle.
        Vector3 rayDirection = Quaternion.Euler(0f, 0f, landingPointAngle) * Vector3.up;
        Vector3 rayOrigin = moonTransform.position + rayDirection * moonSize;

        // Debug the ray with two colors
        Vector3 midpoint = rayOrigin - rayDirection * moonSize; // Midpoint of the ray
        Debug.DrawRay(rayOrigin, -rayDirection * moonSize, Color.green, 2f); // First half in green
        Debug.DrawRay(midpoint, -rayDirection * moonSize, Color.yellow, 2f); // Second half in yellow

        // Cast a ray from the moon's position to find the surface.
        int hitCount = Physics.RaycastNonAlloc(rayOrigin, -rayDirection, moonSurfaceColliders, moonSize * 2f);
        if(hitCount == 0)
        {
            Debug.LogWarning("No surface found.");
            return;
        }
        // Set the landing point position to the first hit point that has the "Moon" tag.
        for (int i = 0; i < hitCount; i++)
        {
            if (moonSurfaceColliders[i].collider.CompareTag("Moon"))
            {
                landingPointTransform.position = moonSurfaceColliders[i].point;
                landingPointTransform.rotation = Quaternion.LookRotation(moonSurfaceColliders[i].normal);
                return;
            }
        }
        // If no valid hit was found, log a warning.
        Debug.LogWarning("No valid surface found for landing point.");
    }
}
