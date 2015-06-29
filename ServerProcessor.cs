using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Peformance_Model_Queue
{
    public class ServerProcessor : Processor
    {
        private readonly string _serverName;
        private double _processingTime;

        public ServerProcessor() : this ("Generic")
        { }

        public ServerProcessor(string serverName)
        {
            _serverName = serverName;
        }

        public void Process(Process p)
        {
            IsBusy = true;
            _processingTime += p.TaskExecutionTime;
            Thread.Sleep(TimeSpan.FromMilliseconds(p.TaskExecutionTime * 1000));
            IsBusy = false;
        }

        public override string ToString()
        {
            return _serverName.ToString();
        }

        public double ProcessingTime
        {
            get { return _processingTime; }
        }
    }
}
