using ErrorOr;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Handlers;

public sealed class ServiceCategoryHandler
{
    private readonly IServiceCategoryRepository _serviceCategoryRepository;

    public ServiceCategoryHandler(IServiceCategoryRepository serviceCategoryRepository)
    {
        _serviceCategoryRepository = serviceCategoryRepository;
    }

    // From ServiceCategoriesController: GET /api/categories
    public async Task<ErrorOr<List<ServiceCategory>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _serviceCategoryRepository.GetAllAsync(cancellationToken);

        if (categories is null)
        {
            return Error.Failure(
                code: "GettingServiceCategories.Null",
                description: "Failed to get service categories and returned null.");
        }

        if (categories.Count == 0)
        {
            return Error.NotFound(
                code: "GettingServiceCategories.Empty",
                description: "No service categories found.");
        }

        return categories.ToList();
    }
}