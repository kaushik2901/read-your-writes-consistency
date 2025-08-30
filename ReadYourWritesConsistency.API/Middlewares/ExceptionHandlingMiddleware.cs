using System.Net.Mime;
using ReadYourWritesConsistency.API.Models;
using ReadYourWritesConsistency.API.Persistence;

namespace ReadYourWritesConsistency.API.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, IDbIntentAccessor dbIntentAccessor, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
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

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}


