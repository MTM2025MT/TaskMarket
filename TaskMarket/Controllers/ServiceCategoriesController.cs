using Microsoft.AspNetCore.Mvc;
using TaskMarket.Handlers;

namespace TaskMarket.Controllers;

[ApiController]
[Route("api/categories")]
public class ServiceCategoriesController : ProblemHandler
{
    private readonly ServiceCategoryHandler _serviceCategoryHandler;

    public ServiceCategoriesController(ServiceCategoryHandler serviceCategoryHandler)
    {
        _serviceCategoryHandler = serviceCategoryHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var resultBox = await _serviceCategoryHandler.GetAllAsync();

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => Problem(failure)
        );
    }
}