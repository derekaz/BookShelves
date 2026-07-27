using BookShelves.WebApi.BooksDataAccess;
using CommunityToolkit.Datasync.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShelves.WebApi.Controllers;

[Authorize]
[Route("tables/[controller]")]
[TypeFilter(typeof(DatasyncDebugExceptionFilter), Order = int.MinValue)]
public class BooksController : TableController<Book>
{
    public BooksController(IRepository<Book> repository, IHttpContextAccessor accessor, ILogger<BooksController> logger) : base(repository)
    {
        AccessControlProvider = new BooksAccessControlProvider(accessor);
        Logger = logger;

        Options = new TableControllerOptions
        {
            UnsafeEntityLogging = true
        };
    }
}