using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Peformance_Model_Queue
{
    class Program
    {

        static void Main(string[] args)
        {
            ///Generate 50 Jobs With Random Task, Disk Execution Times
            IEnumerable<Process> jobs = ProcessGenerator.Instance.GenerateJobs();

            QueuingNetwork queue = new QueuingNetwork(jobs);
            queue.Run();
            Console.ReadKey();
        }
    }
}
