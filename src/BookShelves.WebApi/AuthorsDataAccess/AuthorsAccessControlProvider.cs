using CommunityToolkit.Datasync.Server;
using System.Linq.Expressions;

namespace BookShelves.WebApi.AuthorsDataAccess
{
    public class AuthorsAccessControlProvider : AccessControlProvider<Author>
    {
        private readonly IHttpContextAccessor _accessor;

        public AuthorsAccessControlProvider(IHttpContextAccessor accessor) { _accessor = accessor; }

        private string? UserId => _accessor.HttpContext?.User?.Identity?.Name;

        public override Expression<Func<Author, bool>> GetDataView()
        {
            return _ => true;
            //return (UserId == null)
            //    ? _ => false
            //    : _ => true;   // model => model.UserId == UserId;
        }

        public override ValueTask<bool> IsAuthorizedAsync(TableOperation operation, Author? entity, CancellationToken cancellationToken = default)
        {
            return base.IsAuthorizedAsync(operation, entity, cancellationToken);
        }
    }
}



//public class PrivateAccessControlProvider<T> : IAccessControlProvider<T>
//    where T : ITableData
//    where T : IUserId
//{
//    private readonly IHttpContextAccessor _accessor;

//    public PrivateAccessControlProvider(IHttpContextAccessor accessor)
//    {
//        _accessor = accessor;
//    }

//    private string UserId { get => _accessor.HttpContext.User?.Identity?.Name; }

//    public Expression<Func<T, bool>> GetDataView()
//    {
//        return (UserId == null)
//          ? _ => false
//          : model => model.UserId == UserId;
//    }

//    public Task<bool> IsAuthorizedAsync(TableOperation op, T entity, CancellationToken token = default)
//    {
//        if (op == TableOperation.Create || op == TableOperation.Query)
//        {
//            return Task.FromResult(true);
//        }
//        else
//        {
//            return Task.FromResult(entity?.UserId != null && entity?.UserId == UserId);
//        }
//    }

//    public virtual Task PreCommitHookAsync(TableOperation operation, T entity, CancellationToken token = default)
//    {
//        entity.UserId == UserId;
//        return Task.CompletedTask;
//    }
//}