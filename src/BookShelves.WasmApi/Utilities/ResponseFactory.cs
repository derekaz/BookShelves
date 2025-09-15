using BookShelves.Shared.DataInterfaces;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Net;
using System.Threading.Tasks;

namespace BookShelves.WasmApi.Utilities;

internal class ResponseFactory
{
    public async static Task<HttpResponseData> CreateSuccessResponseAsync<T>(HttpRequestData request, string responseMessage, T? data = default)
    {
        if (data == null)
        {
            var responsePayload = ApiResponse.Success(responseMessage);
            var response = request.CreateResponse(responsePayload.StatusCode);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            await response.WriteStringAsync(System.Text.Json.JsonSerializer.Serialize(responsePayload));

            return response;
        }
        else
        {
            var responsePayload = ApiResponse<T>.Success(data, responseMessage);
            var response = request.CreateResponse(responsePayload.StatusCode);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            await response.WriteStringAsync(System.Text.Json.JsonSerializer.Serialize(responsePayload));

            return response;
        }
    }

    public async static Task<HttpResponseData> CreateFailedResponseNoContentAsync(HttpRequestData request, HttpStatusCode statusCode = HttpStatusCode.UnprocessableEntity, string responseMessage = "Unable to process request.", Exception? ex = null, string? error = null)
    {
        var responsePayload =
            ApiResponse.Failed(statusCode, ex?.Message ?? error ?? responseMessage, responseMessage);

        var response1 = request.CreateResponse(responsePayload.StatusCode);
        response1.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response1.WriteStringAsync(System.Text.Json.JsonSerializer.Serialize(responsePayload));

        return response1;
    }

    public async static Task<HttpResponseData> CreateFailedResponseAsync<T>(HttpRequestData request, T data, HttpStatusCode statusCode = HttpStatusCode.UnprocessableEntity, string responseMessage = "Unable to process request.", Exception? ex = null, string? error = null)
    {
        var responsePayload =
            ApiResponse<T>.Failed(statusCode, data, ex?.Message ?? error ?? responseMessage, responseMessage);

        var response1 = request.CreateResponse(responsePayload.StatusCode);
        response1.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response1.WriteStringAsync(System.Text.Json.JsonSerializer.Serialize(responsePayload));

        return response1;
    }
}
