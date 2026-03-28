using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Add this to any GameObject to diagnose UI input issues.
/// Check the Console for diagnostic information.
/// </summary>
public class UIInputDiagnostic : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== UI INPUT DIAGNOSTIC ===");

        // Check EventSystem
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogError("PROBLEM: No EventSystem found! UI clicks won't work.");
            Debug.Log("FIX: GameObject → UI → Event System");
        }
        else
        {
            Debug.Log("OK: EventSystem found - " + eventSystem.gameObject.name);

            // Check for input module
            BaseInputModule inputModule = eventSystem.GetComponent<BaseInputModule>();
            if (inputModule == null)
            {
                Debug.LogError("PROBLEM: EventSystem has no Input Module!");
                Debug.Log("FIX: Add StandaloneInputModule to EventSystem");
            }
            else
            {
                Debug.Log("OK: Input Module found - " + inputModule.GetType().Name);
            }
        }

        // Check all Canvases
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        Debug.Log("Found " + canvases.Length + " Canvas(es)");

        foreach (Canvas canvas in canvases)
        {
            Debug.Log("--- Canvas: " + canvas.gameObject.name + " ---");
            Debug.Log("  Render Mode: " + canvas.renderMode);
            Debug.Log("  Sorting Order: " + canvas.sortingOrder);

            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                Debug.LogError("  PROBLEM: No GraphicRaycaster! Clicks won't work on this Canvas.");
                Debug.Log("  FIX: Add GraphicRaycaster component to " + canvas.gameObject.name);
            }
            else
            {
                Debug.Log("  OK: GraphicRaycaster found, enabled: " + raycaster.enabled);
            }
        }

        // Check for buttons
        Button[] buttons = FindObjectsOfType<Button>();
        Debug.Log("Found " + buttons.Length + " Button(s)");
        foreach (Button btn in buttons)
        {
            Debug.Log("  Button: " + btn.gameObject.name + ", Interactable: " + btn.interactable);
        }

        Debug.Log("=== END DIAGNOSTIC ===");
    }

    void Update()
    {
        // Log when mouse is clicked
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse clicked at: " + Input.mousePosition);

            // Check what the EventSystem thinks we're hovering over
            EventSystem es = EventSystem.current;
            if (es != null)
            {
                GameObject hoveredObj = es.currentSelectedGameObject;

                // Use pointer data to find what's under the mouse
                PointerEventData pointerData = new PointerEventData(es);
                pointerData.position = Input.mousePosition;

                var results = new System.Collections.Generic.List<RaycastResult>();
                es.RaycastAll(pointerData, results);

                if (results.Count > 0)
                {
                    Debug.Log("UI elements under mouse: " + results.Count);
                    foreach (var result in results)
                    {
                        Debug.Log("  - " + result.gameObject.name);
                    }
                }
                else
                {
                    Debug.LogWarning("No UI elements detected under mouse! Check GraphicRaycaster.");
                }
            }
        }
    }
}
