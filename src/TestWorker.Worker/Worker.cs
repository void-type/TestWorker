using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using VoidCore.Domain.Events;

namespace TestWorker.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IConfiguration _workerConfig;
        private readonly IEventHandler<SendEmail.Request, SendEmail.Response> _eventHandler;

        private int LoopDelayMilliseconds => _workerConfig.GetValue<int>("LoopDelayMilliseconds");
        private int LoopsUntilRetry => _workerConfig.GetValue<int>("LoopsUntilRetry");


        public Worker(IConfiguration config, SendEmail.Handler eventHandler, SendEmail.Logger eventLogger)
        {
            _eventHandler = eventHandler.AddPostProcessor(eventLogger);
            _workerConfig = config.GetSection(nameof(Worker));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var nextLoop = DateTimeOffset.Now.AddMilliseconds(LoopDelayMilliseconds);

                // Send
                var request = new SendEmail.Request(GetRetryPoint());
                await _eventHandler.Handle(request, stoppingToken);

                // Wait until next loop.
                await DelayUntil(nextLoop, stoppingToken);
            }
        }

        private DateTimeOffset GetRetryPoint()
        {
            return DateTimeOffset.Now.AddMilliseconds(-1 * LoopsUntilRetry * LoopDelayMilliseconds);
        }

        private async Task DelayUntil(DateTimeOffset nextLoopStartAfter, CancellationToken stoppingToken)
        {
            var delaySpan = nextLoopStartAfter - DateTimeOffset.Now;

            var delayMilliseconds = Math.Max(0, Convert.ToInt32(delaySpan.TotalMilliseconds));

            await Task.Delay(delayMilliseconds, stoppingToken);
        }
    }
}
