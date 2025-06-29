using UnityEngine;

// Created with the help of Copilot

/// <summary>
/// Used to make this object orbit around a body, with physics interactions.
/// </summary>
public class OrbitObject : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField] private Transform orbitCenter;
    [SerializeField] private float orbitSpeed = 10f;
    [SerializeField] private bool maintainCurrentRadius = true;

    private float orbitRadius;
    private float currentAngle;
    private Rigidbody rb;
    private Vector3 targetPosition;
    private Vector3 lastPosition;


    // ___ Unity Methods ___

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
        if (orbitCenter == null)
        {
            Debug.LogWarning("Orbit Center not assigned on " + gameObject.name);
            enabled = false;
            return;
        }

        // Turn off maintainCurrentRadius when the game starts
        // If this is on while the game is running, it will change its orbit radius, which is not desired
        //maintainCurrentRadius = false;

        // Save initial position to detect manual transforms
        lastPosition = transform.position;

        // Calculate initial radius and angle based on current position
        CalculateOrbitParametersFromPosition();
    }

    private void Update()
    {
        // Check if transform was manually moved in the editor or by another script
        if (transform.position != lastPosition && maintainCurrentRadius)
        {
            CalculateOrbitParametersFromPosition();
            lastPosition = transform.position;
        }
    }

    private void FixedUpdate()
    {
        if (orbitCenter == null) return;

        // Rotate around center
        currentAngle += orbitSpeed * Time.fixedDeltaTime;

        // Keep angle between 0-360
        if (currentAngle > 360f)
        {
            currentAngle -= 360f;
        }

        UpdatePosition();

        // Move the rigidbody to maintain the orbit
        rb.MovePosition(targetPosition);

        // Update last position
        lastPosition = targetPosition;
    }

    private void CalculateOrbitParametersFromPosition()
    {
        if (orbitCenter == null) return;

        // Calculate vector from center to object (in XY plane)
        Vector3 toObject = transform.position - orbitCenter.position;
        toObject.z = 0; // Ensure we're working in the XY plane

        // Calculate the radius from the current position
        orbitRadius = toObject.magnitude;

        // Calculate the current angle based on object's position
        currentAngle = Mathf.Atan2(toObject.y, toObject.x) * Mathf.Rad2Deg;

        // Ensure angle is positive
        if (currentAngle < 0)
        {
            currentAngle += 360f;
        }
    }

    private void UpdatePosition()
    {
        // Calculate position on orbit
        float radians = currentAngle * Mathf.Deg2Rad;

        // Calculate the orbit position in the XY plane (around Z-axis)
        Vector3 orbitPosition = new Vector3(
            Mathf.Cos(radians) * orbitRadius,
            Mathf.Sin(radians) * orbitRadius,
            0f
        );

        // Set the position relative to the orbit center
        targetPosition = orbitCenter.position + orbitPosition;
    }

    // Calculate the current velocity tangent to the orbit
    private Vector3 GetOrbitalVelocity()
    {
        float radians = currentAngle * Mathf.Deg2Rad;

        // Tangent to circle = perpendicular to radius
        Vector3 velocity = new Vector3(
            -Mathf.Sin(radians),  // Perpendicular to cos
            Mathf.Cos(radians),   // Perpendicular to sin
            0f
        );

        // Scale by speed and radius
        return velocity * orbitSpeed * orbitRadius;
    }

    private void OnDrawGizmosSelected()
    {
        // Display the orbit even when not in play mode
        if (orbitCenter != null)
        {
            // Calculate radius for gizmo if not in play mode
            if (!Application.isPlaying)
            {
                Vector3 toObject = transform.position - orbitCenter.position;
                toObject.z = 0; // Ensure we're working in the XY plane
                orbitRadius = toObject.magnitude;
            }

            Gizmos.color = Color.cyan;

            // Draw the orbit path
            DrawOrbitGizmo();

            // Draw line to orbit center
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, orbitCenter.position);

            // Draw orbital velocity vector when playing
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, GetOrbitalVelocity().normalized * 2f);
            }
        }
    }

    private void DrawOrbitGizmo()
    {
        // Draw orbit circle using line segments in the XY plane
        int segments = 32;
        Vector3 prevPoint = orbitCenter.position;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2;
            Vector3 orbitPosition = new Vector3(
                Mathf.Cos(angle) * orbitRadius,
                Mathf.Sin(angle) * orbitRadius,
                0f
            );

            Vector3 point = orbitCenter.position + orbitPosition;

            if (i > 0)
            {
                Gizmos.DrawLine(prevPoint, point);
            }

            prevPoint = point;
        }
    }
}
