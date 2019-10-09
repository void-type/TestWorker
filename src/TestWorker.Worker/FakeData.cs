using System.Collections.Generic;

namespace TestWorker
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
