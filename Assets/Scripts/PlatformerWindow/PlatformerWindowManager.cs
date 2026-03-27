using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using Cinemachine;

public class PlatformerWindowManager : MonoBehaviour
{
    public static PlatformerWindowManager Instance { get; private set; }

    [Header("Window Settings")]
    public GameObject windowPanel;
    public RawImage renderTextureDisplay;
    public GameObject openButton;
    public int renderTextureWidth = 800;
    public int renderTextureHeight = 600;

    [Header("Scene Settings")]
    public string platformingSceneName = "Platforming";
    public Vector3 sceneOffset = new Vector3(10000, 0, 0); // Move scene far away so main camera doesn't see it

    [Header("Input")]
    public KeyCode closeKey = KeyCode.Escape;

    public RenderTexture RenderTexture { get; private set; }
    public bool IsWindowOpen { get; private set; }

    // Events for animation hooks
    public event Action OnWindowOpening;
    public event Action OnWindowOpened;
    public event Action OnWindowClosing;
    public event Action OnWindowClosed;

    private bool isPlatformingSceneLoaded = false;
    private bool isInitialized = false;
    private Camera platformerCamera;
    private Camera mainCamera;
    private GameObject player;
    private Vector3 playerSpawnPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Initialize();

        // Pre-load the platforming scene on start so it's ready when the user opens the window
        if (!string.IsNullOrEmpty(platformingSceneName))
        {
            StartCoroutine(PreloadPlatformingScene());
        }
    }

    private void Update()
    {
        if (IsWindowOpen && Input.GetKeyDown(closeKey))
        {
            CloseWindow();
        }
    }

    public void Initialize()
    {
        if (isInitialized) return;

        CreateRenderTexture();

        if (windowPanel != null)
        {
            windowPanel.SetActive(false);
        }

        // Cache the main camera
        mainCamera = Camera.main;

        isInitialized = true;
        Debug.Log("[PlatformerWindowManager] Initialized");
    }

    public void CreateRenderTexture()
    {
        if (RenderTexture != null)
        {
            RenderTexture.Release();
            Destroy(RenderTexture);
        }

        RenderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 24);
        RenderTexture.antiAliasing = 1;
        RenderTexture.Create();

        Debug.Log("[PlatformerWindowManager] RenderTexture created: " + renderTextureWidth + "x" + renderTextureHeight);

        if (renderTextureDisplay != null)
        {
            renderTextureDisplay.texture = RenderTexture;
            Debug.Log("[PlatformerWindowManager] RenderTexture assigned to display");
        }
        else
        {
            Debug.LogWarning("[PlatformerWindowManager] renderTextureDisplay is null!");
        }
    }

    public void OpenWindow()
    {
        Debug.Log("[PlatformerWindowManager] OpenWindow called");

        if (IsWindowOpen) return;

        if (!isInitialized)
        {
            Initialize();
        }

        OnWindowOpening?.Invoke();

        IsWindowOpen = true;

        if (windowPanel != null)
        {
            windowPanel.SetActive(true);
        }

        if (openButton != null)
        {
            openButton.SetActive(false);
        }

        // Disable main camera
        if (mainCamera != null)
        {
            mainCamera.enabled = false;
        }

        if (isPlatformingSceneLoaded)
        {
            // Scene already preloaded, just enable it
            EnablePlatformer(true);
            OnWindowOpened?.Invoke();
        }
        else
        {
            // Scene not loaded yet (shouldn't happen normally), load it now
            StartCoroutine(LoadPlatformingScene());
        }
    }

    public void CloseWindow()
    {
        Debug.Log("[PlatformerWindowManager] CloseWindow called");

        if (!IsWindowOpen) return;

        OnWindowClosing?.Invoke();

        IsWindowOpen = false;

        if (windowPanel != null)
        {
            windowPanel.SetActive(false);
            Debug.Log("[PlatformerWindowManager] Window panel hidden");
        }

        if (openButton != null)
        {
            openButton.SetActive(true);
        }

        // Reset player to spawn position
        ResetPlayerPosition();

        // Disable platformer but keep scene loaded
        EnablePlatformer(false);

        OnWindowClosed?.Invoke();
    }

    private void ResetPlayerPosition()
    {
        if (player != null)
        {
            player.transform.position = playerSpawnPosition;

            // Reset velocity if PlayerControls exists
            var playerControls = player.GetComponent<PlayerControls>();
            if (playerControls != null)
            {
                playerControls.SetVelocity(Vector2.zero);
            }

            Debug.Log("[PlatformerWindowManager] Player reset to spawn position");
        }
    }

    private void EnablePlatformer(bool enabled)
    {
        Debug.Log("[PlatformerWindowManager] EnablePlatformer: " + enabled);

        if (platformerCamera != null)
        {
            platformerCamera.enabled = enabled;
            Debug.Log("[PlatformerWindowManager] Platformer camera enabled: " + enabled);
        }

        // Disable main camera when platformer is open so we don't see the offset scene
        if (mainCamera != null && mainCamera != platformerCamera)
        {
            mainCamera.enabled = !enabled;
            // Ensure far clip plane is limited so it won't see offset scene
            if (!enabled)
            {
                mainCamera.farClipPlane = Mathf.Min(mainCamera.farClipPlane, 100f);
            }
            Debug.Log("[PlatformerWindowManager] Main camera enabled: " + !enabled);
        }

        // Enable/disable player controls
        if (player != null)
        {
            var playerControls = player.GetComponent<PlayerControls>();
            if (playerControls != null)
            {
                playerControls.enabled = enabled;
            }
        }
    }

    private IEnumerator PreloadPlatformingScene()
    {
        Debug.Log("[PlatformerWindowManager] Pre-loading scene: " + platformingSceneName);

        // Check if scene is in build settings
        bool sceneFound = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            if (scenePath.Contains(platformingSceneName))
            {
                sceneFound = true;
                break;
            }
        }

        if (!sceneFound)
        {
            Debug.LogError("[PlatformerWindowManager] Scene '" + platformingSceneName + "' not found in Build Settings!");
            yield break;
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(platformingSceneName, LoadSceneMode.Additive);

        if (asyncLoad == null)
        {
            Debug.LogError("[PlatformerWindowManager] Failed to start loading scene");
            yield break;
        }

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        isPlatformingSceneLoaded = true;
        Debug.Log("[PlatformerWindowManager] Scene pre-loaded successfully");

        // Keep the original scene as active
        Scene originalScene = gameObject.scene;
        SceneManager.SetActiveScene(originalScene);

        yield return null;

        // Set up the platforming scene (camera disabled, objects offset)
        SetupPlatformingScene();

        // Keep platformer DISABLED until user opens the window
        // Camera is already disabled from SetupPlatformingScene
        Debug.Log("[PlatformerWindowManager] Platforming scene ready and waiting");
    }

    private IEnumerator LoadPlatformingScene()
    {
        // If already loaded, just enable
        if (isPlatformingSceneLoaded)
        {
            EnablePlatformer(true);
            OnWindowOpened?.Invoke();
            yield break;
        }

        // Otherwise load it now
        yield return PreloadPlatformingScene();

        // Then enable
        EnablePlatformer(true);

        OnWindowOpened?.Invoke();
    }

    private void SetupPlatformingScene()
    {
        Scene platformingScene = SceneManager.GetSceneByName(platformingSceneName);
        if (!platformingScene.IsValid())
        {
            Debug.LogError("[PlatformerWindowManager] Could not find loaded platforming scene");
            return;
        }

        // Verify RenderTexture exists
        if (RenderTexture == null)
        {
            Debug.LogError("[PlatformerWindowManager] RenderTexture is null! Creating new one.");
            CreateRenderTexture();
        }

        // FIRST: Find ALL cameras in the platforming scene and configure them
        Camera[] allCameras = FindObjectsOfType<Camera>();
        int platformCameraCount = 0;

        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject.scene.name == platformingSceneName)
            {
                platformCameraCount++;

                // Disable IMMEDIATELY to prevent any screen rendering
                cam.enabled = false;

                // First camera becomes our main platformer camera
                if (platformerCamera == null)
                {
                    platformerCamera = cam;

                    // CRITICAL: Disable CinemachineBrain - it overrides camera settings!
                    CinemachineBrain brain = cam.GetComponent<CinemachineBrain>();
                    if (brain != null)
                    {
                        brain.enabled = false;
                        Debug.Log("[PlatformerWindowManager] CinemachineBrain disabled");
                    }

                    // CRITICAL: Set targetTexture - camera will ONLY render to this texture
                    cam.targetTexture = RenderTexture;

                    // Set depth to be very low so it doesn't interfere
                    cam.depth = -100;

                    // Clear flags should be solid color
                    cam.clearFlags = CameraClearFlags.SolidColor;
                    cam.backgroundColor = Color.black;

                    Debug.Log("[PlatformerWindowManager] Main platformer camera configured: " + cam.name + ", TargetTexture assigned: " + (cam.targetTexture == RenderTexture));
                }
                else
                {
                    // Disable any additional cameras in the scene
                    Debug.Log("[PlatformerWindowManager] Disabling extra camera: " + cam.name);
                }

                // Disable audio listener on all platforming cameras
                AudioListener listener = cam.GetComponent<AudioListener>();
                if (listener != null)
                {
                    listener.enabled = false;
                }
            }
        }

        Debug.Log("[PlatformerWindowManager] Found " + platformCameraCount + " camera(s) in platforming scene");

        if (platformerCamera == null)
        {
            Debug.LogError("[PlatformerWindowManager] No camera found in platforming scene!");
        }

        // SECOND: Move all root objects to the offset position
        GameObject[] rootObjects = platformingScene.GetRootGameObjects();
        foreach (GameObject obj in rootObjects)
        {
            obj.transform.position += sceneOffset;
        }

        // THIRD: Find the player and store spawn position (after offset applied)
        PlayerControls[] playerControls = FindObjectsOfType<PlayerControls>();
        foreach (var pc in playerControls)
        {
            if (pc.gameObject.scene.name == platformingSceneName)
            {
                player = pc.gameObject;
                playerSpawnPosition = player.transform.position;
                Debug.Log("[PlatformerWindowManager] Player found, spawn position stored");
                break;
            }
        }

        // Camera will be enabled by EnablePlatformer(true) after this
    }

    public void UnloadPlatformingScene()
    {
        if (isPlatformingSceneLoaded)
        {
            StartCoroutine(DoUnloadPlatformingScene());
        }
    }

    private IEnumerator DoUnloadPlatformingScene()
    {
        Debug.Log("[PlatformerWindowManager] Unloading scene");

        CloseWindow();

        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(platformingSceneName);

        if (asyncUnload != null)
        {
            while (!asyncUnload.isDone)
            {
                yield return null;
            }
        }

        isPlatformingSceneLoaded = false;
        platformerCamera = null;
        player = null;
        Debug.Log("[PlatformerWindowManager] Scene unloaded");
    }

    private void OnDestroy()
    {
        if (RenderTexture != null)
        {
            RenderTexture.Release();
            Destroy(RenderTexture);
        }
    }
}
