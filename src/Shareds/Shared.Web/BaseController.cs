using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SamarPlanner.Shared.Kernel;
using Microsoft.Extensions.Logging;
using SamarPlanner.Shared.Extensions;

namespace SamarPlanner.Shared;

[ApiController]
public class BaseController : ControllerBase
{
    protected Guid UserId => User.GetUserId();

    protected ActionResult Result<TResponse>(Result<TResponse> result)
    {
       // if (result.IsSuccess)
            return Ok(result);

        // return result.BadResultType switch
        // {
        //     BadResultType.NotFound => NotFound(result),
        //     BadResultType.Validation => BadRequest(result),
        //     BadResultType.General => StatusCode(500, result),
        //     _ => StatusCode(500, new { error = "Unknown bad result type" })
        // };
    }
}