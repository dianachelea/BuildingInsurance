using BuildingInsurance.Application.Features.Common.Result;
using Microsoft.AspNetCore.Mvc;

namespace BuildingInsurance.API.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult ToActionResult<T>(Result<T> result)
        {
            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            if (!result.IsSuccess)
            {
                switch (result.ErrorType)
                {
                    case ErrorType.Validation:
                        return BadRequest(result);

                    case ErrorType.NotFound:
                        return NotFound(result);

                    case ErrorType.Conflict:
                        return Conflict(result);

                    case ErrorType.Unauthorized:
                        return Unauthorized(result);

                    case ErrorType.Forbidden:
                        return Forbid();

                    case ErrorType.BusinessRule:
                        return UnprocessableEntity(result);

                    default:
                        return StatusCode(500, result);
                }
            }

            if (result.Value is null)
                return NoContent();

            return Ok(result);
        }

        protected IActionResult ToCreatedAtActionResult<T>(string actionName, object routeValues, Result<T> result)
        {
            if (result is null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            if (!result.IsSuccess)
                return ToActionResult(result);

            return CreatedAtAction(actionName, routeValues, result);
        }
    }
}