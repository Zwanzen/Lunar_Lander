using UnityEngine;

/// <summary>
/// Used to set the z rotation in line with the direction from the moon and to the player.
/// </summary>
public class TestRotation : MonoBehaviour
{
    [SerializeField] private Transform moonTransform;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float rotationSpeed = 50f;

    private void Start()
    {
        // Set initial rotation if needed
        if (moonTransform == null || playerTransform == null)
        {
            Debug.LogError("Moon or Player Transform is not assigned in the inspector.");
        }
        else
        {
            Vector3 directionToPlayer = playerTransform.position - moonTransform.position;
            float initialAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            initialAngle -= 90f; // Adjust angle to match Unity's coordinate system (0 degrees is up)
            transform.rotation = Quaternion.Euler(0, 0, initialAngle);
        }
    }

    private void Update()
    {
        if (moonTransform != null && playerTransform != null)
        {
            Vector3 directionToPlayer = playerTransform.position - moonTransform.position;
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            angle -= 90f; // Adjust angle to match Unity's coordinate system (0 degrees is up)
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
