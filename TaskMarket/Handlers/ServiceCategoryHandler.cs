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
        return await _serviceCategoryRepository.GetAllAsync(cancellationToken);
    }
}