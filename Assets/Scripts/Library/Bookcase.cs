using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A bookcase that can hold a book or be corrupted by an enemy.
/// Player can interact with it by pressing E when nearby.
/// </summary>
public class Bookcase : MonoBehaviour
{
    [Header("Book Storage")]
    [Tooltip("The book currently on this shelf (null if empty)")]
    [SerializeField] private BookData currentBook;

    [Tooltip("What genre this shelf is meant to hold")]
    [SerializeField] private BookGenre expectedGenre;

    [Header("State")]
    [SerializeField] private bool isCorrupted = false;
    [SerializeField] private bool isEmpty = true;

    [Header("Visuals (Optional - for your friend)")]
    [SerializeField] private GameObject normalVisual;
    [SerializeField] private GameObject corruptedVisual;
    [SerializeField] private GameObject bookVisual;

    [Header("Events")]
    public UnityEvent<BookData> OnBookPlaced;
    public UnityEvent<BookData> OnBookRemoved;
    public UnityEvent OnCorrupted;
    public UnityEvent OnCleansed;

    // Properties
    public BookData CurrentBook => currentBook;
    public BookGenre ExpectedGenre => expectedGenre;
    public bool IsCorrupted => isCorrupted;
    public bool IsEmpty => isEmpty;
    public bool HasCorrectBook => currentBook != null && currentBook.genre == expectedGenre;

    private void Start()
    {
        UpdateVisuals();
        isEmpty = currentBook == null;
    }

    /// <summary>
    /// Place a book on this shelf
    /// </summary>
    public bool PlaceBook(BookData book)
    {
        if (isCorrupted)
        {
            Debug.Log("[Bookcase] Cannot place book - shelf is corrupted!");
            return false;
        }

        if (!isEmpty)
        {
            Debug.Log("[Bookcase] Cannot place book - shelf already has a book!");
            return false;
        }

        currentBook = book;
        isEmpty = false;

        if (book.genre == expectedGenre)
        {
            Debug.Log($"[Bookcase] Correct! {book.bookName} belongs in {expectedGenre}!");
        }
        else
        {
            Debug.Log($"[Bookcase] {book.bookName} is {book.genre}, but this shelf is for {expectedGenre}");
        }

        OnBookPlaced?.Invoke(book);
        UpdateVisuals();
        return true;
    }

    /// <summary>
    /// Remove the book from this shelf
    /// </summary>
    public BookData RemoveBook()
    {
        if (isEmpty || currentBook == null)
        {
            Debug.Log("[Bookcase] No book to remove!");
            return null;
        }

        BookData removedBook = currentBook;
        currentBook = null;
        isEmpty = true;

        OnBookRemoved?.Invoke(removedBook);
        UpdateVisuals();
        return removedBook;
    }

    /// <summary>
    /// Corrupt this bookcase (called by enemies)
    /// </summary>
    public void Corrupt()
    {
        if (isCorrupted) return;

        isCorrupted = true;
        Debug.Log("[Bookcase] Bookcase has been corrupted!");

        OnCorrupted?.Invoke();
        UpdateVisuals();
    }

    /// <summary>
    /// Cleanse this bookcase of corruption
    /// </summary>
    public void Cleanse()
    {
        if (!isCorrupted) return;

        isCorrupted = false;
        Debug.Log("[Bookcase] Bookcase has been cleansed!");

        OnCleansed?.Invoke();
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (normalVisual != null)
            normalVisual.SetActive(!isCorrupted);

        if (corruptedVisual != null)
            corruptedVisual.SetActive(isCorrupted);

        if (bookVisual != null)
            bookVisual.SetActive(!isEmpty && currentBook != null);
    }

    /// <summary>
    /// Called when player interacts with this bookcase
    /// </summary>
    public void OnInteract(PlayerInventory inventory)
    {
        if (isCorrupted)
        {
            Debug.Log("[Bookcase] This shelf is corrupted! Cleanse it first.");
            return;
        }

        if (isEmpty)
        {
            // Try to place a book from inventory
            if (inventory != null && inventory.HasBook)
            {
                BookData bookToPlace = inventory.RemoveBook();
                if (bookToPlace != null)
                {
                    PlaceBook(bookToPlace);
                }
            }
            else
            {
                Debug.Log("[Bookcase] Empty shelf. You need a book to place here.");
            }
        }
        else
        {
            // Pick up the book
            BookData pickedBook = RemoveBook();
            if (pickedBook != null && inventory != null)
            {
                inventory.AddBook(pickedBook);
            }
        }
    }
}
