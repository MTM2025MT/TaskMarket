using Microsoft.Extensions.Caching.Memory;
using TaskMarket.Handlers;
using TaskMarket.IRepositories;
namespace TaskMarket.BackgroundJobs
{
    // Refreshes the "best contractors" cache read by ContractorHandler.GetBestContractors
    public class JobCacheRefreshService : BackgroundService
    {
        private readonly IMemoryCache _cache;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<JobCacheRefreshService> _logger;

        public JobCacheRefreshService(IMemoryCache cache, IServiceScopeFactory scopeFactory, ILogger<JobCacheRefreshService> logger)
        {
            _cache = cache;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromHours(1));

            do
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IContractorRepository>();

                    var bestContractors = await repo.GetBestAsync(ContractorHandler.BestContractorsCacheSize, stoppingToken);
                    _cache.Set(ContractorHandler.BestContractorsCacheKey, bestContractors, TimeSpan.FromHours(2));
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Failed to refresh the best contractors cache");
                }
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}
