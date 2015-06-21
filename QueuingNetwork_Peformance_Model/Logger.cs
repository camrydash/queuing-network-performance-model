using System;
using System.Globalization;

namespace Peformance_Model_Queue
{
    public class Logger
    {
        public static void Log(string format)
        {
            Console.WriteLine(format);
        }

        public static void LogFormat(string format, params object[] args)
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, format, args));
        }
    }
}