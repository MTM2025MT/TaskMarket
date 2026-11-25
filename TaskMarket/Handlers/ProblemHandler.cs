using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace TaskMarket.Handlers
{
    // Abstract so it is neither discovered as a controller nor registered as a handler service
    public abstract class ProblemHandler : ControllerBase
    {
        protected IActionResult Problem(List<ErrorOr.Error> error)
        {
            // It's standard practice to base the HTTP response on the FIRST error
            var firstError = error.FirstOrDefault();

            var statusCode = firstError.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            // Return ASP.NET Core's built-in ProblemDetails format
            return Problem(
                title: firstError.Code,
                detail: firstError.Description,
                statusCode: statusCode
            );
        }
    }
}
