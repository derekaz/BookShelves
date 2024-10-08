﻿namespace BookShelves.Maui.Data;

public interface IDataService
{
    Task<IEnumerable<T>> GetItemsWithQuery<T>(string query) where T : BaseTable, new();
    Task<IEnumerable<T>> GetItems<T>() where T : BaseTable, new();
    Task<bool> ExecuteQuery(string query);
    Task<int> CountItemsWithQuery(string query);
}
