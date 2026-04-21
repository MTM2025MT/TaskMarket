using ErrorOr;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Handlers;

public sealed class ContractorHandler
{
    private readonly IContractorRepository _contractorRepository;

    public ContractorHandler(IContractorRepository contractorRepository)
    {
        _contractorRepository = contractorRepository;
    }

    public async Task<ErrorOr<Contractor>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var contractor = await _contractorRepository.GetByIdAsync(id, cancellationToken);

        if (contractor is null)
        {
            return Error.NotFound(
                code: "Contractor.NotFound",
                description: "Contractor not found."
            );
        }

        return contractor;
    }

    public async Task<ErrorOr<List<Contractor>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var contractors = await _contractorRepository.GetAllAsync(cancellationToken);

        if (contractors is null)
        {
            return Error.Failure(
                code: "GettingAllContractors.Null",
                description: "Failed to Get All contractors and returned null."
            );
        }
        if (contractors.Count == 0)
        {
            return Error.NotFound(
                code: "GettingAllContractors.Empty",
                description: "No contractors found."
            );
        }

        return contractors;
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

        _contractorRepository.Remove(contractor);
        return Result.Success;
    }
}