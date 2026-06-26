namespace LibrarySystem.Services.Exceptions;

public class AlreadyReturnedException : Exception
{
    public AlreadyReturnedException()
        : base("This loan has already been returned.")
    {
    }

    public AlreadyReturnedException(string message)
        : base(message)
    {
    }
}
