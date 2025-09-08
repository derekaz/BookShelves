using BookShelves.Maui.Data.Models;
using BookShelves.Shared.DataInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace BookShelves.Maui.Data.Services
{
    public class BooksSyncService : IBooksSyncService
    {
        private readonly HttpClient _httpClient;
        private readonly IBooksDataService _booksDataService;

        public BooksSyncService(HttpClient httpClient, IBooksDataService booksDataService) 
        {
            _httpClient = httpClient;
            _booksDataService = booksDataService;
        }

        public void BeginSync()
        {
            GetLastSyncTime();
            GetBooksAsync();
        }
        
        private void GetLastSyncTime()
        {
            DateTime latestUpdateTime = DateTime.MinValue;
            var books = _booksDataService.GetBooksAsync().Result;
            var maxUpdateTime = books.Max(book => book.LastUpdateTime ?? DateTime.MinValue);
        }

        private async void GetBooksAsync()
        {
            try
            {
                // _httpClient.BaseAddress = new Uri("https://bookshelves.cloud.azmoore.com");
                _httpClient.BaseAddress = new Uri("https://green-ground-05694281e-dev013.westus2.2.azurestaticapps.net");
                var temp = await _httpClient.GetStringAsync("/api/books/sync?lastSyncTime=1970-01-01");
                var books = System.Text.Json.JsonSerializer.Deserialize<List<Book>>(temp!);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                // _httpClient.Dispose();
            }
        }
    }
}
