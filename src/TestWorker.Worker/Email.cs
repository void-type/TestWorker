using System;

namespace TestWorker.Worker
{
    public class Email
    {
        public string Name { get; set; } = "Untitled";
        public bool Picked { get; set; }
        public DateTimeOffset PickedOn { get; set; }
        public bool Sent { get; set; }
        public DateTimeOffset SentOn { get; set; }
        public bool IsScheduled { get; set; }
        public DateTimeOffset ScheduledToBeSentOn { get; set; }
    }
}
