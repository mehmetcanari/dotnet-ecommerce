using ECommerce.Application.Utility;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    public class ApiBaseController : Controller
    {
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsFailure)
                return BadRequest(result);

            return Ok(result);
        }

        protected IActionResult HandleResult(Result result)
        {
            if (result.IsFailure)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
