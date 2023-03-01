using FileService.Managers;

namespace FileService.Infrastructure;

public class RequestCounterMiddleware : IMiddleware
{
    private DiagnosticManager Manager { get; }

    public RequestCounterMiddleware(DiagnosticManager manager)
    {
        Manager = manager;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var startAt = DateTime.Now;

        try
        {
            await next(context);
        }
        finally
        {
            await Manager.OnRequestEndAsync(context, startAt);
        }
    }
}
