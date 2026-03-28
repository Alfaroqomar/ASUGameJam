using UnityEngine;

/// <summary>
/// Handles player interaction with bookcases and other interactables.
/// Attach to the Player GameObject.
/// Press E to interact with nearby bookcases.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("References")]
    [SerializeField] private PlayerInventory inventory;

    [Header("Debug")]
    [SerializeField] private Bookcase nearbyBookcase;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponent<PlayerInventory>();
        }
    }

    private void Update()
    {
        // Check for nearby bookcases
        CheckForNearbyBookcase();

        // Handle interaction input
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    private void CheckForNearbyBookcase()
    {
        // Find all colliders in range
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRange, interactableLayer);

        Bookcase closest = null;
        float closestDist = float.MaxValue;

        foreach (var col in colliders)
        {
            Bookcase bookcase = col.GetComponent<Bookcase>();
            if (bookcase != null)
            {
                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = bookcase;
                }
            }
        }

        nearbyBookcase = closest;
    }

    private void TryInteract()
    {
        if (nearbyBookcase != null)
        {
            Debug.Log("[Interaction] Interacting with bookcase");
            nearbyBookcase.OnInteract(inventory);
        }
        else
        {
            Debug.Log("[Interaction] Nothing to interact with nearby");
        }
    }

    /// <summary>
    /// Check if player can currently interact with something
    /// </summary>
    public bool CanInteract()
    {
        return nearbyBookcase != null;
    }

    /// <summary>
    /// Get info about what's nearby for UI prompts
    /// </summary>
    public string GetInteractionPrompt()
    {
        if (nearbyBookcase == null) return null;

        if (nearbyBookcase.IsCorrupted)
        {
            return "Corrupted Shelf";
        }
        else if (nearbyBookcase.IsEmpty)
        {
            if (inventory != null && inventory.HasBook)
            {
                return "Press E to place book";
            }
            return "Empty Shelf";
        }
        else
        {
            return $"Press E to take: {nearbyBookcase.CurrentBook?.bookName}";
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize interact range in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
