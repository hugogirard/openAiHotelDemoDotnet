namespace DevOpsTool;

public class Bootstrapper : IBootstrapper
{
    private readonly ILogger<Bootstrapper> _logger;
    private readonly IAISearchService _aiSearchService;

    public Bootstrapper(ILogger<Bootstrapper> logger,
                        IAISearchService aiSearchService)
    {
        _logger = logger;
        _aiSearchService = aiSearchService;
    }

    public async Task CreateIndexingResources() 
    { 
        await _aiSearchService.CreateIndexingResources();
    }
}