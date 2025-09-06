using ReadYourWritesConsistency.API.Models;
using ReadYourWritesConsistency.API.Services;

namespace ReadYourWritesConsistency.API.ConsistencyServices;

public class ReadConsistencyMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.Value?.Contains("V2", StringComparison.OrdinalIgnoreCase) != true)
        {
            await next(context);
            return;
        }

        CaptureConsistencyResources(context);

        await HandleConsistencyCheck(context);

        await next(context);

        await CaptureConsistencyTimestamp(context);
    }

    private static void CaptureConsistencyResources(HttpContext context)
    {
        var currentUserAccessor = context.RequestServices.GetRequiredService<ICurrentUserAccessor>();
        var consistencyContext = context.RequestServices.GetRequiredService<ConsistencyContext>();
        consistencyContext.Resources = [("user", currentUserAccessor.UserId.ToString())];
    }

    private static async Task HandleConsistencyCheck(HttpContext context)
    {
        if (!IsReadEndpoint(context))
        {
            return;
        }

        var dbIntentAccessor = context.RequestServices.GetRequiredService<IDbIntentAccessor>();
        var consistencyService = context.RequestServices.GetRequiredService<IConsistencyService>();

        var isReplicaConsistent = await consistencyService.IsReplicaConsistent();
        if (!isReplicaConsistent)
        {
            dbIntentAccessor.Intent = DbIntent.Write;
        }
    }

    private static async Task CaptureConsistencyTimestamp(HttpContext context)
    {
        if (IsReadEndpoint(context))
        {
            return;
        }

        var consistencyContext = context.RequestServices.GetRequiredService<ConsistencyContext>();
        var store = context.RequestServices.GetRequiredService<ConsistencyStore>();

        if (string.IsNullOrEmpty(consistencyContext.Timestamp))
        {
            return;
        }

        foreach (var (resourceName, resourceId) in consistencyContext.Resources)
        {
            await store.SetTimestampAsync(resourceName, resourceId, consistencyContext.Timestamp);
        }
    }

    private static bool IsReadEndpoint(HttpContext context)
    {
        return context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase);
    }
}
