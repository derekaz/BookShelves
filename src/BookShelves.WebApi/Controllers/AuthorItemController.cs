using BookShelves.WebApi.AuthorsDataAccess;
using CommunityToolkit.Datasync.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShelves.WebApi.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class AuthorItemController : TableController<AuthorItem>
    {
        public AuthorItemController(IRepository<AuthorItem> repository, IHttpContextAccessor accessor) : base(repository)
        {
            AccessControlProvider = new AuthorsAccessControlProvider(accessor);

            Options = new TableControllerOptions
            {
                UnsafeEntityLogging = false
            };
        }
    }
}
