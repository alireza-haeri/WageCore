namespace Shared.Web;

[ApiController]
public class BaseController : ControllerBase
{
    protected Guid UserId => User.GetUserId();

    protected ActionResult Result<TResponse>(Result<TResponse> result)
    {
        return Ok(result);
    }
}