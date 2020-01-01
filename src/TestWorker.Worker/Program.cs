using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestWorker.Worker.Events;
using TestWorker.Worker.Model;

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
                    services.AddSingleton<DataService>();
                    services.AddSingleton<EmailerService>();
                    services.AddSingleton<DateTimeOffsetNowService>();
                    services.AddSingleton<SendEmail.Handler>();
                    services.AddSingleton<SendEmail.Logger>();
                });
    }
}
