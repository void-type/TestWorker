using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TestWorker.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddSingleton<FakeData>();
                    services.AddSingleton<Emailer>();
                    services.AddSingleton<DateTimeOffsetNowService>();
                    services.AddSingleton<SendEmail.Handler>();
                    services.AddSingleton<SendEmail.Logger>();
                });
    }
}
