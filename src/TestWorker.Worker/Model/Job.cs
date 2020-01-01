using System;

namespace TestWorker.Worker.Model
{
    public class Job<T>
    {
        public Job(T payload)
        {
            Payload = payload;
        }

        public bool IsPicked { get; set; }
        public DateTimeOffset PickedOn { get; set; }
        public bool IsScheduled { get; set; }
        public DateTimeOffset ScheduledFor { get; set; }
        public bool IsComplete { get; set; }
        public DateTimeOffset CompletedOn { get; set; }
        public T Payload { get; }
    }
}
