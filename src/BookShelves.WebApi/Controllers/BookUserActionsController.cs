using BookShelves.WebApi.BookUserActionsDataAccess;
using CommunityToolkit.Datasync.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShelves.WebApi.Controllers;

[Authorize]
[Route("tables/[controller]")]
[TypeFilter(typeof(DatasyncDebugExceptionFilter), Order = int.MinValue)]
public class BookUserActionsController : TableController<BookUserAction>
{
    public BookUserActionsController(IRepository<BookUserAction> repository, IHttpContextAccessor accessor, ILogger<BookUserActionsController> logger) : base(repository)
    {
        AccessControlProvider = new BookUserActionsAccessControlProvider(accessor, null);
        Logger = logger;

        Options = new TableControllerOptions
        {
            UnsafeEntityLogging = true
        };
    }
}
