using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PhonieCore
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .ConfigureServices(services =>
                {
                    services.AddHostedService<PhonieWorker>();
                });       
    }    
}
