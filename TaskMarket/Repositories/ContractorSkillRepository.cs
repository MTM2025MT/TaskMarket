using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Repositories;

public sealed class ContractorSkillRepository : IContractorSkillRepository
{
    private readonly ApplicationDbContext _db;

    public ContractorSkillRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(ContractorSkill skill, CancellationToken cancellationToken = default)
        => await _db.Set<ContractorSkill>().AddAsync(skill, cancellationToken);

    public void Remove(ContractorSkill skill)
        => _db.Set<ContractorSkill>().Remove(skill);
}