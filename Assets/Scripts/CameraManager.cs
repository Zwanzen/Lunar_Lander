using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 1.0f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CinemachineTargetGroup targetGroup;

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
        targetDirection = (moon.LandingPoint.position - moon.Position).normalized;

        // Rotate camera's up vector to match the target direction
        cameraTransform.up = targetDirection;
        SetTargetGroup(moon.LandingPoint, moon.MoonTransform);
    }

    private void OnNewMoon(Moon m)
    {
        moon = m;
        targetDirection = (moon.LandingPoint.position - moon.Position).normalized;
        SetTargetGroup(moon.LandingPoint, moon.MoonTransform);
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

        switch (rotationEffectType)
        {
            case RotationEffectType.EaseInOut:
                return EaseInOut(t);

            case RotationEffectType.Elastic:
                return ElasticEaseOut(t);

            case RotationEffectType.Bounce:
                return BounceEaseOut(t);

            case RotationEffectType.Linear:
            default:
                return t;
        }
    }

    private float EaseInOut(float t)
    {
        // Smoothstep implementation with adjustable power
        float tSquared = t * t;
        float easedT = tSquared / (2.0f * (tSquared - t) + 1.0f);

        // Apply easing power to adjust the curve
        return Mathf.Lerp(t, easedT, easingPower);
    }

    private float ElasticEaseOut(float t)
    {
        if (t == 0 || t == 1)
            return t;

        float p = elasticPeriod;
        float a = elasticAmplitude;
        float s;

        if (a < 1)
        {
            a = 1;
            s = p / 4;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(1 / a);
        }

        return a * Mathf.Pow(2, -10 * t) * Mathf.Sin((t - s) * (2 * Mathf.PI) / p) + 1;
    }

    private float BounceEaseOut(float t)
    {
        float bounce = bounceAmplitude;
        return bounce * (7.5625f * t * t);
        /*
    if (t < 1 / 2.75f)
    {
    }

    else if (t < 2 / 2.75f)
    {
        t -= 1.5f / 2.75f;
        return bounce * (7.5625f * t * t + 0.75f);
    }
    else if (t < 2.5 / 2.75)
    {
        t -= 2.25f / 2.75f;
        return bounce * (7.5625f * t * t + 0.9375f);
    }
    else
    {
        t -= 2.625f / 2.75f;
        return bounce * (7.5625f * t * t + 0.984375f);
    }
    */
    }

    private void SetTargetGroup(Transform _target, Transform _moon)
    {
        targetGroup.Targets[1].Object = _moon;
        targetGroup.Targets[2].Object = _target;
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
