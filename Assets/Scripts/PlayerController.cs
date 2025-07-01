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
    [SerializeField] private float tickRate = 0.1f; // Time between path points in seconds


    // ___ PRIVATE ___
    private LineRenderer pathRenderer;
    private PhysicsManager physicsManager;

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
        // Define our sim object
        // This will be used to simulate the ship's path
        SimObject ship = new SimObject
        {
            Position = transform.position,
            Velocity = rb.linearVelocity,
            Mass = rb.mass
        };

        // Create a list to hold the path points
        Vector3[] pathPoints = new Vector3[pathResolution];
        // Loop through the path points
        for (int i = 0; i < pathResolution; i++)
        {
            // Calculate the time for this point
            float time = i * tickRate;
            // Calculate the new position based on the velocity and gravity
            Vector3 acceleration = physicsManager.GetGravityAtPoint(ship.Position) / ship.Mass;
            ship.Velocity += acceleration * tickRate;
            ship.Position += ship.Velocity * tickRate;
            // Store the position in the path points array
            pathPoints[i] = ship.Position;
        }

        // Update the line renderer with the path points
        pathRenderer.positionCount = pathPoints.Length;
        pathRenderer.SetPositions(pathPoints);
    }

}
