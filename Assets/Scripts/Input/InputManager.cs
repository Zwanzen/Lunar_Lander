using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    // ___ Singelton Instance ___
    public static InputManager Instance { get; private set; }

    // ___ Private Fields ___
    private PlayerInputActions inputActions;
    private Vector2 moveInput;

    // ___ Properties ___
    public Vector2 MoveInput => moveInput;
    public bool IsMoving => moveInput.y > 0;

    // ___ Initialize Instance ___
    private void Awake()
    {
        // Ensure that there is only one instance of InputManager
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

    // ___ Event Actions ___
    public Action FocusTarget;
    public Action Pause;

    // ___ Initialize Inputs ___
    private void Start()
    {
        if(inputActions == null)
        {
            inputActions = new PlayerInputActions();

            inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();

            // Subscribe to the FocusTarget action
            inputActions.Player.FocusTarget.performed += ctx => FocusTarget?.Invoke();
            // Subscribe to the Pause action
            inputActions.Player.Pause.performed += ctx => Pause?.Invoke();

            inputActions.Enable();
        }
    }

    // ___ Cleanup ___
    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Disable();
            inputActions.Dispose();
        }
    }
}
