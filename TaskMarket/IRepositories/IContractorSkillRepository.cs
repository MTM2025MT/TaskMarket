using TaskMarket.Models;

namespace TaskMarket.IRepositories;

public interface IContractorSkillRepository
{
    Task AddAsync(ContractorSkill skill, CancellationToken cancellationToken = default);
    void Remove(ContractorSkill skill);
}