using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace TestWorker.Worker.Model
{
    public class EmailerService
    {
        private readonly ILogger<EmailerService> _logger;
        private readonly Random _random;

        public EmailerService(ILogger<EmailerService> logger)
        {
            _logger = logger;
            _random = new Random();
        }

        public void Send(Email email)
        {
            if (_random.Next(0, 1000) % 7 == 0)
            {
                Thread.Sleep(7000);
                _logger.LogWarning("Delay in sending: {EmailName}", email.Name);
            }

            if (_random.Next(0, 1000) % 25 == 0)
            {
                throw new Exception("Sending email exception.");
            }
        }
    }
}
