using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Central database for all books in the library.
/// Can be used to query books by genre, author, etc.
/// </summary>
public class LibraryDatabase : MonoBehaviour
{
    public static LibraryDatabase Instance { get; private set; }

    [Header("Book Database")]
    [Tooltip("All books available in the game. Add BookData assets here.")]
    [SerializeField] private List<BookData> allBooks = new List<BookData>();

    [Header("Runtime Tracking")]
    [SerializeField] private List<Bookcase> allBookcases = new List<Bookcase>();

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
        // Find all bookcases in the scene
        allBookcases = FindObjectsOfType<Bookcase>().ToList();
        Debug.Log($"[LibraryDatabase] Found {allBookcases.Count} bookcases and {allBooks.Count} books in database");
    }

    /// <summary>
    /// Get all books of a specific genre
    /// </summary>
    public List<BookData> GetBooksByGenre(BookGenre genre)
    {
        return allBooks.Where(b => b.genre == genre).ToList();
    }

    /// <summary>
    /// Get all books by a specific author
    /// </summary>
    public List<BookData> GetBooksByAuthor(string authorName)
    {
        return allBooks.Where(b => b.authorName.ToLower().Contains(authorName.ToLower())).ToList();
    }

    /// <summary>
    /// Search books by name
    /// </summary>
    public List<BookData> SearchBooks(string searchTerm)
    {
        string term = searchTerm.ToLower();
        return allBooks.Where(b =>
            b.bookName.ToLower().Contains(term) ||
            b.authorName.ToLower().Contains(term) ||
            b.blurb.ToLower().Contains(term)
        ).ToList();
    }

    /// <summary>
    /// Get a random book from the database
    /// </summary>
    public BookData GetRandomBook()
    {
        if (allBooks.Count == 0) return null;
        return allBooks[Random.Range(0, allBooks.Count)];
    }

    /// <summary>
    /// Get a random book of a specific genre
    /// </summary>
    public BookData GetRandomBookOfGenre(BookGenre genre)
    {
        var genreBooks = GetBooksByGenre(genre);
        if (genreBooks.Count == 0) return null;
        return genreBooks[Random.Range(0, genreBooks.Count)];
    }

    /// <summary>
    /// Check how many books are correctly shelved
    /// </summary>
    public int GetCorrectlyPlacedBookCount()
    {
        return allBookcases.Count(bc => bc.HasCorrectBook);
    }

    /// <summary>
    /// Check how many bookcases are corrupted
    /// </summary>
    public int GetCorruptedBookcaseCount()
    {
        return allBookcases.Count(bc => bc.IsCorrupted);
    }

    /// <summary>
    /// Get completion percentage (correct books / total bookcases)
    /// </summary>
    public float GetCompletionPercentage()
    {
        if (allBookcases.Count == 0) return 0f;
        return (float)GetCorrectlyPlacedBookCount() / allBookcases.Count * 100f;
    }

    /// <summary>
    /// Register a bookcase with the database
    /// </summary>
    public void RegisterBookcase(Bookcase bookcase)
    {
        if (!allBookcases.Contains(bookcase))
        {
            allBookcases.Add(bookcase);
        }
    }

    /// <summary>
    /// Add a book to the database at runtime
    /// </summary>
    public void AddBook(BookData book)
    {
        if (!allBooks.Contains(book))
        {
            allBooks.Add(book);
        }
    }
}
