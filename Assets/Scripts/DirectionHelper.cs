using UnityEngine;
using UnityEngine.UI;

public class DirectionHelper : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float minSize = 50f;
    [SerializeField] private float maxSize = 60f;
    [SerializeField] private float minImageSize = 1f;
    [SerializeField] private float maxImageSize = 5f;

    private Camera mainCamera;
    private Image directionImage;

    private void Awake()
    {
        mainCamera = Camera.main;
        directionImage = GetComponent<Image>();
    }

    private void Update()
    {
        // Based on the camera's zoom(size) we want to adjust the opacity of the direction image
        if (mainCamera != null && directionImage != null)
        {
            float cameraSize = mainCamera.orthographicSize;
            float alpha = Mathf.Clamp01((cameraSize - minSize) / (maxSize - minSize));
            Color color = directionImage.color;
            color.a = alpha;
            directionImage.color = color;
            // Adjust the size of the image based on the camera's zoom
            float size = Mathf.Lerp(minImageSize, maxImageSize, alpha);
            directionImage.rectTransform.sizeDelta = new Vector2(size, size);
        }
    }
}
