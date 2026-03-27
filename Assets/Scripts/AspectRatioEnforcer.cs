using UnityEngine;

/// <summary>
/// Forces the game to maintain a 16:9 aspect ratio with letterboxing/pillarboxing.
/// Attach to the Main Camera or a persistent game object.
/// </summary>
public class AspectRatioEnforcer : MonoBehaviour
{
    [SerializeField] private float targetAspect = 16f / 9f; // 16:9
    [SerializeField] private Color letterboxColor = Color.black;

    private Camera cam;
    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }

        EnforceAspectRatio();
    }

    private void Update()
    {
        // Check if screen size changed
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            EnforceAspectRatio();
        }
    }

    private void EnforceAspectRatio()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        // Current aspect ratio
        float currentAspect = (float)Screen.width / Screen.height;

        // Scale height to match target aspect
        float scaleHeight = currentAspect / targetAspect;

        if (cam == null) return;

        if (scaleHeight < 1f)
        {
            // Pillarbox (black bars on sides)
            Rect rect = cam.rect;
            rect.width = 1f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1f - scaleHeight) / 2f;
            cam.rect = rect;
        }
        else
        {
            // Letterbox (black bars on top/bottom)
            float scaleWidth = 1f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1f;
            rect.x = (1f - scaleWidth) / 2f;
            rect.y = 0;
            cam.rect = rect;
        }

        // Set letterbox color via background camera
        SetLetterboxColor();
    }

    private void SetLetterboxColor()
    {
        // Create a background camera to render the letterbox color
        Camera bgCam = null;
        GameObject bgCamObj = GameObject.Find("LetterboxCamera");

        if (bgCamObj == null)
        {
            bgCamObj = new GameObject("LetterboxCamera");
            bgCam = bgCamObj.AddComponent<Camera>();
            bgCam.depth = -100;
            bgCam.clearFlags = CameraClearFlags.SolidColor;
            bgCam.backgroundColor = letterboxColor;
            bgCam.cullingMask = 0; // Render nothing
            DontDestroyOnLoad(bgCamObj);
        }
    }

    private void OnDestroy()
    {
        // Reset camera rect when destroyed
        if (cam != null)
        {
            cam.rect = new Rect(0, 0, 1, 1);
        }
    }
}
