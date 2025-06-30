using UnityEngine;

/// <summary>
/// This class helps manage and create moons and landing points in the game.
/// </summary>
public class Moon : MonoBehaviour
{
    // ___ NOTES ___
    // Structure:
    // Child 1: MoonTransform
    // Child 2: LandingPointTransform

    // ___ SERIALIZED FIELDS ___
    [Header("Moon Settings")]
    [Tooltip("The moon model and mesh used for mesh collider.")]
    [SerializeField] private Mesh moonMesh;

    // ___ PRIVATE VARIABLES ___
    private Transform moonTransform;    // Child transform representing the moon's visuals and collider.
    private MeshCollider moonCollider;  // Collider on the moonTansform.

    // ___ UNITY METHODS ___
    private void OnValidate()
    {
        ValidateMoonTransform();

    }

    // ___ PRIVATE METHODS ___
    private bool ValidateMoonTransform()
    {
        // Flag to check if the moon transform is valid.
        bool isValid = true;

        // Ensure this has a child transform as the moon transform at child index 0.
        if (transform.childCount > 0)
            moonTransform = transform.GetChild(0);
        else
            moonTransform = new GameObject("MoonTransform").transform;

        // Ensure the moonTransform has a MeshCollider component.
        moonCollider = moonTransform.GetComponent<MeshCollider>();
        if (moonCollider == null)
            moonCollider = moonTransform.gameObject.AddComponent<MeshCollider>();

        // Assign the moon mesh to the collider.
        if (moonMesh != null)
            moonCollider.sharedMesh = moonMesh;
        else
            isValid = false;

        return isValid;
    }
}
