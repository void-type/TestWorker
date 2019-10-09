using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TestWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly FakeData _data;
        private readonly Emailer _emailer;
        private readonly Picker _picker;
        private readonly IConfiguration _workerConfig;

        private int LoopDelayMilliseconds => _workerConfig.GetValue<int>("LoopDelayMilliseconds");
        private int LoopsUntilRetry => _workerConfig.GetValue<int>("LoopsUntilRetry");


        public Worker(ILogger<Worker> logger, FakeData data, Emailer emailer, Picker picker, IConfiguration config)
        {
            _logger = logger;
            _data = data;
            _emailer = emailer;
            _picker = picker;
            _workerConfig = config.GetSection(nameof(Worker));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {Now}", DateTimeOffset.Now);

                var nextLoop = DateTimeOffset.Now.AddMilliseconds(LoopDelayMilliseconds);

                // Add data
                AddFakeData(_data);

                // Send
                var emails = _picker.Pick(_data.Emails, GetRetryPoint());
                _emailer.Send(emails);

                _logger.LogDebug("Total not sent: {EmailsNotSent}", _data.Emails.Where(n => !n.Sent).ToArray().Count());

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

            _logger.LogInformation("Delay: {Delay}", delayMilliseconds);

            await Task.Delay(delayMilliseconds, stoppingToken);
        }

        private void AddFakeData(FakeData data)
        {
            for (int i = 0; i < 4; i++)
            {
                data.Emails.Add(new Email()
                {
                    Name = $"Email {data.Emails.Count() + 1}"
                });
            }

            data.Emails.Add(new Email()
            {
                Name = $"Email {data.Emails.Count() + 1}",
                IsScheduled = true,
                ScheduledToBeSentOn = DateTimeOffset.Now.AddMilliseconds(1 * LoopDelayMilliseconds / 2)
            });
        }
    }
}
