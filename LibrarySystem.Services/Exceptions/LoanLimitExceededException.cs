namespace LibrarySystem.Services.Exceptions;

public class LoanLimitExceededException : Exception
{
    public LoanLimitExceededException()
        : base("Member has reached the maximum loan limit of 3 active loans.")
    {
    }

    public LoanLimitExceededException(string message)
        : base(message)
    {
    }
}
