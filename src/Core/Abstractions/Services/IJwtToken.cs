using SamarPlanner.Identity.Core.Contracts;
using SamarPlanner.Identity.Core.Entities;

namespace SamarPlanner.Identity.Core.Abstractions;

public interface IJwtTokenService
{
    JwtTokenResponse GenerateToken(User user);
}