using UnityEngine;

/// <summary>
/// Makes a UI element bounce around the screen like the classic DVD logo screensaver.
/// Attach to the window panel's RectTransform.
/// </summary>
public class DVDBounceEffect : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 100f;
    [SerializeField] private Vector2 direction = new Vector2(1, 1).normalized;

    [Header("Color Change (Optional)")]
    [SerializeField] private bool changeColorOnBounce = true;
    [SerializeField] private UnityEngine.UI.Image[] imagesToColor;

    private RectTransform rectTransform;
    private RectTransform parentRect;
    private Vector2 velocity;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRect = transform.parent as RectTransform;

        // Set initial velocity
        velocity = direction.normalized * speed;
    }

    private void Start()
    {
        // Auto-find images to color if not set (do this in Start so children exist)
        if (imagesToColor == null || imagesToColor.Length == 0)
        {
            imagesToColor = GetComponentsInChildren<UnityEngine.UI.Image>();
        }
    }

    private void OnEnable()
    {
        // Randomize direction when enabled
        float angle = Random.Range(30f, 60f); // Diagonal-ish angle
        if (Random.value > 0.5f) angle = -angle;
        if (Random.value > 0.5f) angle += 180f;

        direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        velocity = direction * speed;
    }

    private void Update()
    {
        if (parentRect == null) return;

        // Move the window
        Vector2 currentPos = rectTransform.anchoredPosition;
        Vector2 newPos = currentPos + velocity * Time.unscaledDeltaTime;

        // Get bounds
        Vector2 parentSize = parentRect.rect.size;
        Vector2 windowSize = rectTransform.rect.size;

        // Calculate boundaries (accounting for anchors at center)
        float minX = -parentSize.x / 2 + windowSize.x / 2;
        float maxX = parentSize.x / 2 - windowSize.x / 2;
        float minY = -parentSize.y / 2 + windowSize.y / 2;
        float maxY = parentSize.y / 2 - windowSize.y / 2;

        bool bounced = false;

        // Bounce off edges
        if (newPos.x <= minX)
        {
            newPos.x = minX;
            velocity.x = Mathf.Abs(velocity.x);
            bounced = true;
        }
        else if (newPos.x >= maxX)
        {
            newPos.x = maxX;
            velocity.x = -Mathf.Abs(velocity.x);
            bounced = true;
        }

        if (newPos.y <= minY)
        {
            newPos.y = minY;
            velocity.y = Mathf.Abs(velocity.y);
            bounced = true;
        }
        else if (newPos.y >= maxY)
        {
            newPos.y = maxY;
            velocity.y = -Mathf.Abs(velocity.y);
            bounced = true;
        }

        // Apply new position
        rectTransform.anchoredPosition = newPos;

        // Change color on bounce
        if (bounced && changeColorOnBounce)
        {
            ChangeColor();
        }
    }

    private void ChangeColor()
    {
        Color newColor = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.8f, 1f);

        if (imagesToColor != null && imagesToColor.Length > 0)
        {
            foreach (var img in imagesToColor)
            {
                if (img != null)
                {
                    img.color = newColor;
                }
            }
        }
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        velocity = velocity.normalized * speed;
    }
}
