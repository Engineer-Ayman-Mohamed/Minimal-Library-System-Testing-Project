namespace LibrarySystem.Services.Exceptions;

public class OutstandingFineException : Exception
{
    public OutstandingFineException()
        : base("Member has outstanding fines. Cannot borrow books until fines are paid.")
    {
    }

    public OutstandingFineException(string message)
        : base(message)
    {
    }
}
