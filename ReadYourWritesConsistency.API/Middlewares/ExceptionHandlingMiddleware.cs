using System.Net.Mime;
using ReadYourWritesConsistency.API.Models;

namespace ReadYourWritesConsistency.API.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ExceptionHandlingMiddleware> _logger;

	public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			var traceId = context.TraceIdentifier;
			_logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);

			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
			context.Response.ContentType = MediaTypeNames.Application.Json;

			var problem = Result.Failure("An unexpected error occurred.");

			await context.Response.WriteAsJsonAsync(problem);
		}
	}
}


