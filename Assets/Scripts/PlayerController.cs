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
    [Header("Float Settings")]
    [SerializeField] private LayerMask ground;
    [SerializeField] private float rayDistance = 1f;
    [SerializeField] private float spring = 5f;
    [SerializeField] private float damping = 1f;
    [SerializeField] private float floatHeight = 0.5f;
    [SerializeField] private float drag = 0.5f;

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

    // ___ Properties ___
    public Transform Transform => transform;

    private void FixedUpdate()
    {
        HandleMovement();
        //HandleLand();
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

    [Obsolete("We use static colliders instead")]
    private void HandleLand()
    {
        // Check if there is ground under us with a raycast
        // If not, return and do not apply any force
        Vector3 rayDir = -Transform.up;
        if (!Physics.Raycast(Transform.position, rayDir, out var hit, rayDistance, ground))
            return;

        // Get velocities
        Vector3 vel = rb.linearVelocity;
        float rayDirVel = Vector3.Dot(vel, rayDir);

        // Using Toyful Games Spring Logic
        float x = hit.distance - floatHeight; 
        float springForce = (x * spring) - (rayDirVel * damping);

        // If we are thrusting, we dont want to apply force in down direction
        // Though the legs should still add upward force as if the legs are catching us
        var isMoving = InputManager.Instance.IsMoving;

        // Find out if spring force is positive or negative
        // If positive and moving, we want to return
        if (springForce > 0 && isMoving)
            return;

        // Apply spring force
        rb.AddForce(rayDir * springForce);

        // Now we need to apply a drag force to stop the player from sliding around
        // Get the horizontal velocity
        Vector3 horizontalVel = new Vector3(vel.x, 0, vel.z);
        // Calculate the drag force to try and stop the player from sliding around
        Vector3 dragForce = -horizontalVel.normalized * horizontalVel.magnitude * drag;
        // Apply the drag force
        rb.AddForce(dragForce, ForceMode.Force);

        // Rotate the player to align with the ground normal but only around the z-axis
        Vector3 groundNormal = hit.normal;
        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, groundNormal);
        Quaternion currentRotation = Transform.rotation;
        // Calculate the new rotation
        Quaternion newRotation = Quaternion.Slerp(currentRotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        // Apply the new rotation
        Transform.rotation = Quaternion.Euler(0, 0, newRotation.eulerAngles.z);


        // Display debug ray
        Debug.DrawLine(Transform.position, Transform.position + rayDir * rayDistance, Color.yellow);
    }
}
