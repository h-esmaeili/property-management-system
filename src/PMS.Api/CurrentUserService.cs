using System.Security.Claims;
using PMS.Application.Common.Interfaces;

namespace PMS.Api;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http)
    {
        _http = http;
    }

    public Guid? UserId
    {
        get
        {
            var v = _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(v, out var id) ? id : null;
        }
    }

    public Guid? TenantId
    {
        get
        {
            var v = _http.HttpContext?.User.FindFirstValue("tenant_id");
            return Guid.TryParse(v, out var id) ? id : null;
        }
    }
}
