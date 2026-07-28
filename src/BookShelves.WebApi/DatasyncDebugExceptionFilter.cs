using Microsoft.AspNetCore.Mvc.Filters;

namespace BookShelves.WebApi;

public class DatasyncDebugExceptionFilter : IExceptionFilter
{
    private readonly ILogger<DatasyncDebugExceptionFilter> _logger;

    public DatasyncDebugExceptionFilter(ILogger<DatasyncDebugExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        // Log the exact exception throwing the 400
        _logger.LogError(context.Exception, "Datasync Intercepted Exception: {Message}", context.Exception.Message);
    }
}