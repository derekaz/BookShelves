using CommunityToolkit.Datasync.Server;
using System.Linq.Expressions;
using System.Security.Claims;

namespace BookShelves.WebApi.BooksDataAccess
{
    public class BooksAccessControlProvider : AccessControlProvider<Book>
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly ILogger<BooksAccessControlProvider>? _logger;

        public BooksAccessControlProvider(IHttpContextAccessor accessor, ILogger<BooksAccessControlProvider>? logger = null)
        {
            _accessor = accessor;
            _logger = logger;
        }

        /// <summary>
        /// Extracts the user ID from the bearer token (JWT claims).
        /// Used for logging/debugging only.
        /// </summary>
        private string? GetUserId()
        {
            var user = _accessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("oid")?.Value
                ?? user.FindFirst("sub")?.Value
                ?? user.Identity?.Name;
        }

        public override Expression<Func<Book, bool>> GetDataView()
        {
            var user = _accessor.HttpContext?.User;

            // If not authenticated, return no rows
            if (user?.Identity?.IsAuthenticated != true)
                return _ => false;

            var userId = GetUserId();
            _logger?.LogInformation("User {UserId} querying books", userId ?? "unknown");

            // Return all records for authenticated users
            return _ => true;
        }

        public override ValueTask<bool> IsAuthorizedAsync(TableOperation operation, Book? entity, CancellationToken cancellationToken = default)
        {
            var user = _accessor.HttpContext?.User;
            var isAuthenticated = user?.Identity?.IsAuthenticated == true;

            if (!isAuthenticated)
            {
                _logger?.LogWarning("Unauthorized operation attempt: {Operation}", operation);
                return new ValueTask<bool>(false);
            }

            var userId = GetUserId();
            _logger?.LogInformation("User {UserId} performing {Operation} on book", userId ?? "unknown", operation);

            return base.IsAuthorizedAsync(operation, entity, cancellationToken);
        }
    }
}