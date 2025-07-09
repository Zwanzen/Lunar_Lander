using FMODUnity;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the landing and the quality of the landing.
/// Calls to the game manager to progress the game state.
/// </summary>
public class LandingManager : MonoBehaviour
{
    // ___ PRIVATE FIELDS ___
    [Header("Landing Settings")]
    [SerializeField] private Slider landingSlider;
    [SerializeField] private Transform leftFoot;
    [SerializeField] private Transform rightFoot;
    [SerializeField] private LayerMask groundLayerMask = 1 << 3;
    [SerializeField] private float footRadius = 0.1f;
    [SerializeField] private StudioEventEmitter chargeSoundEmitter;
    [SerializeField] private EventReference chargeSuccessfulSound;
    [SerializeField] private MMF_Player sliderStartFeedback;
    [SerializeField] private MMF_Player sliderStopFeedback;
    [SerializeField] private MMF_Player chargingFeedback;
    [Space(10)]
    [Header("Crash Settings")]
    [SerializeField] private float crashVelocityThreshold = 5f;
    [SerializeField] private EventReference crashSoundEvent;
    [SerializeField] private EventReference collisionSoundEvent;

    // ___ PRIVATE VARIABLES ___
    private GameManager gameManager;
    private Rigidbody rb;

    private Transform currentLandingPoint = null;
    private float landingRange = 7f;
    private const float TimeToLand = 1.0f;
    private const float MaxVelocityToLand = 0.5f; // Maximum velocity to be considered landed
    private float timerToLand = 0.0f; // When this reaches timeToLand, the ship is considered landed.
    private bool startedLanding = false;

    private GroundState groundState = GroundState.None;
    private bool isLeftLegGrounded = false;
    private bool isRightLegGrounded = false;

    // Stored data for landing quality
    // Will not be reset until the ship is landed.
    private float totalLandTime = 0.0f;
    private float initialImpactVelocity = -1.0f;

    private bool crashed = false;
    private Vector3 storedVelocity;

    /// <summary>
    /// The state at which the ship is grounded.
    /// If only a single leg is grounded, the ship is not considered landed.
    /// However, still might need this information for other purposes, like animations or sound effects.
    /// </summary>
    private enum GroundState
    {
        None,
        Single,
        Both
    }

    // ___ UNITY METHODS ___
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // Get the current landing point from the GameManager
        gameManager = GameManager.Instance;
        gameManager.GetCurrentMoon(OnCurrentMoonGet);

        // Subscribe to the event for when the current moon is set
        gameManager.OnNewMoon += OnCurrentMoonGet;
    }

    private void Update()
    {
        HandleLanding();
        HandleLandingSlider();
        HandleLandingSound();
    }

    private void FixedUpdate()
    {
        UpdateGroundedState();
        if(!crashed)
            storedVelocity = rb.linearVelocity; // Store the current velocity for crash handling
    }

    private void OnCollisionEnter(Collision collision)
    {
        // First check if we crashed
        if (Crashed(collision))
        {
            // Stop the player early
            PlayerController.Instance.GameStopped();
            Crash();
            return;
        }

        // If the collision is not a crash, and it the relative velocity is greater than 0.1f, we play the collision sound.
        if (collision.relativeVelocity.magnitude > 0.4f)
        {
            RuntimeManager.PlayOneShot(collisionSoundEvent, transform.position);
        }

        // If we did not crash, check if we are landing
        if (IsAtLandingRange() && currentLandingPoint != null)
        {
            // If the initial impact velocity is not set (-1)
            // We set the initial impact velocity to the collision's relative velocity magnitude.
            if (initialImpactVelocity < 0f)
                initialImpactVelocity = collision.relativeVelocity.magnitude;
        }
    }

    // ___ PRIVATE METHODS ___
    private void HandleLanding()
    {
        // If the ship is already crashed, we do not handle landing anymore.
        if (crashed)
        {
            // Stop all active feedbacks
            if (sliderStartFeedback.IsPlaying)
                sliderStartFeedback.StopFeedbacks();
            if (sliderStopFeedback.IsPlaying)
                sliderStopFeedback.StopFeedbacks();
            if (chargingFeedback.IsPlaying)
                chargingFeedback.StopFeedbacks();
            chargeSoundEmitter.Stop();

            return;
        }
        if (!startedLanding && timerToLand > 0f)
        {
            // Start the landing sound
            chargeSoundEmitter.Play();
            sliderStartFeedback?.PlayFeedbacks();
            // Interupt the stop feedbacks if they are playing
            sliderStopFeedback?.StopFeedbacks();
            startedLanding = true;
        }
        else if (startedLanding && timerToLand <= 0f)
        {
            // Stop the landing sound
            chargeSoundEmitter.Stop();
            sliderStopFeedback?.PlayFeedbacks();
            startedLanding = false;
        }

        // Returns if the ship is not currently landing
        if (!IsAtLandingRange() || currentLandingPoint == null || groundState == GroundState.None)
        {
            if (timerToLand > 0f)
            {
                timerToLand -= Time.deltaTime * 5f; 
            }
            if (timerToLand < 0f)
            {
                timerToLand = 0f;
            }

            if (chargingFeedback.IsPlaying)
                chargingFeedback.StopFeedbacks();
            return;
        }

        // If both legs are grounded, start the landing timer
        if (groundState == GroundState.Both && rb.linearVelocity.magnitude <= MaxVelocityToLand)
        {
            timerToLand += Time.deltaTime;
            totalLandTime += Time.deltaTime;
            if(!chargingFeedback.IsPlaying)
                chargingFeedback.PlayFeedbacks();

            // If the timer reaches the time to land, consider the ship landed
            if (timerToLand >= TimeToLand)
            {
                // Reset the timer and notify the GameManager about the landing
                currentLandingPoint = null;
                // Create landing quality data
                GameManager.LandingData landingQualityData = new GameManager.LandingData
                {
                    LandingTime = totalLandTime,
                    InitialSpeed = initialImpactVelocity,
                };
                gameManager.Landed(landingQualityData);
                RuntimeManager.PlayOneShot(chargeSuccessfulSound, transform.position);
                timerToLand = 0.0f;

                // Reset stored data
                totalLandTime = 0.0f;
                initialImpactVelocity = -1.0f;
            }
        }
        else if (groundState == GroundState.Single)
        {
            if (chargingFeedback.IsPlaying)
                chargingFeedback.StopFeedbacks();

            // Still increase the total time
            totalLandTime += Time.deltaTime;

            // Reset the timer if not both legs are grounded
            if (timerToLand >= 0f)
            {
                timerToLand -= Time.deltaTime * 5f; // Decrease the timer faster if not grounded
                if (timerToLand < 0.01f)
                {
                    timerToLand = 0.01f; // Ensure timer does not go to zero for slider reasons
                }
            }
        }
    }

    private void HandleLandingSlider()
    {
        // When timerToLand is greater than 0, we are in the process of landing.
        if (landingSlider.IsActive())
        {
            //landingSlider.gameObject.SetActive(true);
            // Calculate the normalized value for the slider
            float normalizedValue = Mathf.Clamp01(timerToLand / TimeToLand);
            landingSlider.value = normalizedValue;
        }
    }

    private void HandleLandingSound()
    {
        // If we are landing, we set the charge parameter to the timerToLand normalized value.
        if (startedLanding)
        {
            float chargeValue = Mathf.Clamp01(timerToLand / TimeToLand);
            chargeSoundEmitter.SetParameter("Charge", chargeValue);
        }
    }

    private void OnCurrentMoonGet(Moon m)
    {
        currentLandingPoint = m.LandingPointTransform;
    }

    private bool IsAtLandingRange()
    {
        if (currentLandingPoint == null)
            return false;
        float distanceToLandingPoint = Vector2.Distance(transform.position, currentLandingPoint.position);
        return distanceToLandingPoint <= landingRange;
    }

    private bool IsLegGrounded(Transform leg)
    {
        if(Physics.CheckSphere(leg.position, footRadius, groundLayerMask))
            return true;
        return false;
    }

    private void UpdateGroundedState()
    {
        isLeftLegGrounded = IsLegGrounded(leftFoot);
        isRightLegGrounded = IsLegGrounded(rightFoot);
        if (isLeftLegGrounded && isRightLegGrounded)
        {
            groundState = GroundState.Both;
        }
        else if (isLeftLegGrounded || isRightLegGrounded)
        {
            groundState = GroundState.Single;
        }
        else
        {
            groundState = GroundState.None;
        }

    }

    private bool Crashed(Collision col)
    {
        // Check if the collision velocity exceeds the crash threshold
        return col.relativeVelocity.magnitude > crashVelocityThreshold;
    }

    /// <summary>
    /// If the player crashes, this method handles all the visuals of the ship crashing.
    /// </summary>
    private void Crash()
    {
        crashed = true;
        TurnOffMainVisuals();

        // Spawn in a Fractured ship from resources and set the position and rotation to the current ship's position and rotation.
        GameObject fracturedShipPrefab = Resources.Load<GameObject>("Fractured");
        GameObject fracturedShip = Instantiate(fracturedShipPrefab, transform.position, transform.rotation);


        // Get all the rigidbodies in the fractured ship and give them the current ship's velocity.
        Rigidbody[] fracturedRigidbodies = fracturedShip.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody fracturedRb in fracturedRigidbodies)
        {
            fracturedRb.linearVelocity = storedVelocity;
        }

        var cameraManager = CameraManager.Instance;
        cameraManager.Crashed(fracturedRigidbodies);

        // Spawn Sound
        RuntimeManager.PlayOneShot(crashSoundEvent, transform.position);

        // Notify the GameManager about the crash
        gameManager.MissionFail();
    }

    private void TurnOffMainVisuals()
    {
        // Set the rigidbody to kinematic to stop all physics interactions
        rb.isKinematic = true;
        // Turn off colliders
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
        // Turn off main collider
        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null)
        {
            mainCollider.enabled = false;
        }
        // Trun off this transforms first child (It holds the mesh)
        Transform shipMesh = transform.GetChild(0);
        if (shipMesh != null)
        {
            shipMesh.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Ship mesh not found in the first child of the ship transform.");
        }

    }

    // ___ PUBLIC METHODS ___
    public void StopSounds()
    {
        // Stop all active feedbacks
        if (sliderStartFeedback.IsPlaying)
            sliderStartFeedback.StopFeedbacks();
        if (sliderStopFeedback.IsPlaying)
            sliderStopFeedback.StopFeedbacks();
        if (chargingFeedback.IsPlaying)
            chargingFeedback.StopFeedbacks();
        chargeSoundEmitter.Stop();
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to prevent memory leaks
        if (gameManager != null)
        {
            gameManager.OnNewMoon -= OnCurrentMoonGet;
        }
    }
}
