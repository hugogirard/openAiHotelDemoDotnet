using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DevOpsTool;

public class Utility
{
    public static IBootstrapper CreateBoostrapInstance() 
    {

        // Create configuration file
        var configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)     
                            .AddEnvironmentVariables()
                            .AddUserSecrets<Program>()
                            .Build();

        var serviceProvider = new ServiceCollection()
                                    .AddLogging(c => c.AddConsole())
                                    .AddHttpClient()
                                    .AddSingleton<IConfiguration>(configuration)
                                    .AddTransient<IAISearchService, AISearchService>()
                                    .AddSingleton<IBootstrapper, Bootstrapper>()
                                    .BuildServiceProvider();


        return serviceProvider.GetService<IBootstrapper>() 
                                ?? throw new NullReferenceException("IBootstrapper is not implemented");
        
    }

}