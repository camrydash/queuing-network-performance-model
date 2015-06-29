using QueuingNetwork_Peformance_Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Peformance_Model_Queue
{
    public class QueuingNetwork
    {
        /// <summary>
        /// The list of processes we need to execute in the Queuing Network
        /// </summary>
        private readonly List<Process> _processes = new List<Process>();

        /// <summary>
        /// Server Processor Queue for Servers S1, S2 & S3
        /// </summary>
        private readonly Queue<Process> _processorQueue;

        /// <summary>
        /// Disk I/O Queue For D1, D2 & D3
        /// </summary>
        private readonly Queue<Process> _disk1Queue = new Queue<Process>();
        private readonly Queue<Process> _disk2Queue = new Queue<Process>();
        private readonly Queue<Process> _disk3Queue = new Queue<Process>();

        /// <summary>
        /// Server Processor S1, S2 & S3
        /// </summary>
        private readonly ServerProcessor _serverOne = new ServerProcessor("S1");
        private readonly ServerProcessor _serverTwo = new ServerProcessor("S2");
        private readonly ServerProcessor _serverThree = new ServerProcessor("S3");

        /// <summary>
        /// Disk Processor D1, D2 & D3
        /// </summary>
        private readonly DiskProcessor _diskOne = new DiskProcessor("D1");
        private readonly DiskProcessor _diskTwo = new DiskProcessor("D2");
        private readonly DiskProcessor _diskThree = new DiskProcessor("D3");

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly Random _random;

        private int _processNumber = 1;
        private int _jobsCompleted = 0;
        private double _serverProcessingTime = 0;
        private double _diskProcessingTime = 0;
        private int minimumQueueLength = 0;
        private int maximumQueueLength = 0;

        /// <summary>
        /// Process numbers (1...50), Which Queue Held Which Process, D1 Queue Could Be : 4, 6, 12
        /// </summary>
        public List<int> _processorQueueHandled;
        public List<int> _serverProcessesorQueueHandled;
        public List<int> _disk1QueueHandled;
        public List<int> _disk2QueueHandled;
        public List<int> _disk3QueueHandled;

        private Func<bool> _executionContinue;

        private int _firstThreadEntry = 0;
        private readonly SafeExecution _threadExecutionContext = new SafeExecution(typeof(QueuingNetwork).Name);
        private readonly static Logger Logger = Logger.Instance;

        public QueuingNetwork()
        {
            _random = new Random();
            _processorQueue = new Queue<Process>();

            _processorQueueHandled = new List<int>();
            _serverProcessesorQueueHandled = new List<int>();
            _disk1QueueHandled = new List<int>(); ;
            _disk2QueueHandled = new List<int>();
            _disk3QueueHandled = new List<int>();

            _executionContinue = () => ContinueExecution();
        }

        /// <summary>
        /// Executed at program completion
        /// </summary>
        /// <returns></returns>
        private bool ContinueExecution()
        {        
            if (_threadExecutionContext.Execute<int>(() => _jobsCompleted) < 50)
                return true;

            if (_firstThreadEntry == 0)
            {
                _threadExecutionContext.Execute(() =>
                {
                    if (_firstThreadEntry == 0)
                    {
                        _firstThreadEntry = 1;
                        Stop();
                    }
                });
            }

            return false;
        }

        private bool IsServerAvailable
        {
            get { return !_serverOne.IsBusy || !_serverTwo.IsBusy || !_serverThree.IsBusy; }
        }

        /// <summary>
        /// Select the disk with the shortest queue
        /// else
        /// Choose a random disk D1, D2 or D3
        /// </summary>
        /// <returns></returns>
        private Queue<Process> GetNextAvailableDisk()
        {
            int diskNumber = CalculateRandomFromRange(1, 3);
            if(_disk1Queue.Count < _disk2Queue.Count) {
                if(_disk1Queue.Count < _disk3Queue.Count)
                    diskNumber = 1; 
                else if(_disk3Queue.Count < _disk1Queue.Count)
                    diskNumber = 3;
            } else if (_disk2Queue.Count < _disk1Queue.Count)
                if(_disk2Queue.Count < _disk3Queue.Count)
                    diskNumber = 2;
                else if (_disk3Queue.Count < _disk2Queue.Count)
                    diskNumber = 3;

            //Logger.LogFormat("Pushed to Disk: {0}", diskNumber);
            return diskNumber == 1 ? _disk1Queue : diskNumber == 2 ? _disk2Queue : diskNumber == 3 ? _disk3Queue : null;
        }

        /// <summary>
        /// Get the next available server to process the job
        /// </summary>
        /// <returns></returns>
        private ServerProcessor GetNextAvailableServer()
        {
            if (!_serverOne.IsBusy)
            {
                //Logger.LogFormat("Server: {0} Is Available!", 1);
                return _serverOne;
            }

            if (!_serverTwo.IsBusy)
            {
                //Logger.LogFormat("Server: {0} Is Available!", 2);
                return _serverTwo;
            }

            if (!_serverThree.IsBusy)
            {
                //Logger.LogFormat("Server: {0} Is Available!", 3);
                return _serverThree;
            }

            throw new ArgumentException("No Server Process Is Free!");
        }

        /// <summary>
        /// Simulates the arrival of processes and puts them into the processor queue
        /// continue the generation of processes until 50 jobs have been processed
        /// </summary>
        private void EnqueueProcess()
        {
            Task.Factory.StartNew(() =>
            {
                do
                {
                    Process p = ProcessFactory.GenerateProcess(_processNumber++);
                    
                    _processorQueue.Enqueue(p);

                    Logger.LogFormat("Job: {0} is Entering the Queuing Network", p.ProcessNumber);

                    if (_processorQueue.Count > maximumQueueLength)
                        maximumQueueLength = _processorQueue.Count;

                    if (_processorQueue.Count < minimumQueueLength)
                        minimumQueueLength = _processorQueue.Count;

                    _processes.Add(p);

                    Func<Process, TimeSpan> calculateInterArrivalTime = (process) => TimeSpan.FromSeconds(p.InterArrivalTime);
                    Thread.Sleep(calculateInterArrivalTime(p));
                }
                while (_executionContinue());
            });
        }

        /// <summary>
        /// Simulates a thread for S1, S2 & S3 which extracts a process from the Process Queue, process the task, and put in a disk queue
        /// </summary>
        private void ExecuteProcesses()
        {
            Task.Factory.StartNew(() =>
            {
                do
                {
                    if (_processorQueue.Count > 0)
                    {
                        if (IsServerAvailable)
                        {
                            ServerProcessor processor = GetNextAvailableServer();

                            ///Dequeue the process and get the server processor to process it
                            Process p = _processorQueue.Dequeue();
                            //Logger.LogFormat("Dequeue: {0}\t\t[U]", process);

                            Logger.LogFormat("Server: {0} is Executing Job {1}", processor, p.ProcessNumber);
                            _serverProcessingTime += p.TaskExecutionTime;

                            ExecuteAsync(()=> processor.Process(p));

                            _serverProcessesorQueueHandled.Add(p.ProcessNumber);

                            ///After processing the process, we need to put in a disk queue
                            ///Disk Processor may be requesting the same resource while enquing, we need to lock the resource

                            _threadExecutionContext.Execute(() =>
                            {
                                Queue<Process> diskQueue = GetNextAvailableDisk();
                                //Logger.Log("Enqueuing Process in Disk");
                                diskQueue.Enqueue(p);
                            });
                        }
                    }
                    Thread.Sleep(50);
                }
                while (_executionContinue());
            });
        }

        /// <summary>
        /// D1 looks to see if I have any items in the D1 queue, if so, dequeue and process the job
        /// </summary>
        private void Disk1Executor()
        {
            Task.Factory.StartNew(() =>
            {
                do
                {
                    if (_threadExecutionContext.Execute<int>(()=> _disk1Queue.Count()) > 0)
                    {
                        Process p = _threadExecutionContext.Execute<Process>(_disk1Queue.Dequeue);

                        DiskProcessor processor = _diskOne;
                        processor.Process(p);

                        Logger.LogFormat("Disk {0} is Executing Job {1}", 1, p.ProcessNumber);
                        _threadExecutionContext.Execute(() => _diskProcessingTime += p.DiskExecutionTime);

                        _threadExecutionContext.Execute(() => _jobsCompleted++);
                        Logger.LogFormat("Job {0} is Exiting the Queuing network", p.ProcessNumber);

                        _disk1QueueHandled.Add(p.ProcessNumber);
                    }
                    Thread.Sleep(100);
                }
                while (_executionContinue());
            });
        }

        private void Disk2Executor()
        {
            Task.Factory.StartNew(() =>
            {
                do
                {
                    if (_threadExecutionContext.Execute<int>(() => _disk2Queue.Count()) > 0)
                    {
                        Process p = _threadExecutionContext.Execute<Process>(_disk2Queue.Dequeue);
                        //Logger.LogFormat("Dequeuing Process in Disk 2: {0}", p);

                        Logger.LogFormat("Disk {0} is Executing Job {1}", 2, p.ProcessNumber);
                        //Logger.LogFormat("Processing Process: {0} in Disk 2", p);

                        _threadExecutionContext.Execute(() => _diskProcessingTime += p.DiskExecutionTime);

                        DiskProcessor processor = _diskTwo;
                        processor.Process(p);

                        _threadExecutionContext.Execute(() => _jobsCompleted++);
                        Logger.LogFormat("Job {0} is Exiting the Queuing Network", p.ProcessNumber);
                        //Logger.LogFormat("Completed Proccess: {0} \t\t [C]", p);

                        _disk2QueueHandled.Add(p.ProcessNumber);
                    }
                    Thread.Sleep(100);
                }
                while (_executionContinue());
            });
        }

        private void Disk3Executor()
        {
            Task.Factory.StartNew(() =>
            {
                do
                {
                    if (_threadExecutionContext.Execute<int>(() => _disk3Queue.Count()) > 0)
                    {
                        Process p = _threadExecutionContext.Execute<Process>(_disk3Queue.Dequeue);
                        //Logger.LogFormat("Dequeuing Process in Disk 3: {0}", p);

                        Logger.LogFormat("Disk {0} is Executing Job {1}", 3, p.ProcessNumber);
                        //Logger.LogFormat("Processing Process: {0} in Disk 3", p);

                        _threadExecutionContext.Execute(() => _diskProcessingTime += p.DiskExecutionTime);

                        DiskProcessor processor = _diskThree;
                        processor.Process(p);

                        _threadExecutionContext.Execute(() => _jobsCompleted++);
                        Logger.LogFormat("Job {0} is Exiting the Queuing Network", p.ProcessNumber);
                        //Logger.LogFormat("Completed Proccess: {0} \t\t [C]", p);

                        _disk3QueueHandled.Add(p.ProcessNumber);
                    }
                    Thread.Sleep(100);
                }
                while (_executionContinue());
            });
        }

        private void Stop()
        {
            _stopwatch.Stop();

            ProcessScenarioStats();

            Logger.LogFormat("Length of Simulated Time : {0}", _stopwatch.Elapsed.TotalSeconds);
            Logger.LogFormat("Server 1 was Busy at Termination: {0}", _serverOne.IsBusy);
            Logger.LogFormat("Server 2 was Busy at Termination: {0}", _serverTwo.IsBusy);
            Logger.LogFormat("Server 3 was Busy at Termination: {0}", _serverThree.IsBusy);
            Logger.LogFormat("Tasks Completed : {0}", _jobsCompleted);
            Logger.LogFormat("Tasks Currently In The Queue: {0}", _processorQueue.Count);
            Logger.LogFormat("Throughput Rate (Tasks/Time): {0}", Convert.ToDouble(_jobsCompleted) / _stopwatch.Elapsed.TotalSeconds);
            Logger.LogFormat("Average Turnaround Time : {0}", (double)_stopwatch.Elapsed.TotalSeconds / (double)_jobsCompleted);
            Logger.LogFormat("Average Disk Queue Length: {0}", (_disk1QueueHandled.Count + _disk2QueueHandled.Count + _disk3QueueHandled.Count) / 3);
            Logger.LogFormat("Maximum Disk Queue Length: {0}", Math.Max(_disk1QueueHandled.Count, Math.Max(_disk2QueueHandled.Count, _disk3QueueHandled.Count)));      
            Logger.LogFormat("Maximum Task Queue Length: {0}", maximumQueueLength);
            Logger.LogFormat("Average Processor Service Time : {0}s", _serverProcessingTime / _jobsCompleted);
            Logger.LogFormat("Average Device Service Time : {0}s", _diskProcessingTime / _jobsCompleted);
            Logger.LogFormat("Percentage of Time Server 1 Busy: {0:P}", _serverOne.ProcessingTime / _serverProcessingTime);
            Logger.LogFormat("Percentage of Time Server 2 Busy: {0:P}", _serverTwo.ProcessingTime / _serverProcessingTime);
            Logger.LogFormat("Percentage of Time Server 3 Busy: {0:P}", _serverThree.ProcessingTime / _serverProcessingTime);
            Logger.LogFormat("Percentage of Time Disk 1 Busy: {0:P}", _diskOne.ProcessingTime / _diskProcessingTime);
            Logger.LogFormat("Percentage of Time Disk 2 Busy: {0:P}", _diskTwo.ProcessingTime / _diskProcessingTime);
            Logger.LogFormat("Percentage of Time Disk 3 Busy: {0:P}", _diskThree.ProcessingTime / _diskProcessingTime);

            /* Extra Stats */
            Logger.LogFormat("Minimum Queue Length: {0}", minimumQueueLength);       
            Logger.LogFormat("Average Disk Queue Length : {0}", (_disk1QueueHandled.Count + _disk2QueueHandled.Count + _disk3QueueHandled.Count) / 3);
        }

        private double CalculateRandom()
        {
            return _random.NextDouble();
        }

        private int CalculateRandomFromRange(int min, int max)
        {
            return _random.Next(min, max);
        }

        public void Run()
        {

            _stopwatch.Start();

            EnqueueProcess();
            ExecuteProcesses();
            Disk1Executor();
            Disk2Executor();
            Disk3Executor();
        }

        public void ExecuteAsync(Action action)
        {
            Task.Factory.StartNew(action);
        }

        private void ProcessScenarioStats()
        {
            Logger jobScenarioLogger = Logger.CreateLogger(filePath: "jobs.log", isOutputToConsole: false, appendDate: false);
            StringBuilder sb = new StringBuilder();
            foreach(Process p in _processes)
            {
                jobScenarioLogger.LogFormat("Job:{0} \t Inter-Arrival Time: {1:f2}, Task Execution Time: {2:f2}, Device Execution Time: {3:f2}", p.ProcessNumber, p.InterArrivalTime, p.TaskExecutionTime, p.DiskExecutionTime);
            }
        }

    }
}
