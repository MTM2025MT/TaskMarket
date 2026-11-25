using ErrorOr;
using Microsoft.Extensions.Caching.Memory;
using TaskMarket.Dtos;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Handlers;

public sealed class ContractorHandler
{
    // Shared with JobCacheRefreshService, which refreshes this entry every hour
    public const string BestContractorsCacheKey = "best-contractors-list";
    public const int BestContractorsCacheSize = 50;

    private readonly IContractorRepository _contractorRepository;
    private ILogger<Contractor> _ContractorLogger;
    private readonly IMemoryCache _cache;

    public ContractorHandler(IContractorRepository contractorRepository, ILogger<Contractor> contractorLogger,IMemoryCache Cache)
    {
        _contractorRepository = contractorRepository;
        _ContractorLogger = contractorLogger;
        _cache = Cache;
    }

    public async Task<ErrorOr<Contractor>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var contractor = await _contractorRepository.GetByIdAsync(id, cancellationToken);

        if (contractor is null)
        {
            _ContractorLogger.LogWarning("Contractor with ID {ContractorId} not found.", id);
            return Error.NotFound(
                code: "Contractor.NotFound",
                description: "Contractor not found."
            );
        }

        return contractor;
    }

    public async Task<ErrorOr<List<Contractor>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _contractorRepository.GetAllAsync(cancellationToken);
    }

    public async Task<ErrorOr<Success>> AddAsync(Contractor contractor, CancellationToken cancellationToken = default)
    {
        if (contractor is null)
        {
            return Error.Validation(
                code: "Contractor.Null",
                description: "Contractor cannot be null."
            );
        }

        await _contractorRepository.AddAsync(contractor, cancellationToken);
        await _contractorRepository.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> UpdateAsync(Contractor contractor, CancellationToken cancellationToken = default)
    {
        if (contractor is null)
        {
            return Error.Validation(
                code: "Contractor.Null",
                description: "Contractor cannot be null."
            );
        }

        var existingContractor = await _contractorRepository.GetByIdAsync(contractor.Id, cancellationToken);
        if (existingContractor is null)
        {
            return Error.NotFound(
                code: "Contractor.NotFound",
                description: "Contractor to update was not found."
            );
        }

        _contractorRepository.Update(contractor);
        await _contractorRepository.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> RemoveAsync(Contractor contractor, CancellationToken cancellationToken = default)
    {
        if (contractor is null)
        {
            return Error.Validation(
                code: "Contractor.Null",
                description: "Contractor cannot be null."
            );
        }

        var existingContractor = await _contractorRepository.GetByIdAsync(contractor.Id, cancellationToken);
        if (existingContractor is null)
        {
            return Error.NotFound(
                code: "Contractor.NotFound",
                description: "Contractor to remove was not found."
            );
        }

        _contractorRepository.Remove(existingContractor);
        await _contractorRepository.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }

    public async Task<ErrorOr<List<ContractorDto>>> GetBestContractors(int count, CancellationToken cancellationToken = default)
    {
        if (count < 1 || count > BestContractorsCacheSize)
        {
            return Error.Validation(
                code: "Contractor.InvalidCount",
                description: $"Count must be between 1 and {BestContractorsCacheSize}."
            );
        }

        // Cache-aside: the background job normally fills the cache, but fall back to the DB if it hasn't yet
        if (!_cache.TryGetValue(BestContractorsCacheKey, out List<Contractor>? bestContractors) || bestContractors is null)
        {
            bestContractors = await _contractorRepository.GetBestAsync(BestContractorsCacheSize, cancellationToken);
            _cache.Set(BestContractorsCacheKey, bestContractors, TimeSpan.FromHours(1));
        }

        return bestContractors
            .Take(count)
            .Select(c => c.ToDto())
            .ToList();
    }

}