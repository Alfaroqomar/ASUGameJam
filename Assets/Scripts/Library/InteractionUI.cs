using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI to show interaction prompts and current held book.
/// </summary>
public class InteractionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private PlayerInventory playerInventory;

    [Header("UI Elements (Optional)")]
    [SerializeField] private Text promptText;
    [SerializeField] private Text inventoryText;
    [SerializeField] private GameObject promptPanel;

    private void Update()
    {
        UpdatePrompt();
        UpdateInventoryDisplay();
    }

    private void UpdatePrompt()
    {
        if (playerInteraction == null) return;

        string prompt = playerInteraction.GetInteractionPrompt();

        if (promptText != null)
        {
            promptText.text = prompt ?? "";
        }

        if (promptPanel != null)
        {
            promptPanel.SetActive(!string.IsNullOrEmpty(prompt));
        }
    }

    private void UpdateInventoryDisplay()
    {
        if (playerInventory == null || inventoryText == null) return;

        if (playerInventory.HasBook)
        {
            var books = playerInventory.Books;
            string display = $"Carrying ({playerInventory.BookCount}): ";
            for (int i = 0; i < books.Count; i++)
            {
                if (i > 0) display += ", ";
                display += books[i].bookName;
            }
            inventoryText.text = display;
        }
        else
        {
            inventoryText.text = "No books";
        }
    }
}
