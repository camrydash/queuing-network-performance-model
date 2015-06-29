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
        public static string Pattern(int n)
        {
            // Happy Coding ^_^
            //var result = "";
            return String.Join("\n", Enumerable.Range(1, Math.Max(0, n)).Select(x=> String.Concat(Enumerable.Repeat(x, x))));
        }

        public static void CheckCoupon(string enteredCode, string correctCode, string currentDate, string expirationDate)
        {
            Enumerable.Range(1, 4)
                .Select(x => Enumerable.Repeat<char>('*', x).Count()).Sum();
        }



        static void Main(string[] args)
        {
            //Console.WriteLine(CheckCoupon("123","123","September 5, 2014","October 1, 2014"));

            ///Generate 50 Jobs With Random Task, Disk Execution Times
            ///edit: the professor wants us to generate jobs on the fly (there are infinite jobs), so this is useless
            //IEnumerable<Process> jobs = ProcessFactory.GenerateJobs();

            QueuingNetwork queue = new QueuingNetwork();
            queue.Run();
            Console.ReadKey();

        }
    }
}
