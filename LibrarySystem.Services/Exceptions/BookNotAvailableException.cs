namespace LibrarySystem.Services.Exceptions;

public class BookNotAvailableException : Exception
{
    public BookNotAvailableException()
        : base("The book is not available for borrowing. No copies available.")
    {
    }

    public BookNotAvailableException(string message)
        : base(message)
    {
    }
}
