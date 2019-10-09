using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace TestWorker
{
    public class Emailer
    {
        private readonly ILogger<Emailer> _logger;
        private readonly Random _random;

        public Emailer(ILogger<Emailer> logger)
        {
            _logger = logger;
            _random = new Random();
        }

        public void Send(Email[] emails)
        {
            foreach (var email in emails)
            {
                Send(email);
            }
        }

        public void Send(Email email)
        {
            try
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

                _logger.LogDebug("Sending: {EmailName}", email.Name);

                email.Sent = true;
                email.SentOn = DateTimeOffset.Now;
            }
            catch
            {
                _logger.LogCritical("Thrown exception sending: {EmailName}", email.Name);
            }
        }
    }
}
