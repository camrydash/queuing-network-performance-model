using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Peformance_Model_Queue
{
    public class DiskProcessor : Processor
    {
        private readonly string _serverName;
        private double _processingTime;

        public DiskProcessor()
            : this("Generic")
        { }

        public DiskProcessor(string serverName)
        {
            _serverName = serverName;
        }

        public void Process(Process p)
        {
            IsBusy = true;
            _processingTime += p.TaskExecutionTime;
            Thread.Sleep(TimeSpan.FromSeconds(p.DiskExecutionTime));
            IsBusy = false;
        }

        public override string ToString()
        {
            return _serverName;
        }

        public double ProcessingTime
        {
            get { return _processingTime; }
        }
    }
}
