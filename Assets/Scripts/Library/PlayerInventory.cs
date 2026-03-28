using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Handles the player's book inventory.
/// Attach to the Player GameObject.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [Tooltip("Maximum number of books the player can carry")]
    [SerializeField] private int maxBooks = 5;

    [Header("Current Inventory")]
    [SerializeField] private List<BookData> books = new List<BookData>();

    [Header("Events")]
    public UnityEvent<BookData> OnBookAdded;
    public UnityEvent<BookData> OnBookRemoved;
    public UnityEvent OnInventoryFull;

    // Properties
    public bool HasBook => books.Count > 0;
    public bool IsFull => books.Count >= maxBooks;
    public int BookCount => books.Count;
    public IReadOnlyList<BookData> Books => books;

    /// <summary>
    /// Add a book to inventory
    /// </summary>
    public bool AddBook(BookData book)
    {
        if (book == null) return false;

        if (IsFull)
        {
            Debug.Log("[Inventory] Inventory is full!");
            OnInventoryFull?.Invoke();
            return false;
        }

        books.Add(book);
        Debug.Log($"[Inventory] Picked up: {book.bookName}");
        OnBookAdded?.Invoke(book);
        return true;
    }

    /// <summary>
    /// Remove and return the most recently added book
    /// </summary>
    public BookData RemoveBook()
    {
        if (!HasBook) return null;

        int lastIndex = books.Count - 1;
        BookData book = books[lastIndex];
        books.RemoveAt(lastIndex);

        Debug.Log($"[Inventory] Removed: {book.bookName}");
        OnBookRemoved?.Invoke(book);
        return book;
    }

    /// <summary>
    /// Remove a specific book from inventory
    /// </summary>
    public bool RemoveBook(BookData book)
    {
        if (books.Remove(book))
        {
            Debug.Log($"[Inventory] Removed: {book.bookName}");
            OnBookRemoved?.Invoke(book);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Check if inventory contains a specific book
    /// </summary>
    public bool HasSpecificBook(BookData book)
    {
        return books.Contains(book);
    }

    /// <summary>
    /// Check if inventory contains a book of a specific genre
    /// </summary>
    public bool HasBookOfGenre(BookGenre genre)
    {
        foreach (var book in books)
        {
            if (book.genre == genre) return true;
        }
        return false;
    }

    /// <summary>
    /// Get first book of a specific genre
    /// </summary>
    public BookData GetBookOfGenre(BookGenre genre)
    {
        foreach (var book in books)
        {
            if (book.genre == genre) return book;
        }
        return null;
    }

    /// <summary>
    /// Clear all books from inventory
    /// </summary>
    public void ClearInventory()
    {
        books.Clear();
        Debug.Log("[Inventory] Inventory cleared");
    }
}
