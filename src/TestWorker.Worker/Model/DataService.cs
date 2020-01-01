using System.Collections.Generic;

namespace TestWorker.Worker.Model
{
    public class DataService
    {
        public DataService()
        {
            Emails = new List<Job<Email>>();
        }

        public List<Job<Email>> Emails { get; }

        public void SaveChanges() { return; }
    }
}
