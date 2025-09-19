using BookShelves.Maui.Data.Models;
using BookShelves.Shared.DataInterfaces;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BookShelves.Maui.Data.Services
{
    public class BooksSyncService(IHttpClientFactory httpClientFactory, IBooksDataService booksDataService, ILogger<BooksSyncService> logger) : IBooksSyncService
    {
        private readonly TestBooksService _localBooksDataService = (TestBooksService)booksDataService;
        // private readonly IBooksDataService _localBooksDataService = (BooksDataService)booksDataService;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ILogger<BooksSyncService> _logger = logger;

        public void BeginSync()
        {
            GetLastSyncTime();
            var updatedServerBooks = GetServerBooksAsync().Result;
            UpdateLocalStoreAsync(updatedServerBooks);
            var updatedLocalBooks = GetLocalBooksAsync().Result;
            UpdateServerStoreAsync(updatedLocalBooks);
        }

        readonly Expression<Func<LocalBook, bool>> changedBooks = p => p.UpdateType == "C" || p.UpdateType == "U" || p.UpdateType == "D";

        private async Task<IEnumerable<LocalBook>> GetLocalBooksAsync()
        {
            var books = await _localBooksDataService.GetBooksAsync(changedBooks);
            return books;
        }

        private async void UpdateServerStoreAsync(IEnumerable<LocalBook> updatedLocalBooks)
        {
            var httpClient = _httpClientFactory.CreateClient("BooksApi");
            var currentServerBooks = await httpClient.GetFromJsonAsync<RemoteBook[]>("/api/books") ?? [];

            foreach (var updatedBook in updatedLocalBooks)
            {
                if (updatedBook.ServerId == null || updatedBook.ServerId == 0)
                {
                    var newServerBook = new RemoteBook()
                    {
                        Id = "0",
                        Title = updatedBook.Title,
                        Author = updatedBook.Author,
                        Revision = updatedBook.Revision,
                        LastUpdateTime = updatedBook.LastUpdateTime
                    };

                    using StringContent jsonContent = new(
                        System.Text.Json.JsonSerializer.Serialize(newServerBook),
                        Encoding.UTF8,
                        "application/json");

                    var result = await httpClient.PostAsync("api/v2/books/new", jsonContent);

                    try
                    {
                        var content = await result.Content.ReadAsStringAsync();
                        // var response = JsonConvert.DeserializeObject<ApiResponse<RemoteBook>>(content);
                        var response = JsonSerializer.Deserialize<ApiResponse<RemoteBook>>(content);

                        // update the local version and save it
                        if (response != null && response.IsSuccess)
                        {
                            updatedBook.ServerId = int.Parse(response?.Data?.Id ?? "0");
                            updatedBook.UpdateType = string.Empty;

                            if (updatedBook.ServerId != 0)
                            {
                                var localUpdateResult = await _localBooksDataService.UpdateBookFromSyncAsync(updatedBook);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unable to deserialize ApiResponse");
                        throw;
                    }

                }
                else if (updatedBook.ServerId != null && updatedBook.ServerId != 0)
                {
                    var currentServerBook = currentServerBooks.FirstOrDefault(b => (b?.Id ?? "0") == updatedBook.ServerId.ToString());
                    if (currentServerBook != null) // assume that there is no book to update
                    {
                        if (updatedBook.UpdateType == "U" || updatedBook.UpdateType == "C")
                        {
                            currentServerBook.Title = updatedBook.Title;
                            currentServerBook.Author = updatedBook.Author;
                            currentServerBook.LastUpdateTime = updatedBook.LastUpdateTime;
                            currentServerBook.Revision = updatedBook.Revision;

                            using StringContent jsonContent = new(
                                System.Text.Json.JsonSerializer.Serialize(currentServerBook),
                                Encoding.UTF8,
                                "application/json");

                            var temp = await httpClient.PostAsync("api/v2/books/edit", jsonContent);
                        }
                        else if (updatedBook.UpdateType == "D")
                        {
                            var temp = await httpClient.DeleteAsync($"/api/book/{updatedBook.ServerId}");
                        }
                    }
                }
            }
        }

        private void GetLastSyncTime()
        {
            DateTime latestUpdateTime = DateTime.MinValue;
            var books = _localBooksDataService.GetBooksAsync().Result;
            var maxUpdateTime = books.Max(book => book.LastUpdateTime ?? DateTime.MinValue);
        }

        private async Task<List<RemoteBook>> GetServerBooksAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("BooksApi");
            try
            {
                var temp = await httpClient.GetAsync("/api/books/sync?lastSyncTime=2025-09-10", HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                var data = await temp.Content.ReadAsStringAsync();
                var response = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<List<RemoteBook>>>(data)
                    ?? throw new ApplicationException("Unable to parse returned object");
                
                if (response.IsSuccess && response.StatusCode == System.Net.HttpStatusCode.OK && response.Data != null)
                {
                    return response.Data;
                }
                else
                {
                    return [];
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to returieve changed books from server");
                throw new ApplicationException("Unable to retrieve changed books from server");
            }
            finally
            {
                httpClient.Dispose();
            }
        }

        private async void UpdateLocalStoreAsync(List<RemoteBook> updatedBooks)
        {
            try
            {
                foreach (var book in updatedBooks)
                {
                    if (book.Id != null)
                    {
                        var currentBook = await _localBooksDataService.GetBookWithServerIdAsync(int.Parse(book.Id));

                        if (currentBook != null && currentBook.LastUpdateTime != book.LastUpdateTime)
                        {
                            // the assumption here is the server version overwrites any local changes...
                            currentBook.Title = book.Title;
                            currentBook.Author = book.Author;
                            currentBook.ServerId = int.Parse(book.Id);
                            currentBook.LastUpdateTime = book.LastUpdateTime;
                            currentBook.Revision = book.Revision;
                            currentBook.UpdateType = string.Empty;

                            await _localBooksDataService.UpdateBookFromSyncAsync(currentBook);
                        }
                        else if (currentBook == null)
                        {
                            currentBook = (LocalBook)_localBooksDataService.InitializeBookInstance();

                            currentBook.Title = book.Title;
                            currentBook.Author = book.Author;
                            currentBook.ServerId = int.Parse(book.Id);
                            currentBook.LastUpdateTime = book.LastUpdateTime;
                            currentBook.Revision = book.Revision;
                            currentBook.UpdateType = string.Empty;

                            await _localBooksDataService.CreateBookFromSyncAsync(currentBook);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to update changed books with server version");
                throw new ApplicationException("Unable to update changed books with server version");
            }
            finally
            {

            }        
        }
    }
}
