using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Peformance_Model_Queue
{
    /// <summary>
    /// This class is responsible for creating a process
    /// </summary>
    public class ProcessFactory
    {
        private readonly static Random _random = new Random();

        public ProcessFactory()
        { }

        public static Process GenerateProcess(int processNumber)
        {
            return new Process(processNumber);
        }

    }
}
