using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peformance_Model_Queue
{
    public class ProcessConstants
    {
        public static readonly double TaskExecutionConstant = -1 / Math.Pow(5, -1.0);
        public static readonly double DiskExecutionConstant = -1 / Math.Pow(10, -1.0);
        public static readonly double ArrivalTimeConstant = -1 / Math.Pow(50, -1.0);

        public static readonly int RandomGeneratorSeed = 1245;
        public static readonly int NumberOfProcesses = 50;
    }
}
