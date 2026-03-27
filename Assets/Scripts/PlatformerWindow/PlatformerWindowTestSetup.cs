using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this to an empty GameObject in a new scene to automatically set up
/// a test environment for the Platformer Window feature.
///
/// Usage:
/// 1. Create a new scene
/// 2. Create an empty GameObject
/// 3. Add this script to it
/// 4. Press Play
/// 5. Click "Open Platformer" button to test
///
/// Make sure "Platforming" scene is added to Build Settings!
/// </summary>
public class PlatformerWindowTestSetup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int renderWidth = 600;
    [SerializeField] private int renderHeight = 400;
    [SerializeField] private float bounceSpeed = 150f;
    [SerializeField] private Color panelBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
    [SerializeField] private Color buttonColor = new Color(0.2f, 0.4f, 0.6f, 1f);

    private PlatformerWindowManager manager;

    private void Start()
    {
        SetupTestScene();
    }

    private void SetupTestScene()
    {
        // Create main camera if none exists
        if (Camera.main == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            cam.tag = "MainCamera";
            cam.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f; // Keep far plane low so it won't see offset scene
        }
        else
        {
            // Configure existing main camera to not see far away objects
            Camera.main.farClipPlane = 100f;
        }

        // Create EventSystem if none exists (required for UI)
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        // Create Canvas - Screen Space Overlay ensures it renders on top of everything
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // High sorting order to be on top

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Create Window Panel (hidden by default)
        GameObject windowPanel = CreateWindowPanel(canvasObj.transform);
        RawImage renderDisplay = windowPanel.GetComponentInChildren<RawImage>();

        // Create PlatformerWindowManager
        GameObject managerObj = new GameObject("PlatformerWindowManager");
        manager = managerObj.AddComponent<PlatformerWindowManager>();

        // Assign references directly (fields are now public)
        manager.windowPanel = windowPanel;
        manager.renderTextureDisplay = renderDisplay;
        manager.renderTextureWidth = renderWidth;
        manager.renderTextureHeight = renderHeight;
        manager.platformingSceneName = "Platforming";

        // Initialize after setting up references
        manager.Initialize();

        // Wire up close button
        Button closeBtn = windowPanel.transform.Find("TitleBar/Button_X").GetComponent<Button>();
        closeBtn.onClick.AddListener(() => manager.CloseWindow());

        // Create Open Button and assign reference so it can be hidden
        GameObject openBtn = CreateOpenButton(canvasObj.transform);
        manager.openButton = openBtn;

        Debug.Log("===========================================");
        Debug.Log("Platformer Window Test Setup Complete!");
        Debug.Log("Click 'Open Platformer' button to test.");
        Debug.Log("Make sure 'Platforming' scene is in Build Settings!");
        Debug.Log("===========================================");
    }

    private GameObject CreateWindowPanel(Transform parent)
    {
        // Window Panel Container
        GameObject panel = new GameObject("PlatformerWindowPanel");
        panel.transform.SetParent(parent, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        // Center anchors for bouncing movement
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        // Fixed size: render width x (render height + title bar)
        panelRect.sizeDelta = new Vector2(renderWidth + 10, renderHeight + 50);
        panelRect.anchoredPosition = Vector2.zero;

        // Background
        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = panelBackgroundColor;

        // Add DVD bounce effect
        DVDBounceEffect bounce = panel.AddComponent<DVDBounceEffect>();
        bounce.SetSpeed(bounceSpeed);

        // Title Bar
        GameObject titleBar = new GameObject("TitleBar");
        titleBar.transform.SetParent(panel.transform, false);

        RectTransform titleRect = titleBar.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(0, 40);
        titleRect.anchoredPosition = Vector2.zero;

        Image titleBg = titleBar.AddComponent<Image>();
        titleBg.color = new Color(0.05f, 0.05f, 0.1f, 1f);

        // Title Text
        GameObject titleTextObj = new GameObject("TitleText");
        titleTextObj.transform.SetParent(titleBar.transform, false);

        RectTransform titleTextRect = titleTextObj.AddComponent<RectTransform>();
        titleTextRect.anchorMin = new Vector2(0, 0);
        titleTextRect.anchorMax = new Vector2(0.8f, 1);
        titleTextRect.offsetMin = new Vector2(10, 0);
        titleTextRect.offsetMax = new Vector2(0, 0);

        Text titleText = titleTextObj.AddComponent<Text>();
        titleText.text = "Library Aisles";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 24;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleLeft;

        // Close Button
        GameObject closeBtn = CreateButton(titleBar.transform, "X", 35, 35);
        RectTransform closeBtnRect = closeBtn.GetComponent<RectTransform>();
        closeBtnRect.anchorMin = new Vector2(1, 0.5f);
        closeBtnRect.anchorMax = new Vector2(1, 0.5f);
        closeBtnRect.pivot = new Vector2(1, 0.5f);
        closeBtnRect.anchoredPosition = new Vector2(-5, 0);

        // Render Display Container (with mask to clip content)
        GameObject renderContainer = new GameObject("RenderContainer");
        renderContainer.transform.SetParent(panel.transform, false);

        RectTransform containerRect = renderContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.offsetMin = new Vector2(5, 5);
        containerRect.offsetMax = new Vector2(-5, -45);

        // Add mask to clip content within bounds
        renderContainer.AddComponent<Image>().color = Color.black; // Background
        renderContainer.AddComponent<RectMask2D>(); // Clips children to this rect

        // Render Display Area (child of container)
        GameObject renderArea = new GameObject("RenderDisplay");
        renderArea.transform.SetParent(renderContainer.transform, false);

        RectTransform renderRect = renderArea.AddComponent<RectTransform>();
        renderRect.anchorMin = new Vector2(0, 0);
        renderRect.anchorMax = new Vector2(1, 1);
        renderRect.offsetMin = Vector2.zero;
        renderRect.offsetMax = Vector2.zero;

        RawImage renderImage = renderArea.AddComponent<RawImage>();
        renderImage.color = Color.white;

        // Fill the entire area (may crop edges but no black bars)
        // Use EnvelopeParent to fill space, or FitInParent for black bars but no cropping
        AspectRatioFitter aspectFitter = renderArea.AddComponent<AspectRatioFitter>();
        aspectFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        aspectFitter.aspectRatio = (float)renderWidth / (float)renderHeight;

        // Start hidden
        panel.SetActive(false);

        return panel;
    }

    private GameObject CreateOpenButton(Transform parent)
    {
        GameObject btn = CreateButton(parent, "Open Platformer", 250, 60);

        RectTransform btnRect = btn.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = Vector2.zero;

        Button btnComponent = btn.GetComponent<Button>();
        btnComponent.onClick.AddListener(OnOpenButtonClicked);

        return btn;
    }

    private void OnOpenButtonClicked()
    {
        Debug.Log("Open button clicked!");
        if (manager != null)
        {
            manager.OpenWindow();
        }
        else
        {
            Debug.LogError("Manager is null!");
        }
    }

    private GameObject CreateButton(Transform parent, string text, float width, float height)
    {
        GameObject btnObj = new GameObject("Button_" + text);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);

        Image img = btnObj.AddComponent<Image>();
        img.color = buttonColor;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.5f, 0.7f, 1f);
        colors.pressedColor = new Color(0.15f, 0.3f, 0.5f, 1f);
        btn.colors = colors;

        // Button Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = 24;
        textComponent.color = Color.white;
        textComponent.alignment = TextAnchor.MiddleCenter;

        return btnObj;
    }
}
