namespace NCMISAPI.Exceptions;

/// <summary>
/// Business rule / expected failure that should return a controlled status code.
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = StatusCodes.Status400BadRequest)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
