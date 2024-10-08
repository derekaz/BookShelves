﻿using BookShelves.Shared.DataInterfaces;

namespace BookShelves.Maui.Data;

public class BooksDataService(IDataService dataService) : IBooksDataService
{
    readonly IDataService dataService = dataService;

    public IBook InitializeBookInstance()
    {
        return new Book();
    }

    public async Task<IEnumerable<IBook>> GetBooksAsync()
    {
        return await dataService.GetItemsWithQuery<Book>(Constants.AllBooksQuery);
    }

    public async Task<bool> DeleteBookAsync(IBook book)
    {
        return await dataService.ExecuteQuery($"delete from {Constants.BookTable} where Id = {book.Id};");
    }

    public async Task<bool> CreateBookAsync(IBook book)
    {
        return await dataService.ExecuteQuery($"insert into {Constants.BookTable}(Title, Author) VALUES ('{book.Title}', '{book.Author}');");
    }

    public async Task<bool> UpdateBookAsync(IBook book)
    {
        return await dataService.ExecuteQuery($"update {Constants.BookTable} SET Title='{book.Title}', Author='{book.Author}' WHERE Id='{book.Id}';");
    }
}
