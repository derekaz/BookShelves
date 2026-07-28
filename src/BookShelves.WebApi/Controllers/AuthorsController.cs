using BookShelves.WebApi.AuthorsDataAccess;
using CommunityToolkit.Datasync.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShelves.WebApi.Controllers
{
    [Authorize]
    [Route("tables/[controller]")]
    [TypeFilter(typeof(DatasyncDebugExceptionFilter), Order = int.MinValue)]
    public class AuthorsController : TableController<Author>
    {
        public AuthorsController(IRepository<Author> repository, IHttpContextAccessor accessor, ILogger<AuthorsController> logger) : base(repository)
        {
            AccessControlProvider = new AuthorsAccessControlProvider(accessor);
            Logger = logger;

            Options = new TableControllerOptions
            {
                UnsafeEntityLogging = true
            };
        }
    }
}
