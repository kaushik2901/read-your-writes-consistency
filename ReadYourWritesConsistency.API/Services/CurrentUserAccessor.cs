namespace ReadYourWritesConsistency.API.Services;

public interface ICurrentUserAccessor
{
    int UserId { get; }
}

public sealed class CurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public int UserId
    {
        get
        {
            var http = httpContextAccessor.HttpContext;

            if (http != null && http.Request.Headers.TryGetValue("X-User-Id", out var values) &&
                int.TryParse(values.ToString(), out var id))
            {
                return id;
            }

            return 0;
        }
    }
}


