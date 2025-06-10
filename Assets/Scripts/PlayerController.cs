using UnityEditor.ShaderGraph.Internal;
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

    // ___ Unity Methods ___
    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
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
}
