using BookShelves.WebApi.AuthorsDataAccess;
using CommunityToolkit.Datasync.Server;
using Microsoft.AspNetCore.Mvc;

namespace BookShelves.WebApi.Controllers
{
    [Route("[controller]")]
    public class AuthorItemController : TableController<AuthorItem>
    {
        public AuthorItemController(IRepository<AuthorItem> repository) : base(repository) 
        {
            Options = new TableControllerOptions
            {
                UnsafeEntityLogging = false
            };
        }
    }
}
