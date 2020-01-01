using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TestWorker.Worker.Events;
using TestWorker.Worker.Model;
using VoidCore.Domain.Events;

namespace TestWorker.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IConfiguration _workerConfig;
        private readonly DateTimeOffsetNowService _now;
        private readonly IEventHandler<SendEmail.Request, SendEmail.Response> _eventHandler;

        private int LoopDelayMilliseconds => _workerConfig.GetValue<int>("LoopDelayMilliseconds");
        private int LoopsUntilRetry => _workerConfig.GetValue<int>("LoopsUntilRetry");


        public Worker(IConfiguration config, SendEmail.Handler eventHandler, SendEmail.Logger eventLogger, DateTimeOffsetNowService now)
        {
            _eventHandler = eventHandler.AddPostProcessor(eventLogger);
            _workerConfig = config.GetSection(nameof(Worker));
            _now = now;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var nextLoop = _now.Moment.AddMilliseconds(LoopDelayMilliseconds);

                var request = new SendEmail.Request(GetRetryPoint());
                await _eventHandler.Handle(request, cancellationToken);

                await DelayUntil(nextLoop, cancellationToken);
            }
        }

        private DateTimeOffset GetRetryPoint()
        {
            return _now.Moment.AddMilliseconds(-1 * LoopsUntilRetry * LoopDelayMilliseconds);
        }

        private async Task DelayUntil(DateTimeOffset nextLoopStartAfter, CancellationToken stoppingToken)
        {
            var delaySpan = nextLoopStartAfter - _now.Moment;

            var delayMilliseconds = Math.Max(0, Convert.ToInt32(delaySpan.TotalMilliseconds));

            await Task.Delay(delayMilliseconds, stoppingToken);
        }
    }
}
