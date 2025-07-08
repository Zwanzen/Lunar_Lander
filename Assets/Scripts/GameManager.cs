using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ___ PRIVATE FIELDS ___
    [Header("Game Settings")]
    [SerializeField] private Moon[] moons;


    // ___ PRIVATE ___
    private int currentLandingPointIndex = 0;

    public enum GameState
    {
        Playing,
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
        }
    }

    private void Update()
    {
        if(currentGameState == GameState.Playing)
        {
            // Handle game logic here, e.g., checking for landing conditions, updating UI, etc.
        }
        else if(!stopped)
        {
            slowDownTimer += Time.deltaTime * slowDownSpeed;
            // Slow down the game if the game state is not playing
            Time.timeScale = Mathf.Lerp(1f, 0f, slowDownTimer);
            // Increase the fixed time step to get smoother slow down
            Time.fixedDeltaTime = Mathf.Lerp(0.02f, 0.002f, slowDownTimer);

            if(slowDownTimer >= 1.0f)
            {
                stopped = true;
                // Make sure the time scale is set to 0 when the game is stopped
                Time.timeScale = 0f;
                Time.fixedDeltaTime = 0.02f; // Reset fixed delta time to normal

                // Call in the menu
            }
        }

    }

    // ___ PRIVATE METHODS ___
    private void SetNextMoon()
    {
        // Increment the index
        currentLandingPointIndex++;
        // If the index exceeds the number of moons, return
        if (currentLandingPointIndex >= moons.Length)
        {
            Debug.LogWarning("No more moons available.");
            MissionComplete(); // When the last moon has landed, the game is over.
            return;
        }
        // Set the current moon to the next one in the array
        CurrentMoon = moons[currentLandingPointIndex];
        // Invoke the event to notify subscribers about the new moon
        OnNewMoon?.Invoke(CurrentMoon);

        // Debug the past and new moon
        Debug.Log($"New Moon Set: {CurrentMoon.gameObject.name} (Index: {currentLandingPointIndex})");
    }

    private void MissionComplete()
    {

    }

    private float ValidateLandingQuality(LandingData data)
    {
        float quality = 0.0f;

        return quality;
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
        public float InitialAngle; 
        public float InitialSpeed;
        public float DistanceToLandingPoint; 
        public float LandingTime; // Time taken to land from grounded state
    }

    /// <summary>
    /// Handles the landing event. Determines the quality of the landing, and progresses the game state accordingly.
    /// </summary>
    public void Landed(LandingData data)
    {
        SetNextMoon();
    }

    public void MissionFail()
    {
        // When we crash, we want the game to slowly slow down. And when everything stops, we want to show a game over screen.
        // We set the game state to MissionFail, which will slow down the game.
        currentGameState = GameState.MissionFail;
    }

    #region UI OPTIONS

    public void GoToMenu()
    {
        // Remember to set the time scale back to 1.0f when going back to the menu.
        Time.timeScale = 1.0f;
    }

    public void GoNextLevel()
    {
        // Remeber to set the time scale back to 1.0f when going to the next level.
        Time.timeScale = 1.0f;
    }
    #endregion
}
