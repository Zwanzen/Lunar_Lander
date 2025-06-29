using UnityEngine;

// Created with the help of Copilot

/// <summary>
/// Wobbles the position of the object, but only as an offset, does not move the object in a major way.
/// </summary>
public class WobbleObject : MonoBehaviour
{
    [Header("Wobble Settings")]
    [SerializeField] private float wobbleAmplitude = 0.5f; // Amplitude of the wobble
    [SerializeField] private float wobbleFrequency = 1f; // Frequency of the wobble
    [SerializeField] private Vector3 directionWeight = new Vector3(1, 1, 0); // Control wobble strength per axis
    [SerializeField] private float randomnessStrength = 0.6f; // How random the movement is (0-1)
    [SerializeField] private float directionChangeSpeed = 2.0f; // How quickly the wobble changes direction

    private Rigidbody rb; // Reference to the Rigidbody component

    private Vector3 originalLocalPosition; // Store the initial local position
    private Vector3 originalWorldPosition; // Store the initial world position
    private Transform parentTransform; // Reference to parent transform
    private Vector3 targetWobbleDirection; // Target direction to wobble towards
    private Vector3 currentWobbleDirection; // Current direction of wobble
    private float nextDirectionChangeTime; // Time to change direction
    private float noise1Offset, noise2Offset, noise3Offset; // Perlin noise offsets

    private void Awake()
    {
        // Get or add a Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        // Configure Rigidbody for kinematic movement
        rb.isKinematic = true;  // This is crucial - allows collisions without physics affecting the orbit
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Smoother movement
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // Better collision detection
    }

    private void Start()
    {
        // Store original positions
        originalLocalPosition = transform.localPosition;
        originalWorldPosition = transform.position;
        parentTransform = transform.parent;

        // Initialize with random values
        noise1Offset = Random.Range(0f, 1000f);
        noise2Offset = Random.Range(0f, 1000f);
        noise3Offset = Random.Range(0f, 1000f);

        // Set initial wobble direction
        targetWobbleDirection = Random.onUnitSphere;
        currentWobbleDirection = targetWobbleDirection;

        // Set first direction change time
        ScheduleNextDirectionChange();
    }

    private void ScheduleNextDirectionChange()
    {
        // Random time for the next direction change
        nextDirectionChangeTime = Time.time + Random.Range(0.5f, 2.0f) / directionChangeSpeed;
        targetWobbleDirection = Random.onUnitSphere;
    }

    private void Update()
    {
        // If parent has moved, update world position reference
        if (parentTransform != null)
        {
            originalWorldPosition = parentTransform.TransformPoint(originalLocalPosition);
        }

        // Check if it's time to change direction
        if (Time.time >= nextDirectionChangeTime)
        {
            ScheduleNextDirectionChange();
        }

        // Smoothly interpolate to the new direction
        currentWobbleDirection = Vector3.Lerp(
            currentWobbleDirection,
            targetWobbleDirection,
            directionChangeSpeed * Time.deltaTime
        );

        // Base wobble using sine wave
        float wobbleMagnitude = Mathf.Sin(Time.time * wobbleFrequency) * 0.5f + 0.5f;

        // Add randomness with Perlin noise for each axis
        float noiseTime = Time.time * wobbleFrequency * 0.5f;
        float xNoise = Mathf.PerlinNoise(noiseTime + noise1Offset, 0) * 2 - 1;
        float yNoise = Mathf.PerlinNoise(noiseTime + noise2Offset, 0) * 2 - 1;
        float zNoise = Mathf.PerlinNoise(noiseTime + noise3Offset, 0) * 2 - 1;

        // Combine directional wobble with noise
        Vector3 wobbleOffset = Vector3.zero;
        wobbleOffset.x = Mathf.Lerp(currentWobbleDirection.x * wobbleMagnitude, xNoise, randomnessStrength);
        wobbleOffset.y = Mathf.Lerp(currentWobbleDirection.y * wobbleMagnitude, yNoise, randomnessStrength);
        wobbleOffset.z = Mathf.Lerp(currentWobbleDirection.z * wobbleMagnitude, zNoise, randomnessStrength);

        // Apply amplitude and direction weights
        wobbleOffset *= wobbleAmplitude;
        wobbleOffset.x *= directionWeight.x;
        wobbleOffset.y *= directionWeight.y;
        wobbleOffset.z *= directionWeight.z;

        // Ensure the wobble stays within amplitude bounds
        if (wobbleOffset.magnitude > wobbleAmplitude)
        {
            wobbleOffset = wobbleOffset.normalized * wobbleAmplitude;
        }

        // Apply wobble in world space
        rb.MovePosition(originalWorldPosition + wobbleOffset);
    }

    public void ResetPosition()
    {
        // Reset to original position if needed
        transform.localPosition = originalLocalPosition;
    }
}
