using UnityEngine;

/// <summary>
/// Rotates the object around world z-axis at a constant speed.
/// </summary>
public class RotateObject : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 10f; // Speed of rotation in degrees per second

    // ___ Unity Methods ___
    private void Update()
    {
        // Rotate the object around the world z-axis
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime, Space.World);
    }
}
