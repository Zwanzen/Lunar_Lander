using UnityEngine;

public class InputManager : MonoBehaviour
{

    // ___ Singelton Instance ___
    public static InputManager Instance { get; private set; }

    // ___ Private Fields ___
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private float rotationInput;

    // ___ Properties ___
    public Vector2 MoveInput => moveInput;
    public float RotationInput => rotationInput;

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

    // ___ Initialize Inputs ___
    private void Start()
    {
        if(inputActions == null)
        {
            inputActions = new PlayerInputActions();

            // Move Input
            inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            // Rotation Input
            inputActions.Player.Rotation.performed += ctx => rotationInput = ctx.ReadValue<float>();

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
