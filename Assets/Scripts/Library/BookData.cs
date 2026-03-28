using UnityEngine;

/// <summary>
/// ScriptableObject containing book information.
/// Create new books via: Right-click in Project > Create > Library > Book
/// </summary>
[CreateAssetMenu(fileName = "NewBook", menuName = "Library/Book")]
public class BookData : ScriptableObject
{
    [Header("Book Information")]
    public string bookName = "Untitled Book";
    public string authorName = "Unknown Author";
    public BookGenre genre = BookGenre.NonFiction;

    [TextArea(3, 6)]
    public string blurb = "A fascinating read...";

    [Header("Optional")]
    public Sprite coverImage;

    public override string ToString()
    {
        return $"{bookName} by {authorName} ({genre})";
    }
}
