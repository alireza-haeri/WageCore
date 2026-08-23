namespace Shared.Web;

[ApiController]
public class BaseController : ControllerBase
{
    protected Guid UserId => User.GetUserId();
    protected string UserPersianDateFormat = "yyyy/MM/dd";

    protected ActionResult Result<TResponse>(Result<TResponse> result)
    {
        return Ok(result);
    }
}