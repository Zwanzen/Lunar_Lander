using MoreMountains.Feedbacks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    // ___ PRIVATE FIELDS ___
    [Header("Game Settings")]
    [SerializeField] private Moon[] moons;
    [SerializeField] private float TimeStep = 0.02f;
    [Space(10)]
    [Header("UI Settings")]
    [SerializeField] private ResultManager resultManager;
    [SerializeField] private MMF_Player missionCompleteFeedback;
    [Space(2)]
    [SerializeField] private GameObject pauseMenuButton;
    [SerializeField] private MMF_Player MissionPausedFeedback;
    [SerializeField] private MMF_Player MissionContinueFeedback;
    [Space(2)]
    [SerializeField] private GameObject missionFailButton;
    [SerializeField] private MMF_Player missionFailFeedback;
    [Space(10)]
    [Header("Mission Requirements")]
    [SerializeField] private float minFuelForStar = 20.0f;
    [Space(2)]
    [SerializeField] private float maxLandingSpeedForPerfectLanding = 5.0f;
    [SerializeField] private float maxLandingSpeedForGoodLanding = 10.0f;
    [Space(2)]
    [SerializeField] private float maxLandingDurationForPerfectLanding = 1.5f;
    [SerializeField] private float maxLandingDurationForGoodLanding = 2.5f;

    // ___ PRIVATE ___
    private int currentLandingPointIndex = 0;

    // Landing Resources
    // Just stores them initially to spawn them easier
    private GameObject badLandingPrefab;
    private GameObject goodLandingPrefab;
    private GameObject perfectLandingPrefab;

    // Mission Stats
    // Used to determine how many stars the player gets at the end of the mission.
    private bool fuelThreshold = true;
    private bool noBadLandings = true;
    private bool perfectLandings = true;

    public enum GameState
    {
        Playing,
        Paused,
        MissionFail,
        MissionComplete
    }
    private GameState currentGameState = GameState.Playing;
    private float slowDownSpeed = 1.0f;
    private float slowDownTimer = 0.0f;
    private bool stopped = false;

    // ___ EVENTS / DELEGATES ___
    public delegate void OnMoonSetDelegate(Moon m);
    public Action<Moon> OnNewMoon;

    // ___ PROPERTIES ___
    public Moon CurrentMoon { get; private set; }

    // ___ INSTANCE ___
    public static GameManager Instance { get; private set; }

    // ___ UNITY METHODS ___
    private void Awake()
    {
        // Ensure that there is only one instance of GameManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;


        // Load the landing prefabs from resources
        badLandingPrefab = Resources.Load<GameObject>("BadLanding");
        goodLandingPrefab = Resources.Load<GameObject>("GoodLanding");
        perfectLandingPrefab = Resources.Load<GameObject>("PerfectLanding");
    }

    private void Start()
    {
        // This unfortunately has to be done here,
        // because the moons needs to set their LandingPoint in awake.
        // This means if we try to get the current landing point in Awake,
        // it will not be set yet.
        // And if we set the current point in start, others will not be able to access it until the next frame.
        // Therefore, others has to use GetCurrentPoint to get the current landing point through callbacks/delegate.
        if (moons.Length > 0)
        {
            CurrentMoon = moons[currentLandingPointIndex];
            // Activate the visuals for the first moon's landing point
            CurrentMoon.LandingPoint.SetVisualsState(true);
        }

        // Subscribe to the pause action
        InputManager.Instance.Pause += PauseGame;
    }

    private void Update()
    {

        if(!stopped && currentGameState == GameState.MissionFail)
        {
            slowDownTimer += Time.deltaTime * slowDownSpeed;
            // Slow down the game if the game state is not playing
            Time.timeScale = Mathf.Lerp(1f, 0f, slowDownTimer);
            // Increase the fixed time step to get smoother slow down
            Time.fixedDeltaTime = Mathf.Lerp(TimeStep, 0.002f, slowDownTimer);

            if(slowDownTimer >= 1.0f)
            {
                stopped = true;
                // Make sure the time scale is set to 0 when the game is stopped
                Time.timeScale = 0f;
                Time.fixedDeltaTime = TimeStep; // Reset fixed delta time to normal

                // Call in the menu
            }
        }

    }

    // ___ PRIVATE METHODS ___
    private void PauseGame()
    {
        // If the game is not playing, return immediately
        if (currentGameState != GameState.Playing)
            return;


        // Set the game state to Paused
        currentGameState = GameState.Paused;
        // Stop the player
        PlayerController.Instance.GameStopped();
        // Stop the time
        Time.timeScale = 0f;
        // Play the mission paused feedback
        MissionPausedFeedback?.PlayFeedbacks();
        // Set the eventsystem to target pause menu button
        EventSystem.current.SetSelectedGameObject(pauseMenuButton);

    }

    private IEnumerator DelayNextMoon ()
    {
        yield return new WaitForSeconds(2.0f); 
        SetNextMoon(); 
    }

    private void SetNextMoon()
    {
        // Increment the index
        currentLandingPointIndex++;
        // If the index exceeds the number of moons, return
        if (currentLandingPointIndex >= moons.Length)
        {
            Debug.LogWarning("No more moons available.");
            MissionComplete(); // When the last moon has landed, the game is over
            return;
        }
        // Set the current moon to the next one in the array
        CurrentMoon = moons[currentLandingPointIndex];
        // Invoke the event to notify subscribers about the new moon
        OnNewMoon?.Invoke(CurrentMoon);
        // Set the visuals for the new moon's landing point
        CurrentMoon.LandingPoint.SetVisualsState(true);
    }

    private void MissionComplete()
    {
        // Set the game state to MissionComplete
        currentGameState = GameState.MissionComplete;
        // Stop the player
        PlayerController.Instance.GameStopped();

        // Check if the player has enough fuel for a star
        if (PlayerController.Instance.Fuel < minFuelForStar)
            fuelThreshold = false;

        // Play the feedback for mission completion
        missionCompleteFeedback?.PlayFeedbacks();

        StartCoroutine(DelayMissionComplete(1.0f));

    }

    private IEnumerator DelayMissionComplete(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        // Give the results to the result manager
        resultManager.SetStars(fuelThreshold, noBadLandings, perfectLandings);
        // Start the sequence
        resultManager.StartSequence();
    }

    /// <summary>
    /// Validates the landing quality based on the provided data.
    /// Also checks if any of the mission stats are broken.
    /// </summary>
    private void ValidateLandingQuality(LandingData data)
    {
        // Find out if the landing was perfect, good or bad.
        bool perfectLanding = data.InitialSpeed <= maxLandingSpeedForPerfectLanding && data.LandingTime <= maxLandingDurationForPerfectLanding;
        bool goodLanding = data.InitialSpeed <= maxLandingSpeedForGoodLanding && data.LandingTime <= maxLandingDurationForGoodLanding;

        // Decide what landing prefab to spawn based on the landing quality
        GameObject landingPrefab = null;
        if (perfectLanding)
        {
            landingPrefab = perfectLandingPrefab;
        }
        else if (goodLanding)
        {
            landingPrefab = goodLandingPrefab;
            perfectLandings = false;
        }
        else
        {
            landingPrefab = badLandingPrefab;
            noBadLandings = false;
            perfectLandings = false;
        }

        // Spawn the landing prefab at the player's position and rotation
        if (landingPrefab != null)
        {
            Instantiate(landingPrefab, PlayerController.Instance.transform.position, PlayerController.Instance.transform.rotation);
        }

        // Debug the landing data
       // Debug.Log($"Initial Speed = {data.InitialSpeed}, Landing Time = {data.LandingTime}");
    }

    // ___ PUBLIC METHODS ___
    /// <summary>
    /// Delegate to get the current landing point.
    /// This is used to not get errors with orders of operations.
    /// If something tries to access the current landing point before it is set, 
    /// this delegate will return the current landing point when available.
    /// </summary>
    public void GetCurrentMoon(OnMoonSetDelegate callback)
    {
        // If CurrentPoint is already set, invoke callback immediately
        if (CurrentMoon != null)
        {
            callback?.Invoke(CurrentMoon);
        }
        // Otherwise, we need to store the callback and invoke it when CurrentPoint becomes available
        else
        {
            // Store this callback to be called when CurrentPoint is set
            StartCoroutine(WaitForCurrentPoint(callback));
        }
    }

    private IEnumerator WaitForCurrentPoint(OnMoonSetDelegate callback)
    {
        // Wait until CurrentPoint is no longer null
        yield return new WaitUntil(() => CurrentMoon != null);

        // Once CurrentPoint is available, invoke the callback
        callback?.Invoke(CurrentMoon);
    }

    public struct LandingData
    {
        public float InitialSpeed;
        public float LandingTime;
    }

    /// <summary>
    /// Handles the landing event. Determines the quality of the landing, and progresses the game state accordingly.
    /// </summary>
    public void Landed(LandingData data)
    {
        // Validate the landing quality based on the provided data
        ValidateLandingQuality(data);
        // Activate the landing point visuals
        CurrentMoon.LandingPoint.SetVisualsState(false);
        // Play the flag feedback
        CurrentMoon.LandingPoint.PlayFlagFeedback();
        // Wait 2 seconds before setting the next moon
        StartCoroutine(DelayNextMoon());
    }

    public void MissionFail()
    {
        // When we crash, we want the game to slowly slow down. And when everything stops, we want to show a game over screen.
        // We set the game state to MissionFail, which will slow down the game.
        currentGameState = GameState.MissionFail;

        StartCoroutine(DelayedMissionFail());
    }

    private IEnumerator DelayedMissionFail()
    {
        // Wait for a short duration before showing the mission fail feedback
        yield return new WaitForSecondsRealtime(2.0f);
        // Play the mission fail feedback
        missionFailFeedback?.PlayFeedbacks();
        // Stop the player from moving
        PlayerController.Instance.GameStopped();
        // Set the eventsystem to target mission fail button
        EventSystem.current.SetSelectedGameObject(missionFailButton);
    }

    #region UI OPTIONS

    public void ContinueMission()
    {
        // Set the game state to Playing
        currentGameState = GameState.Playing;
        // Resume the time
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = TimeStep; 
        // Resume the player
        PlayerController.Instance.GameResumed();
        // Play the mission continue feedback
        MissionContinueFeedback?.PlayFeedbacks();
    }

    public void RestartMission()
    {
        // Stop Player
        PlayerController.Instance.GameStopped();
        // Temp Reload scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        // Make sure to fix time scale
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = TimeStep;

        // This should not be a problem... 
        // But it is...

        // Set the game state to Playing
        currentGameState = GameState.Playing;
        // Start the player
        PlayerController.Instance.GameResumed();
    }

    public void GoToMenu()
    {
        // Remember to set the time scale back to 1.0f when going back to the menu.
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = TimeStep;
    }

    public void GoNextLevel()
    {
        // Remeber to set the time scale back to 1.0f when going to the next level.
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = TimeStep;
    }
    #endregion

    private void OnDestroy()
    {
        // Unsubscribe from the pause action
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Pause -= PauseGame;
        }
    }
}
