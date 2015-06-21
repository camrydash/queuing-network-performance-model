using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peformance_Model_Queue
{
    public class ProcessGenerator
    {
        private readonly int _randomSeed;
        private readonly int _numberOfJobs;
        private readonly Random _random;

        private static Lazy<ProcessGenerator> instance = new Lazy<ProcessGenerator>(() =>
            new ProcessGenerator(ProcessConstants.NumberOfProcesses, ProcessConstants.RandomGeneratorSeed));

        public static ProcessGenerator Instance { get { return instance.Value; } }

        public ProcessGenerator(int numberOfJobs, int randomSeed)
        {
            _numberOfJobs = numberOfJobs;
            _randomSeed = randomSeed;
            _random = new Random(_randomSeed);
        }

        private double CalculateRandomTaskExecutionTime(double x)
        {
            return ProcessConstants.TaskExecutionConstant * Math.Log(1 - x, Math.E);
        }

        private double CalculateRandomDiskExecutionTime(double x)
        {
            return ProcessConstants.DiskExecutionConstant * Math.Log(1 - x, Math.E);
        }

        private double CalculateRandomInterArrivalTime(double x)
        {
            return ProcessConstants.ArrivalTimeConstant * Math.Log(1 - x, Math.E);
        }

        private IEnumerable<Process> GenerateJobCollection()
        {
            var result = new List<Process>();
            foreach (int processNumber in Enumerable.Range(1, _numberOfJobs))
            {
                double x = _random.NextDouble();
                result.Add(new Process(processNumber, CalculateRandomTaskExecutionTime(x),
                    CalculateRandomDiskExecutionTime(x), processNumber == 1 ? 0 : CalculateRandomInterArrivalTime(x)));
                //yield return new Process(processNumber, CalculateRandomTaskExecutionTime(x), CalculateRandomDiskExecutionTime(x), CalculateRandomInterArrivalTime(x));
            }
            return result;
        }

        /// <summary>
        /// Generate a collection of jobs sorted by inter-arrival time
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Process> GenerateJobs()
        {
            return GenerateJobCollection().OrderBy(x => x.InterArrivalTime);
        }
    }
}
