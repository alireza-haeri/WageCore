using System.Security.Claims;

namespace SamarPlanner.Shared.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? throw new UnauthorizedAccessException("User ID claim not found.");
        
        if (!Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User ID claim has invalid format.");
        
        return userId;
    }
}