using QueuingNetwork_Peformance_Model;
using System;
using System.Globalization;

namespace Peformance_Model_Queue
{
    public class Logger
    {
        private readonly IFileWriter _fileWriter;
        private readonly bool _peristStorage = true;
        private readonly Action<string> _persistToDisk;
        private readonly Action<string> _outputToConsole;
        private readonly bool _appendDate;
        private readonly bool _isOutputToConsole;

        private static readonly Lazy<Logger> defaultInstance = new Lazy<Logger>(() => new Logger("output.log"));

        public static Logger Instance
        {
            get { return Logger.defaultInstance.Value; }
        }

        public static Logger CreateLogger(string filePath, bool isOutputToConsole = true, bool appendDate = true)
        {
            return new Lazy<Logger>(() => new Logger(filePath, isOutputToConsole, appendDate)).Value;
        }

        public Logger (string logPath, bool isOutputToConsole = true, bool appendDate = true)
	    {
            _fileWriter = new DefaultFileWriter(logPath);
            _persistToDisk = (str) => LogToDisk(str);
            _outputToConsole = (str) => OutputToConsole(str);
            _appendDate = appendDate;
            _isOutputToConsole = isOutputToConsole;
	    }

        public void Log(string format)
        {
            _outputToConsole(format);
            _persistToDisk(format);
        }

        public void LogFormat(string format, params object[] args)
        {
            var formattedString = string.Format(CultureInfo.InvariantCulture, format, args);
            _outputToConsole(formattedString);
            _persistToDisk(formattedString);
        }

        private void OutputToConsole(string message)
        {
            if(_isOutputToConsole)
                Console.WriteLine(message);
        }

        private void LogToDisk(string message)
        {
            if (_peristStorage)
                if(_appendDate)
                    _fileWriter.Append(string.Format("{0}: \t{1}", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss:fff"), message));
                else
                    _fileWriter.Append(string.Format("{0}", message));
        }
    }
}