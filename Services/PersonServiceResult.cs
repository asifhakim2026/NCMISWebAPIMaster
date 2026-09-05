namespace NCMISAPI.Services;

public class PersonServiceResult
{
    public int StatusCode { get; init; } = 200;
    public object? Body { get; init; }

    public static PersonServiceResult Ok(object? body) => new() { StatusCode = 200, Body = body };
    public static PersonServiceResult BadRequest(object? body) => new() { StatusCode = 400, Body = body };
    public static PersonServiceResult Error(object? body) => new() { StatusCode = 500, Body = body };
}

