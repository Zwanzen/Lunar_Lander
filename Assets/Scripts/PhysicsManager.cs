using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    // ___ Private Fields ___
    [Header("Physics Settings")]
    [SerializeField] private Transform gravitySource;
    [SerializeField] private float gravity = -1.62f;
    [Space(2)]
    [SerializeField] private Rigidbody[] gravityObjects;

    // ___ Singleton Instance ___
    public static PhysicsManager Instance { get; private set; }

    // ___ Initialize Instance ___
    private void Awake()
    {
        // Ensure that there is only one instance of PhysicsManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this instance across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    // ___ Unity Methods ___
    private void FixedUpdate()
    {
        ApplyGravity();
    }

    // ___ Private Methods ___
    private void ApplyGravity()
    {
        if (gravityObjects.Length == 0)
        {
            return; // No objects to apply gravity to
        }
        foreach (Rigidbody rb in gravityObjects)
        {
            if (rb != null)
            {
                Vector3 direction = (gravitySource.position - rb.position).normalized;
                rb.AddForce(direction * gravity, ForceMode.Acceleration);
            }
        }
    }

}
