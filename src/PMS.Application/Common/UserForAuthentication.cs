using PMS.Domain.Users;

namespace PMS.Application.Common;

public sealed record UserForAuthentication(User User, string RoleName);
