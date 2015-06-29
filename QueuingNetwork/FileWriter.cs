using Peformance_Model_Queue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueuingNetwork_Peformance_Model
{
    internal class DefaultFileWriter : IFileWriter
    {
        private readonly string _filePath;
        private readonly SafeExecution _threadContext = new SafeExecution(typeof(DefaultFileWriter).Name);

        public DefaultFileWriter(string filePath)
        {
            _filePath = filePath;
            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }

        public void Append(string text)
        {
            _threadContext.Execute(() => InternalAppend(text));
        }

        private void InternalAppend(string text)
        {
            try
            {
                using (var stream = new StreamWriter(_filePath, true))
                {
                    stream.WriteLine(text);
                }
            }
            catch
            {

            }
        }
    }
}
