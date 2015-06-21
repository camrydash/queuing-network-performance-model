using QueuingNetwork_Peformance_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peformance_Model_Queue
{


    public class Process
    {
        private readonly int _processNumber;
        private readonly double _taskExecutionTime;
        private readonly double _diskExecutionTime;
        private readonly double _interArrivalTime;

        public ProcessState State { get; set;}

        public Process(int processNumber, double taskExecutionTime, double diskExecutionTime, double interArrivalTime)
        {
            _processNumber = processNumber;
            _taskExecutionTime = taskExecutionTime;
            _diskExecutionTime = diskExecutionTime;
            _interArrivalTime = interArrivalTime;
        }

        public int ProcessNumber
        {
            get { return _processNumber; }
        }

        public double InterArrivalTime
        {
            get { return _interArrivalTime; }
        }

        public double TaskExecutionTime
        {
            get { return _taskExecutionTime; }
        }

        public double DiskExecutionTime
        {
            get { return _diskExecutionTime; }
        }

        public override string ToString()
        {
            return String.Format("Process: {0}", _processNumber);
        }
    }
}
