namespace ReadYourWritesConsistency.API.Services;

public interface ICurrentUserAccessor
{
	int UserId { get; }
}

public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public int UserId
	{
		get
		{
			var http = _httpContextAccessor.HttpContext;
			if (http != null && http.Request.Headers.TryGetValue("X-User-Id", out var values) &&
				int.TryParse(values.ToString(), out var id))
			{
				return id;
			}
			return 1;
		}
	}
}


