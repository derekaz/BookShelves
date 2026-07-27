using BookShelves.Maui.Data.Interfaces;

namespace BookShelves.Maui.Data.Services.Maui;

public sealed class SyncApiClient : ISyncApiClient
{
    public HttpClient HttpClient { get; }

    public SyncApiClient(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }
}
