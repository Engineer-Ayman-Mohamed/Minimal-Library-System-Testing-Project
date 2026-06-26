namespace LibrarySystem.Services.Exceptions;

public class MembershipExpiredException : Exception
{
    public MembershipExpiredException()
        : base("Member's membership has expired. Cannot borrow books.")
    {
    }

    public MembershipExpiredException(string message)
        : base(message)
    {
    }
}
