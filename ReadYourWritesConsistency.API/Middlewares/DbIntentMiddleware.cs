using ReadYourWritesConsistency.API.Persistence;

namespace ReadYourWritesConsistency.API.Middlewares;

public sealed class DbIntentMiddleware
{
    private readonly RequestDelegate _next;

    public DbIntentMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IDbIntentAccessor accessor)
    {
        // Decide intent: default read; switch to write on methods that modify state
        // or via header `X-Db-Intent: write` to force master for read-your-writes later.
        var method = context.Request.Method;
        accessor.Intent = method is "POST" or "PUT" or "PATCH" or "DELETE"
            ? DbIntent.Write
            : DbIntent.Read;

        if (context.Request.Headers.TryGetValue("X-Db-Intent", out var intent) &&
            intent.ToString().Equals("write", StringComparison.OrdinalIgnoreCase))
        {
            accessor.Intent = DbIntent.Write;
        }

        await _next(context);
    }
}


