using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Sets up the Platformer Window system for a scene.
/// Add this to an empty GameObject in your scene.
///
/// DOES NOT create an open button - add PlatformerTrigger to your own button/image.
/// </summary>
public class PlatformerWindowSetup : MonoBehaviour
{
    [Header("Render Settings")]
    [SerializeField] private int renderWidth = 600;
    [SerializeField] private int renderHeight = 400;

    [Header("Window Settings")]
    [SerializeField] private float bounceSpeed = 150f;
    [SerializeField] private Color panelBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);

    [Header("References (Auto-created if null)")]
    [SerializeField] private Canvas targetCanvas;

    private PlatformerWindowManager manager;

    private void Start()
    {
        Setup();
    }

    public void Setup()
    {
        // Find or create canvas
        if (targetCanvas == null)
        {
            targetCanvas = FindObjectOfType<Canvas>();
        }

        if (targetCanvas == null)
        {
            Debug.LogError("[PlatformerWindowSetup] No Canvas found! Please add a Canvas to the scene.");
            return;
        }

        // Ensure EventSystem exists
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        // Create window panel
        GameObject windowPanel = CreateWindowPanel(targetCanvas.transform);
        RawImage renderDisplay = windowPanel.GetComponentInChildren<RawImage>();

        // Create or find manager
        manager = FindObjectOfType<PlatformerWindowManager>();
        if (manager == null)
        {
            GameObject managerObj = new GameObject("PlatformerWindowManager");
            manager = managerObj.AddComponent<PlatformerWindowManager>();
        }

        // Assign references
        manager.windowPanel = windowPanel;
        manager.renderTextureDisplay = renderDisplay;
        manager.renderTextureWidth = renderWidth;
        manager.renderTextureHeight = renderHeight;
        manager.platformingSceneName = "Platforming";

        // Initialize
        manager.Initialize();

        // Wire up close button
        Button closeBtn = windowPanel.transform.Find("TitleBar/Button_X").GetComponent<Button>();
        closeBtn.onClick.AddListener(() => manager.CloseWindow());

        Debug.Log("[PlatformerWindowSetup] Setup complete. Add PlatformerTrigger to your button to open the window.");
    }

    private GameObject CreateWindowPanel(Transform parent)
    {
        // Window Panel Container
        GameObject panel = new GameObject("PlatformerWindowPanel");
        panel.transform.SetParent(parent, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
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

        // Render Display Container (with mask)
        GameObject renderContainer = new GameObject("RenderContainer");
        renderContainer.transform.SetParent(panel.transform, false);

        RectTransform containerRect = renderContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.offsetMin = new Vector2(5, 5);
        containerRect.offsetMax = new Vector2(-5, -45);

        renderContainer.AddComponent<Image>().color = Color.black;
        renderContainer.AddComponent<RectMask2D>();

        // Render Display Area
        GameObject renderArea = new GameObject("RenderDisplay");
        renderArea.transform.SetParent(renderContainer.transform, false);

        RectTransform renderRect = renderArea.AddComponent<RectTransform>();
        renderRect.anchorMin = new Vector2(0, 0);
        renderRect.anchorMax = new Vector2(1, 1);
        renderRect.offsetMin = Vector2.zero;
        renderRect.offsetMax = Vector2.zero;

        RawImage renderImage = renderArea.AddComponent<RawImage>();
        renderImage.color = Color.white;

        AspectRatioFitter aspectFitter = renderArea.AddComponent<AspectRatioFitter>();
        aspectFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        aspectFitter.aspectRatio = (float)renderWidth / (float)renderHeight;

        // Start hidden
        panel.SetActive(false);

        return panel;
    }

    private GameObject CreateButton(Transform parent, string text, float width, float height)
    {
        GameObject btnObj = new GameObject("Button_" + text);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.4f, 0.6f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.5f, 0.7f, 1f);
        colors.pressedColor = new Color(0.15f, 0.3f, 0.5f, 1f);
        btn.colors = colors;

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
