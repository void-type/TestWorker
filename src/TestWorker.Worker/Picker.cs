using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace TestWorker
{
    public class Picker
    {
        private readonly ILogger<Picker> _logger;

        public Picker(ILogger<Picker> logger)
        {
            _logger = logger;
        }

        public Email[] Pick(IEnumerable<Email> allEmails, DateTimeOffset retryPickedBefore)
        {
            // Picker
            var pickedEmails = allEmails
                .Where(n =>
                    // Not picked and not scheduled (send immediately)
                    !n.Picked && !n.IsScheduled ||
                    // Not picked and scheduled time has passed (send when scheduled)
                    !n.Picked && n.IsScheduled && n.ScheduledToBeSentOn <= DateTimeOffset.Now ||
                    // Was picked, but still hasn't been sent after the retry delay (must have not sent)
                    n.Picked && !n.Sent && n.PickedOn < retryPickedBefore)
                .ToArray();

            _logger.LogDebug("Found {PickedEmailsCount} emails to send.", pickedEmails.Count());

            foreach (var email in pickedEmails)
            {
                email.Picked = true;
                email.PickedOn = DateTimeOffset.Now;
            }

            return pickedEmails;
        }
    }
}
