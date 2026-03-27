using UnityEngine;

/// <summary>
/// Add this to the Main Camera in the Platforming scene.
/// When loaded additively via PlatformerWindowManager, it renders to the RenderTexture.
/// When run standalone, it renders to screen normally.
/// </summary>
[RequireComponent(typeof(Camera))]
public class PlatformerCameraSetup : MonoBehaviour
{
    private Camera cam;
    private bool isSetup = false;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        SetupRenderTexture();
    }

    private void Update()
    {
        // Keep trying in case the manager wasn't ready on Start
        if (!isSetup)
        {
            SetupRenderTexture();
        }
    }

    private void SetupRenderTexture()
    {
        if (PlatformerWindowManager.Instance != null && PlatformerWindowManager.Instance.RenderTexture != null)
        {
            cam.targetTexture = PlatformerWindowManager.Instance.RenderTexture;
            isSetup = true;

            // Disable the main camera's audio listener if we have one, to avoid duplicates
            AudioListener listener = GetComponent<AudioListener>();
            if (listener != null)
            {
                listener.enabled = false;
            }
        }
        else
        {
            // Running standalone - render to screen normally
            cam.targetTexture = null;
            isSetup = true;
        }
    }

    private void OnDestroy()
    {
        // Clean up - don't leave the RenderTexture assigned
        if (cam != null)
        {
            cam.targetTexture = null;
        }
    }
}
