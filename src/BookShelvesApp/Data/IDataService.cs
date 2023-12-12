using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShelves.Data
{
    internal interface IDataService
    {
        Task<IEnumerable<T>> GetItemsWithQuery<T>(string query) where T : BaseTable, new();
        Task<IEnumerable<T>> GetItems<T>() where T : BaseTable, new();
        Task<bool> ExecuteQuery(string query);
        Task<int> CountItemsWithQuery(string query);
    }
}
