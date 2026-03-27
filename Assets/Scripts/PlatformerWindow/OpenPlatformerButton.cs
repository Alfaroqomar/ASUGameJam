using UnityEngine;

/// <summary>
/// Simple script to open the platformer window.
///
/// SETUP:
/// 1. Add this script to ANY GameObject in your scene (can be empty)
/// 2. Select your Button in the scene
/// 3. In the Button's OnClick() list, click +
/// 4. Drag the GameObject with this script to the slot
/// 5. Select OpenPlatformerButton -> OpenPlatformer()
/// </summary>
public class OpenPlatformerButton : MonoBehaviour
{
    /// <summary>
    /// Call this from a Button's OnClick event
    /// </summary>
    public void OpenPlatformer()
    {
        Debug.Log("[OpenPlatformerButton] OpenPlatformer called!");

        PlatformerWindowManager manager = PlatformerWindowManager.Instance;

        if (manager == null)
        {
            manager = FindObjectOfType<PlatformerWindowManager>();
        }

        if (manager != null)
        {
            Debug.Log("[OpenPlatformerButton] Opening window...");
            manager.OpenWindow();
        }
        else
        {
            Debug.LogError("[OpenPlatformerButton] PlatformerWindowManager not found! Make sure PlatformerWindowSetup is in the scene.");
        }
    }

    /// <summary>
    /// Call this to close the platformer window
    /// </summary>
    public void ClosePlatformer()
    {
        PlatformerWindowManager manager = PlatformerWindowManager.Instance;
        if (manager == null)
        {
            manager = FindObjectOfType<PlatformerWindowManager>();
        }

        if (manager != null)
        {
            manager.CloseWindow();
        }
    }
}
