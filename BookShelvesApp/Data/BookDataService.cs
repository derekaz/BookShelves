using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
    }
}
