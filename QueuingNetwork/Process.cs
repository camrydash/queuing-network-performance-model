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
        public int ProcessNumber { get; private set; }
        public double TaskExecutionTime { get; private set; }
        public double DiskExecutionTime { get; private set; }
        public double InterArrivalTime { get; private set; }
        public double ArrivalTime { get; private set; }
        public double NextArrivalTime { get; private set; }
        public ProcessState State { get; set; }

        private readonly Random _random = new Random();

        public Process(int processNumber)
        {
            ProcessNumber = processNumber;

            GenerateTaskExecutionTime();
            GenerateDiskExecutionTime();
            GenerateInterArrivalTime();
        }

        private void GenerateTaskExecutionTime()
        {
            TaskExecutionTime = (1d / Math.Pow(-5d, -1d)) * Math.Log(1 - GetRandom(), Math.E);
        }

        private void GenerateDiskExecutionTime()
        {
            DiskExecutionTime = (1d / Math.Pow(-10d, -1d)) * Math.Log(1 - GetRandom(), Math.E);
        }

        private void GenerateInterArrivalTime()
        {
            InterArrivalTime = (ProcessNumber == 1 ? 0 : (1d / Math.Pow(-50d, -1d)) * Math.Log(1 - GetRandom(), Math.E));
        }

        private double GetRandom()
        {
            return _random.NextDouble();
        }

        public override string ToString()
        {
            return String.Format("Process: {0}", ProcessNumber);
        }
    }
}
