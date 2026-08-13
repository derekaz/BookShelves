using System.Net;
using BookShelves.Shared.Data.Bases;

namespace BookShelves.Shared.Tests.Data;

public sealed class ApiResponseTests
{
    // ── ApiResponse (non-generic) ──────────────────────────────────────────────

    [Fact]
    public void Success_SetsIsSuccessTrue()
    {
        var response = ApiResponse.Success();

        Assert.True(response.IsSuccess);
    }

    [Fact]
    public void Success_SetsStatusCodeOk()
    {
        var response = ApiResponse.Success();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public void Success_UsesDefaultMessage_WhenNoneProvided()
    {
        var response = ApiResponse.Success();

        Assert.Equal("Request successful.", response.Message);
    }

    [Fact]
    public void Success_UsesProvidedMessage()
    {
        var response = ApiResponse.Success("Custom message");

        Assert.Equal("Custom message", response.Message);
    }

    [Fact]
    public void Failed_SetsIsSuccessFalse()
    {
        var response = ApiResponse.Failed(HttpStatusCode.NotFound, "not found");

        Assert.False(response.IsSuccess);
    }

    [Fact]
    public void Failed_SetsStatusCode()
    {
        var response = ApiResponse.Failed(HttpStatusCode.BadRequest, "bad request");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public void Failed_SetsError()
    {
        var response = ApiResponse.Failed(HttpStatusCode.InternalServerError, "server error");

        Assert.Equal("server error", response.Error);
    }

    [Fact]
    public void WithMessage_UpdatesMessage()
    {
        var response = ApiResponse.Success("original");
        response.WithMessage("updated");

        Assert.Equal("updated", response.Message);
    }

    // ── ApiResponse<T> ────────────────────────────────────────────────────────

    [Fact]
    public void GenericSuccess_SetsIsSuccessTrue()
    {
        var response = ApiResponse<string>.Success("data");

        Assert.True(response.IsSuccess);
    }

    [Fact]
    public void GenericSuccess_SetsStatusCodeOk()
    {
        var response = ApiResponse<string>.Success("data");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public void GenericSuccess_SetsData()
    {
        var response = ApiResponse<int>.Success(42);

        Assert.Equal(42, response.Data);
    }

    [Fact]
    public void GenericFailed_SetsIsSuccessFalse()
    {
        var response = ApiResponse<string>.Failed(HttpStatusCode.NotFound, string.Empty, "not found");

        Assert.False(response.IsSuccess);
    }

    [Fact]
    public void GenericFailed_SetsStatusCode()
    {
        var response = ApiResponse<string>.Failed(HttpStatusCode.Forbidden, string.Empty, "forbidden");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public void GenericFailed_SetsError()
    {
        var response = ApiResponse<string>.Failed(HttpStatusCode.BadRequest, string.Empty, "validation error");

        Assert.Equal("validation error", response.Error);
    }

    [Fact]
    public void GenericSuccessNoContent_SetsIsSuccessTrue_WithNullData()
    {
        var response = ApiResponse<string>.SuccessNoContent();

        Assert.True(response.IsSuccess);
        Assert.Null(response.Data);
    }

    [Fact]
    public void GenericWithMessage_UpdatesMessage()
    {
        var response = ApiResponse<int>.Success(1, "original");
        response.WithMessage("updated");

        Assert.Equal("updated", response.Message);
    }
}
