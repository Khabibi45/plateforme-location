namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Adapters;

public record DiscogsResult<T>
{
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public DiscogsErrorKind? ErrorKind { get; }

    public bool IsSuccess => ErrorKind is null;

    private DiscogsResult(T value)
    {
        Value = value;
    }

    private DiscogsResult(DiscogsErrorKind kind, string message)
    {
        ErrorKind = kind;
        ErrorMessage = message;
    }

    public static DiscogsResult<T> Success(T value) => new(value);

    public static DiscogsResult<T> NotFound(int id) =>
        new(DiscogsErrorKind.NotFound, $"Discogs resource with id {id} was not found.");

    public static DiscogsResult<T> ApiError(int statusCode, string reason) =>
        new(DiscogsErrorKind.ApiError, $"Discogs API returned {statusCode}: {reason}");

    public static DiscogsResult<T> NetworkError(string message) =>
        new(DiscogsErrorKind.NetworkError, message);

    public static DiscogsResult<T> DeserializationError(string message) =>
        new(DiscogsErrorKind.DeserializationError, message);
}

public enum DiscogsErrorKind
{
    NotFound,
    ApiError,
    NetworkError,
    DeserializationError
}