using System;

namespace TestWorker.Worker
{
    public class DateTimeOffsetNowService
    {
        public DateTimeOffset Now => DateTimeOffset.Now;
    }
}
