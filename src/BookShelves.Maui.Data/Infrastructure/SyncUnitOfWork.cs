using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookShelves.Maui.Data.Infrastructure;

public class SyncUnitOfWork<TContext> : UnitOfWork<TContext>, ISyncUnitOfWork<TContext>
    where TContext : DbContext
{
    private readonly TContext _context;

    public ISyncProgressService? SyncProgressService { get; set; }

    public SyncUnitOfWork(IDbContextFactory<TContext> contextFactory)
        : base(contextFactory)
    {
        _context = contextFactory.CreateDbContext();
    }

    public async Task SynchronizeAsync(CancellationToken cancellationToken = default)
    {
        var syncMethod = typeof(TContext).GetMethod("SynchronizeAsync", new[] { typeof(CancellationToken) });
        if (syncMethod == null)
            throw new InvalidOperationException($"{typeof(TContext).Name} does not have a SynchronizeAsync method");

        var eventInfo = typeof(TContext).GetEvent("SynchronizationProgress");
        Delegate? handler = null;

        if (eventInfo != null && SyncProgressService != null)
        {
            Action<object, object> forwarder = (s, a) =>
            {
                try
                {
                    var msg = ExtractMessage(a);
                    SyncProgressService.Report(new SyncProgressEventArgs { Message = msg });
                }
                catch { }
            };

            try
            {
                handler = CreateEventHandler(forwarder, eventInfo.EventHandlerType);
                eventInfo.AddEventHandler(_context, handler);
            }
            catch { }
        }

        try
        {
            await (Task)syncMethod.Invoke(_context, new object[] { cancellationToken })!;
        }
        finally
        {
            if (eventInfo != null && handler != null)
            {
                try { eventInfo.RemoveEventHandler(_context, handler); } catch { }
            }
        }
    }

    private static string ExtractMessage(object? eventArgs)
    {
        if (eventArgs == null) return string.Empty;
        var msgProp = eventArgs.GetType().GetProperty("Message");
        return msgProp?.GetValue(eventArgs)?.ToString() ?? eventArgs.ToString() ?? string.Empty;
    }

    private static Delegate CreateEventHandler(Action<object, object> forwarder, Type? handlerType)
    {
        if (handlerType == null)
            throw new ArgumentNullException(nameof(handlerType));

        var invoke = handlerType.GetMethod("Invoke");
        var parameters = invoke!.GetParameters();
        if (parameters.Length != 2)
            throw new InvalidOperationException("Unexpected event handler signature");

        var paramExprs = parameters.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
        var forwardConst = Expression.Constant(forwarder);
        var forwardInvoke = typeof(Action<object, object>).GetMethod("Invoke")!;

        var call = Expression.Call(forwardConst, forwardInvoke,
            Expression.Convert(paramExprs[0], typeof(object)),
            Expression.Convert(paramExprs[1], typeof(object)));

        var lambda = Expression.Lambda(handlerType, call, paramExprs);
        return lambda.Compile();
    }
}
