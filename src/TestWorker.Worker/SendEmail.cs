using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VoidCore.Domain;
using VoidCore.Domain.Events;

namespace TestWorker.Worker
{
    public class SendEmail
    {
        public class Handler : EventHandlerAbstract<Request, Response>
        {
            private readonly FakeData _data;
            private readonly Emailer _emailer;
            private readonly DateTimeOffsetNowService _dateTimeOffsetService;
            private readonly ILogger<Handler> _logger;

            public Handler(FakeData data, Emailer emailer, DateTimeOffsetNowService dateTimeOffsetService, ILogger<Handler> logger)
            {
                _data = data;
                _emailer = emailer;
                _dateTimeOffsetService = dateTimeOffsetService;
                _logger = logger;
            }

            public override Task<IResult<Response>> Handle(Request request, CancellationToken cancellationToken = default)
            {
                // Add data
                AddFakeData(_data);

                var pickedEmails = _data.Emails
                   .Where(n =>
                       // Not picked and not scheduled (send immediately)
                       !n.Picked && !n.IsScheduled ||
                       // Not picked and scheduled time has passed (send when scheduled)
                       !n.Picked && n.IsScheduled && n.ScheduledToBeSentOn <= _dateTimeOffsetService.Now ||
                       // Was picked, but still hasn't been sent after the retry delay (must have not sent)
                       n.Picked && !n.Sent && n.PickedOn < request.RetryIfPickedBefore)
                   .ToArray();

                foreach (var email in pickedEmails)
                {
                    email.Picked = true;
                    email.PickedOn = DateTimeOffset.Now;
                }

                // Save pickedEmails

                var failed = 0;
                var sent = 0;

                foreach (var email in pickedEmails)
                {
                    try
                    {
                        _emailer.Send(email);
                        email.Sent = true;
                        email.SentOn = DateTimeOffset.Now;
                        sent++;
                        // Save email
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending email: {EmailName}", email.Name);
                        failed++;
                    }
                }

                var totalNotSent = _data.Emails.Where(n => !n.Sent).Count();
                var scheduled = _data.Emails.Where(n => !n.Sent && n.IsScheduled).Count();

                return Task.FromResult(Ok(new Response(pickedEmails.Count(), failed, sent, totalNotSent, scheduled)));
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
                    ScheduledToBeSentOn = DateTimeOffset.Now.AddMilliseconds(100)
                });
            }
        }

        public class Request
        {
            public Request(DateTimeOffset retryIfPickedBefore)
            {
                RetryIfPickedBefore = retryIfPickedBefore;
            }

            public DateTimeOffset RetryIfPickedBefore { get; }
        }

        public class Response
        {
            public Response(int picked, int failed, int sent, int totalNotSent, int scheduled)
            {
                Picked = picked;
                Failed = failed;
                Sent = sent;
                TotalNotSent = totalNotSent;
                Scheduled = scheduled;
            }

            public int Picked { get; }
            public int Failed { get; }
            public int Sent { get; }
            public int TotalNotSent { get; }
            public int Scheduled { get; }
        }

        public class Logger : PostProcessorAbstract<Request, Response>
        {
            private readonly ILogger<Logger> _logger;

            public Logger(ILogger<Logger> logger)
            {
                _logger = logger;
            }

            protected override void OnSuccess(Request request, Response response)
            {
                _logger.LogInformation("Email picked: {Picked} failed: {Failed} sent: {Sent} notSentNotScheduled: {NotSent} scheduled: {Scheduled}",
                response.Picked, response.Failed, response.Sent, response.TotalNotSent - response.Scheduled, response.Scheduled);

                base.OnSuccess(request, response);
            }
        }
    }
}
