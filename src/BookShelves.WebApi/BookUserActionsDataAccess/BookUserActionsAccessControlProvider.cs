using BookShelves.Shared.Services.AuthorizationPolicies;
using CommunityToolkit.Datasync.Server;
using System.Linq.Expressions;
using System.Security.Claims;

namespace BookShelves.WebApi.BookUserActionsDataAccess;

public class BookUserActionsAccessControlProvider : AccessControlProvider<BookUserAction>
{
    private readonly IHttpContextAccessor accessor;
    private readonly ILogger<BookUserActionsAccessControlProvider>? logger;

    public BookUserActionsAccessControlProvider(IHttpContextAccessor accessor, ILogger<BookUserActionsAccessControlProvider>? logger = null)
    {
        this.accessor = accessor;
        this.logger = logger;
    }

    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    private bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    private bool IsAdmin => AuthorizationPolicies.IsAdminUser(User);

    private string? GetUserId()
    {
        var user = User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("oid")?.Value
            ?? user.FindFirst("sub")?.Value
            ?? user.Identity?.Name;
    }

    public override Expression<Func<BookUserAction, bool>> GetDataView()
    {
        if (!IsAuthenticated)
        {
            return _ => false;
        }

        if (IsAdmin)
        {
            logger?.LogInformation("Admin user querying book user actions");
            return _ => true;
        }

        var userId = GetUserId();
        logger?.LogInformation("User {UserId} querying book user actions", userId ?? "unknown");

        return action => action.UserId == userId;
    }

    public override ValueTask<bool> IsAuthorizedAsync(TableOperation operation, BookUserAction? entity, CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated)
        {
            logger?.LogWarning("Unauthorized operation attempt: {Operation}", operation);
            return new ValueTask<bool>(false);
        }

        if (IsAdmin)
        {
            logger?.LogInformation("Admin user performing {Operation} on book user action", operation);
            return base.IsAuthorizedAsync(operation, entity, cancellationToken);
        }

        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            logger?.LogWarning("Authenticated user had no identifier for {Operation}", operation);
            return new ValueTask<bool>(false);
        }

        if (entity is null)
        {
            return new ValueTask<bool>(false);
        }

        switch (operation)
        {
            case TableOperation.Create:
                entity.UserId = userId;
                logger?.LogInformation("User {UserId} creating book user action", userId);
                return new ValueTask<bool>(true);

            case TableOperation.Update:
                if (!string.Equals(entity.UserId, userId, StringComparison.OrdinalIgnoreCase))
                {
                    logger?.LogWarning("User {UserId} attempted to update book user action owned by {EntityUserId}", userId, entity.UserId);
                    return new ValueTask<bool>(false);
                }

                entity.UserId = userId;
                logger?.LogInformation("User {UserId} updating own book user action", userId);
                return new ValueTask<bool>(true);

            case TableOperation.Delete:
            case TableOperation.Read:
                if (!string.Equals(entity.UserId, userId, StringComparison.OrdinalIgnoreCase))
                {
                    logger?.LogWarning("User {UserId} attempted to access book user action owned by {EntityUserId}", userId, entity.UserId);
                    return new ValueTask<bool>(false);
                }

                return new ValueTask<bool>(true);

            default:
                return base.IsAuthorizedAsync(operation, entity, cancellationToken);
        }
    }
}
