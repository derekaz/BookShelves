using Microsoft.Maui.ApplicationModel.DataTransfer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BookShelves.Data
{
    internal class BookDataService
    {
        IDataService dataService;

        public BookDataService(IDataService dataService) 
        {
            this.dataService = dataService;                
        }

        public async Task<IEnumerable<Book>> GetBooksAsync()
        {
            return await dataService.GetItemsWithQuery<Book>(Constants.AllBooksQuery);
        }

        public async Task<Boolean> DeleteBook(int id)
        {
            return await dataService.ExecuteQuery($"delete from {Constants.BookTable} where Id = {id};");
        }

        public async Task<bool> AddBook(Book book)
        {
            return await dataService.ExecuteQuery($"insert into {Constants.BookTable}(Title, Author) VALUES ('{book.Title}', '{book.Author}');");
        }

        public async Task<bool> UpdateBook(Book book)
        {
            return await dataService.ExecuteQuery($"update {Constants.BookTable} SET Title='{book.Title}', Author='{book.Author}' WHERE Id='{book.Id}';");
        }
    }
}
