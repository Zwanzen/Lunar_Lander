using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    // ___ Private Fields ___
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private ParticleSystem thrusterParticles;
    [Space(10)]
    [Header("Move Settings")]
    [SerializeField] private float force = 10f;
    [SerializeField] private float rotationSpeed = 10f;
    [Space(10)]
    [Header("Path Render Settings")]
    [SerializeField] private int pathResolution = 100;
    [SerializeField] private float tickRate = 0.1f; 
    [SerializeField] private LayerMask pathObstructionMask;


    // ___ PRIVATE ___
    private LineRenderer pathRenderer;
    private PhysicsManager physicsManager;
    // Suggestions from copilot on optimization
    private SimObject _shipSimObject;
    private Vector3[] _pathPoints;
    private RaycastHit _raycastHitValue;
    private RaycastHit? _raycastHit;


    // ___ Singelton ___
    public static PlayerController Instance { get; private set; }

    // ___ Unity Methods ___
    private void Awake()
    {
        // Ensure that there is only one instance of PlayerController
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // Keep this instance across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    private void Start()
    {
        pathRenderer = GetComponent<LineRenderer>();
        physicsManager = PhysicsManager.Instance;
    }

    // ___ Properties ___
    public Transform Transform => transform;

    private void FixedUpdate()
    {
        HandleMovement();
        HandleShipPath();
    }

    // ___ Private Methods ___
    private void HandleMovement()
    {
        // Get main thrust force
        float thrust = InputManager.Instance.MoveInput.y * force;

        // Get rotation force
        float rotation = InputManager.Instance.MoveInput.x * rotationSpeed;

        // Apply thrust force
        rb.AddRelativeForce(Vector3.up * thrust, ForceMode.Force);
        // Apply rotation force
        rb.AddTorque(-Vector3.forward * rotation, ForceMode.Force);

        // Handle particle system
        if (thrust > 0)
        {
            var particleAmount = (int)(200 * Time.fixedDeltaTime);
            thrusterParticles.Emit(particleAmount);
        }
    }

    private struct SimObject
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float Mass;
    }

    /// <summary>
    /// Simulates the ship's path based on its current velocity and position.
    /// It takes into account every gravity source in the scene.
    /// It also is adjustable per tick rate and resolution.
    /// </summary>
    private void HandleShipPath()
    {
        // Initialize simulation object with current ship data
        // Make sure _shipSimObject is initialized in Awake or Start
        _shipSimObject.Position = transform.position;
        _shipSimObject.Velocity = rb.linearVelocity;
        _shipSimObject.Mass = rb.mass;

        // Initialize or resize path points array only when resolution changes
        if (_pathPoints == null || _pathPoints.Length != pathResolution)
        {
            _pathPoints = new Vector3[pathResolution];
        }

        // Set the first point to the current position
        _pathPoints[0] = _shipSimObject.Position;

        // Track the actual number of points we'll display
        int actualPoints = pathResolution;

        // Cache values to avoid repeated property access
        float tickRateValue = tickRate;
        float shipMass = _shipSimObject.Mass;
        Vector3 directionVector = Vector3.zero;
        Vector3 shipPosition = _shipSimObject.Position;
        Vector3 shipVelocity = _shipSimObject.Velocity;

        // To only display the line renderer when it hits an obstruction
        bool pathHit = false;

        // Loop through the path points
        for (int i = 1; i < pathResolution; i++)
        {
            Vector3 prevPosition = shipPosition;

            // Calculate the new position based on the velocity and gravity
            Vector3 acceleration = physicsManager.GetGravityAtPoint(shipPosition) / shipMass;
            shipVelocity += acceleration * tickRateValue;
            shipPosition += shipVelocity * tickRateValue;

            // Update the struct values
            _shipSimObject.Position = shipPosition;
            _shipSimObject.Velocity = shipVelocity;

            // Check if the path is obstructed between the previous point and current point
            // Avoid creating a new vector in the calculation
            directionVector = shipPosition - prevPosition;
            float distance = directionVector.magnitude;
            if (distance > 0)
            {
                directionVector /= distance; // Normalize without creating a new vector

                // Use the cached vector for the raycast direction
                if (PathObstructed(prevPosition, directionVector, distance, out Vector3 hitPoint))
                {
                    // If obstructed, set the hit point as the final point and exit early
                    _pathPoints[i] = hitPoint;
                    actualPoints = i + 1; // +1 to include the hit point
                    pathHit = true; // Mark that we hit an obstruction
                    break;
                }
            }

            // Store the position in the path points array
            _pathPoints[i] = shipPosition;
        }

        // If we didn't hit an obstruction, we turn off the path renderer
        // And dont update the line renderer
        if (!pathHit)
        {
            // If the path renderer is enabled, disable it to avoid rendering an empty path
            if (pathRenderer.enabled)
            {
                pathRenderer.enabled = false;
            }
            return; // Exit early if no obstruction was hit
        }

        // If the path renderer is not enabled, enable it
        if (!pathRenderer.enabled)
        {
            pathRenderer.enabled = true;
        }

        // Update the line renderer with the path points
        if (pathRenderer.positionCount != actualPoints)
            pathRenderer.positionCount = actualPoints;

        // Use the version of SetPositions that uses a pre-allocated array
        pathRenderer.SetPositions(_pathPoints);
    }

    private bool PathObstructed(Vector3 start, Vector3 direction, float distance, out Vector3 hitPoint)
    {
        // Reuse the same RaycastHit instance to avoid allocation
        if (_raycastHit == null)
            _raycastHit = new RaycastHit();

        hitPoint = Vector3.zero;

        if (Physics.Raycast(start, direction, out _raycastHitValue, distance, pathObstructionMask))
        {
            hitPoint = _raycastHitValue.point; 
            return true; 
        }
        return false;
    }


}
