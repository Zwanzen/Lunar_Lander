using FMODUnity;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Simple singelton to easily play the right button sounds.
/// </summary>
public class ButtonSoundManager : MonoBehaviour
{
    [SerializeField] private EventReference buttonClickSound;
    [SerializeField] private EventReference buttonHoverSound;

    private GameObject lastSelected; 

    private static ButtonSoundManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (EventSystem.current == null) return; // Ensure EventSystem exists

        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        // Check if the selected GameObject has changed
        if (currentSelected != lastSelected)
        {
            // Ensure the new selection is a Button
            if (currentSelected != null && currentSelected.GetComponent<Button>() != null)
            {
                PlayButtonHoverSound();
            }
            lastSelected = currentSelected;
        }
    }

    public static void PlayButtonClickSound()
    {
        if (instance != null)
        {
            RuntimeManager.PlayOneShot(instance.buttonClickSound);
        }
    }

    public static void PlayButtonHoverSound()
    {
        if (instance != null)
        {
            RuntimeManager.PlayOneShot(instance.buttonHoverSound);
        }
    }
}
