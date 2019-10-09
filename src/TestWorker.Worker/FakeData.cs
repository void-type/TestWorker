using System.Collections.Generic;

namespace TestWorker.Worker
{
    public class FakeData
    {
        public FakeData()
        {
            Emails = new List<Email>();

        }

        public List<Email> Emails { get; }
    }
}
