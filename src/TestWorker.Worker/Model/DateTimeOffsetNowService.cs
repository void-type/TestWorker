using System;

namespace TestWorker.Worker.Model
{
    public class DateTimeOffsetNowService
    {
        public DateTimeOffset Moment => DateTimeOffset.Now;
    }
}
