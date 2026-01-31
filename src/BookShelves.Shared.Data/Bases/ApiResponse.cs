using System.Net;

namespace BookShelves.Shared.Data.Bases;

public class ApiResponse
{
    public ApiResponse() { }

    private ApiResponse(bool isSuccess, HttpStatusCode httpStatusCode, string? message, string? error)
    {
        IsSuccess = isSuccess;
        StatusCode = httpStatusCode;
        Message = message;
        Error = error;
    }

    /// <summary>
    /// The HttpStatusCode value of this response.
    /// </summary>
    public HttpStatusCode StatusCode { get; init; } = HttpStatusCode.Ambiguous;

    /// <summary>
    /// Indicates whether the request was successful.
    /// </summary>
    public bool IsSuccess { get; init; } = true;

    /// <summary>
    /// Optional message providing additional information.
    /// </summary>
    public string? Message { get; private set; }

    /// <summary>
    /// Optional error providing additional specific error details
    /// </summary>
    public string? Error { get; init; }

    public ApiResponse WithMessage(string? message)
    {
        Message = message;
        return this;
    }

    public static ApiResponse Success(string? message = "Request successful.")
    {
        return new ApiResponse(true, HttpStatusCode.OK, message ?? "Request successful.", null);
    }

    public static ApiResponse Failed(HttpStatusCode code, string error, string? message = null)
    {
        return new ApiResponse(false, code, message, error);
    }
}

public class ApiResponse<T>
{
    /// <summary>
    /// The HttpStatusCode value of this response.
    /// </summary>
    public HttpStatusCode StatusCode { get; init; } = HttpStatusCode.Ambiguous;

    /// <summary>
    /// Indicates whether the request was successful.
    /// </summary>
    public bool IsSuccess { get; init; } = true;

    /// <summary>
    /// The data payload returned by the API.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Optional message providing additional information.
    /// </summary>
    public string? Message { get; private set; }

    /// <summary>
    /// Optional error providing additional specific error details
    /// </summary>
    public string? Error { get; init; }

    public ApiResponse() { }

    private ApiResponse(bool isSuccess, HttpStatusCode httpStatusCode, string message, string? error, T? data)
    {
        IsSuccess = isSuccess;
        StatusCode = httpStatusCode;
        Message = message;
        Error = error;
        Data = data;
    }

    public ApiResponse<T> WithMessage(string? message)
    {
        Message = message;
        return this;
    }

    // Factory method for successful responses
    public static ApiResponse<T> Success(T data, string? message = "Request successful.")
    {
        return new ApiResponse<T>(true, HttpStatusCode.OK, message ?? "Request successful.", null, data);
    }

    public static ApiResponse<T> Failed(HttpStatusCode code, T data, string error, string? message = "Request failed.")
    {
        return new ApiResponse<T>(false, code, message ?? $"Request failed. {code}", error, data);
    }

    // Optional: Factory method for "success with no content" responses
    public static ApiResponse<T> SuccessNoContent(string? message = "Request successful, no content returned.")
    {
        return new ApiResponse<T>(true, HttpStatusCode.OK, message ?? "Request successful, no content returned.", null, default);
    }
}
