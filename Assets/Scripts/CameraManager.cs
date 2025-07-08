using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 1.0f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CinemachineTargetGroup targetGroup;

    // ___ INSTANCE ___
    public static CameraManager Instance { get; private set; }

    // ___ PRIVATE ___
    private GameManager gameManager;
    private Moon moon;
    private Vector3 targetDirection;

    // Rotation effect types
    public enum RotationEffectType
    {
        Linear,
        EaseInOut,
        Elastic,
        Bounce
    }

    [Tooltip("Select the type of rotation effect")]
    [SerializeField] private RotationEffectType rotationEffectType = RotationEffectType.Linear;

    // Parameters for effects
    [Range(0, 1)]
    [SerializeField] private float easingPower = 0.5f;
    [SerializeField] private float elasticAmplitude = 1.0f;
    [SerializeField] private float elasticPeriod = 0.3f;
    [SerializeField] private float bounceAmplitude = 1.0f;

    // Keeps track of rotation progress
    private float rotationProgress = 0f;
    private Vector3 startDirection;
    private bool isRotating = false;

    private void Awake()
    {
        // Ensure that there is only one instance of CameraManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        // Try and get the current moon from the GameManager
        gameManager = GameManager.Instance;
        gameManager.GetCurrentMoon(OnSetCurrentMoon);

        // Subscribe to the event for when the current moon is set
        gameManager.OnNewMoon += OnNewMoon;
    }

    private void Update()
    {
        HandleRotation();
    }

    // _____ PRIVATE METHODS ___
    private void OnSetCurrentMoon(Moon m)
    {
        moon = m;
        targetDirection = (moon.LandingPointTransform.position - moon.Position).normalized;

        // Rotate camera's up vector to match the target direction
        cameraTransform.up = targetDirection;
        SetTargetGroup(moon.LandingPointTransform, moon.MoonTransform);
    }

    private void OnNewMoon(Moon m)
    {
        moon = m;
        targetDirection = (moon.LandingPointTransform.position - moon.Position).normalized;
        SetTargetGroup(moon.LandingPointTransform, moon.MoonTransform);
    }

    private void HandleRotation()
    {
        if (!isRotating && Vector3.Angle(cameraTransform.up, targetDirection) < 0.1f)
        {
            return; // No need to rotate if we're already aligned
        }

        // If we're not already rotating, start a new rotation
        if (!isRotating)
        {
            startDirection = cameraTransform.up;
            rotationProgress = 0f;
            isRotating = true;
        }

        // Increase progress based on time and speed
        rotationProgress += Time.deltaTime * rotationSpeed;

        // Get the interpolation factor based on the selected effect
        float t = GetInterpolationFactor(rotationProgress);

        // Apply the interpolated rotation
        Vector3 newDirection = Vector3.Slerp(startDirection, targetDirection, t);
        cameraTransform.up = newDirection;

        // Check if rotation is complete
        if (rotationProgress >= 1.0f)
        {
            isRotating = false;
            cameraTransform.up = targetDirection; // Ensure perfect alignment
        }
    }

    private float GetInterpolationFactor(float t)
    {
        // Clamp t between 0 and 1
        t = Mathf.Clamp01(t);

        return BounceEaseOut(t);
    }

    private float BounceEaseOut(float t)
    {
        float bounce = bounceAmplitude;
        return bounce * (7.5625f * t * t);
    }

    private void SetTargetGroup(Transform _target, Transform _moon)
    {
        targetGroup.Targets[1].Object = _moon;
        targetGroup.Targets[2].Object = _target;
    }

    // ___ PUBLIC METHODS ___
    public void Crashed(Rigidbody[] fracturedPieces)
    {
        // Remove all targets from the target group
        targetGroup.Targets.RemoveRange(0, targetGroup.Targets.Count);
        // Add fractured pieces as targets
        foreach (Rigidbody piece in fracturedPieces)
        {
            targetGroup.AddMember(piece.transform, 1.0f, 0.0f);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to prevent memory leaks
        if (gameManager != null)
        {
            gameManager.OnNewMoon -= OnSetCurrentMoon;
        }
    }
}
