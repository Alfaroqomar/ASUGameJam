using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this to any Button or Image to make it open the Platformer window when clicked.
/// For Images, make sure Raycast Target is enabled.
/// </summary>
public class PlatformerTrigger : MonoBehaviour, IPointerClickHandler
{
    [Header("Optional")]
    [Tooltip("If left empty, will find PlatformerWindowManager automatically")]
    [SerializeField] private PlatformerWindowManager manager;

    private void Awake()
    {
        // Ensure raycast target is enabled for click detection
        Graphic graphic = GetComponent<Graphic>();
        if (graphic != null)
        {
            graphic.raycastTarget = true;
        }
        else
        {
            // If no graphic, check for Image or add one
            Image img = GetComponent<Image>();
            if (img == null)
            {
                Debug.LogWarning("[PlatformerTrigger] No Image/Graphic found on " + gameObject.name + ". Add an Image component for click detection.");
            }
        }
    }

    private void Start()
    {
        // Try to find manager if not assigned
        if (manager == null)
        {
            manager = PlatformerWindowManager.Instance;
        }
        if (manager == null)
        {
            manager = FindObjectOfType<PlatformerWindowManager>();
        }

        // If this has a Button component, also wire up the onClick event
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OpenPlatformer);
        }

        Debug.Log("[PlatformerTrigger] Ready. Manager found: " + (manager != null));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("[PlatformerTrigger] Clicked!");
        OpenPlatformer();
    }

    public void OpenPlatformer()
    {
        // Try to find manager again if still null
        if (manager == null)
        {
            manager = PlatformerWindowManager.Instance;
        }
        if (manager == null)
        {
            manager = FindObjectOfType<PlatformerWindowManager>();
        }

        if (manager != null)
        {
            manager.OpenWindow();
        }
        else
        {
            Debug.LogError("[PlatformerTrigger] PlatformerWindowManager not found! Make sure PlatformerWindowSetup is in the scene.");
        }
    }
}
