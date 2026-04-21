using Microsoft.AspNetCore.Mvc;
using TaskMarket.Handlers;
using TaskMarket.IRepositories;

namespace TaskMarket.Controllers;

[ApiController]
[Route("api/categories")]
public class ServiceCategoriesController : ControllerBase
{
    private readonly IServiceCategoryRepository _serviceCategoryRepository;
    private readonly ServiceCategoryHandler _serviceCategoryHandler;

    public ServiceCategoriesController(IServiceCategoryRepository serviceCategoryRepository, ServiceCategoryHandler serviceCategoryHandler)
    {
        _serviceCategoryRepository = serviceCategoryRepository;
        _serviceCategoryHandler = serviceCategoryHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var resultBox = await _serviceCategoryHandler.GetAllAsync();

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }
}