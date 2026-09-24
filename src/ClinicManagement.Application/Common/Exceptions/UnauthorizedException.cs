namespace ClinicManagement.Application.Common.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Authentication failed or token is invalid.") : base(message)
    {
    }
}
