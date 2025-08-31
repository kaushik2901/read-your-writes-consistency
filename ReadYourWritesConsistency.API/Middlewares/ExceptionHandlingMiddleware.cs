using System.Net.Mime;
using ReadYourWritesConsistency.API.JSONSerialization;
using ReadYourWritesConsistency.API.Models;

namespace ReadYourWritesConsistency.API.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IDbIntentAccessor dbIntentAccessor)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var traceId = context.TraceIdentifier;
            logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = MediaTypeNames.Application.Json;

            var problem = Result.Failure("An unexpected error occurred.", dbIntentAccessor.Intent == DbIntent.Write ? "Master" : "Replica");
            
            await context.Response.WriteAsJsonAsync(problem, ExtendedJsonSerializationContext.Default.Result);
        }
    }
}


