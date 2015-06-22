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
        private readonly IEnumerable<Process> _processes;

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
        private readonly ServerProcessor _serverOne = new ServerProcessor();
        private readonly ServerProcessor _serverTwo = new ServerProcessor();
        private readonly ServerProcessor _serverThree = new ServerProcessor();

        /// <summary>
        /// Disk Processor D1, D2 & D3
        /// </summary>
        private readonly DiskProcessor _diskOne = new DiskProcessor();
        private readonly DiskProcessor _diskTwo = new DiskProcessor();
        private readonly DiskProcessor _diskThree = new DiskProcessor();

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly Random _random;

        private bool _executionStop = true;
        private object _locker = new object();

        public List<int> _processorQueueHandled;
        public List<int> _serverProcessesorQueueHandled;
        public List<int> _disk1QueueHandled;
        public List<int> _disk2QueueHandled;
        public List<int> _disk3QueueHandled;


        public QueuingNetwork(IEnumerable<Process> initialJobs, bool isInitiallySuspended = false)
        {
            ///Initialize the Processor Queue with the first 50 jobs
            _processes = initialJobs;
            _executionStop = isInitiallySuspended;
            _random = new Random();
            _processorQueue = new Queue<Process>();

            _processorQueueHandled = new List<int>();
            _serverProcessesorQueueHandled = new List<int>();
            _disk1QueueHandled = new List<int>(); ;
            _disk2QueueHandled = new List<int>();
            _disk3QueueHandled = new List<int>();
        }

        public void Start()
        {
            _executionStop = false;
        }

        private bool IsServerAvailable
        {
            get { return !_serverOne.IsBusy || !_serverTwo.IsBusy || !_serverThree.IsBusy; }
        }

        private Queue<Process> GetNextAvailableDisk()
        {
            int diskNumber = _random.Next(1, 3);
            Logger.LogFormat("Retreving Disk: {0}", 1);
            switch (diskNumber)
            {
                case 1:
                    return _disk1Queue;
                case 2:
                    return _disk2Queue;
                case 3:
                    return _disk3Queue;
                default:
                    throw new ArgumentException(string.Format("Disk: {0} does not exist.", diskNumber));
            }
        }

        private ServerProcessor GetNextAvailableServer()
        {
            if (!_serverOne.IsBusy)
            {
                Logger.LogFormat("Server: {0} Is Available!", 1);
                return _serverOne;
            }

            if (!_serverTwo.IsBusy)
            {
                Logger.LogFormat("Server: {0} Is Available!", 2);
                return _serverTwo;
            }

            if (!_serverThree.IsBusy)
            {
                Logger.LogFormat("Server: {0} Is Available!", 3);
                return _serverThree;
            }

            throw new ArgumentException("No Server Process Is Free!");
        }

        private TimeSpan CalculateProcessInterArrivalTime(Process p)
        {
            //TimeSpan arrivalTime;
            //int currentProcessNumber = p.ProcessNumber;
            //if (currentProcessNumber == 1)
            //{
            //    arrivalTime = TimeSpan.FromSeconds(p.InterArrivalTime);
            //}
            //else
            //{
            //    Process previousProcessContext = _processes.ElementAt((currentProcessNumber - 1) - 1);
            //    arrivalTime = TimeSpan.FromSeconds(previousProcessContext.InterArrivalTime) +
            //                  TimeSpan.FromSeconds(p.InterArrivalTime);
            //}
            //return arrivalTime;
            return TimeSpan.FromSeconds(p.InterArrivalTime);
        }

        /// <summary>
        /// Simulates the arrival of processes and puts them into the processor queue
        /// </summary>
        private void EnqueueProcessCollection()
        {
            Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < _processes.Count(); i++)
                {
                    Process p = _processes.ElementAt(i);
                    Logger.LogFormat("Adding Process: {0} To Processor Queue", p);

                    TimeSpan arrivalTime = CalculateProcessInterArrivalTime(p);
                    Thread.Sleep(arrivalTime);

                    _processorQueue.Enqueue(p);
                    Logger.LogFormat("Enqueued Process: {0}, Wait: {1}s", p, arrivalTime.TotalSeconds);

                    lock (_locker)
                    {
                        _processorQueueHandled.Add(p.ProcessNumber);
                    }
                }
                Logger.Log("Enqueued All Process!");
            });
        }

        /// <summary>
        /// Simulates a thread for S1, S2 & S3 which extracts a process from the Process Queue, process the task, and put in a disk queue (random)
        /// </summary>
        private void ExecuteProcesses()
        {
            Task.Factory.StartNew(() =>
            {
                bool continueExpression = true;
                while (continueExpression)
                {
                    // continue until there are no waiting jobs in the processor queue
                    if (_processorQueue.Count > 0)
                    {
                        if (IsServerAvailable)
                        {
                            ServerProcessor processor = GetNextAvailableServer();

                            ///Dequeue the process and get the server processor to process it
                            Process process = _processorQueue.Dequeue();
                            Logger.LogFormat("Dequeue: {0}\t\t[U]", process);

                            Logger.LogFormat("Processing: {0}", process);
                            new Action(() =>
                            {
                                processor.Process(process);
                            }).BeginInvoke(null, null);

                            _serverProcessesorQueueHandled.Add(process.ProcessNumber);

                            lock (_locker)
                            {
                                continueExpression = _processorQueueHandled.Count < _processes.Count();
                            }

                            ///After processing the process, we need to put in a disk queue
                            ///Disk Processor may be requesting the same resource while enquing, we need to lock the resource
                            lock (_locker)
                            {
                                Queue<Process> diskQueue = GetNextAvailableDisk();
                                Logger.Log("Enqueuing Process in Disk");
                                diskQueue.Enqueue(process);
                            }
                        }
                        else
                        {
                            //Logger.Log("No Server Is Available!");
                        }
                    }
                }
                Logger.Log("Processed All Processes\t\t[S]");
            });
        }

        private void Disk1Executor()
        {
            Task.Factory.StartNew(() =>
            {
                var continueExpression = true;
                while (continueExpression)
                {
                    if (_disk1Queue.Count > 0)
                    {
                        Process p = _disk1Queue.Dequeue();
                        Logger.LogFormat("Dequeuing Process in Disk 1: {0}", p);


                        Logger.LogFormat("Processing Process: {0} in Disk 1", p);
                        DiskProcessor processor = _diskOne;
                        new Action(() =>
                        {
                            processor.Process(p);
                        }).BeginInvoke(null, null);
                        
                        Logger.LogFormat("Completed Proccess: {0} \t\t [C]", p);
                        MarkComplete(p);

                        _disk1QueueHandled.Add(p.ProcessNumber);
                    }
                    else
                    {
                        //Logger.Log("Disk 1 Queue is Empty!");
                    }

                    lock (_locker)
                    {
                        continueExpression = _disk1QueueHandled.Count + _disk2QueueHandled.Count + _disk3QueueHandled.Count < _processes.Count();
                    }

                    Thread.Sleep(50);
                }

            });
        }

        private void Disk2Executor()
        {
            Task.Factory.StartNew(() =>
            {
                var continueExpression = true;
                while (continueExpression)
                {
                    if (_disk2Queue.Count > 0)
                    {
                        Process p = _disk2Queue.Dequeue();
                        Logger.LogFormat("Dequeuing Process in Disk 2: {0}", p);

                        Logger.LogFormat("Processing Process: {0} in Disk 2", p);
                        DiskProcessor processor = _diskTwo;
                        new Action(() =>
                        {
                            processor.Process(p);
                        }).BeginInvoke(null, null);

                        Logger.LogFormat("Completed Proccess: {0} \t\t [C]", p);
                        MarkComplete(p);

                        _disk2QueueHandled.Add(p.ProcessNumber);
                    }
                    else
                    {
                        //Logger.Log("Disk 2 Queue is Empty!");
                    }

                    lock (_locker)
                    {
                        continueExpression = _disk1QueueHandled.Count + _disk2QueueHandled.Count + _disk3QueueHandled.Count < _processes.Count();
                    }

                    Thread.Sleep(50);
                }
            });
        }

        private void Disk3Executor()
        {
            Task.Factory.StartNew(() =>
            {
                var continueExpression = true;
                while (continueExpression)
                {
                    if (_disk3Queue.Count > 0)
                    {
                        Process p = _disk3Queue.Dequeue();
                        Logger.LogFormat("Dequeuing Process in Disk 3: {0}", p);


                        Logger.LogFormat("Processing Process: {0} in Disk 3", p);
                        DiskProcessor processor = _diskThree;
                        new Action(() =>
                        {
                            processor.Process(p);
                        }).BeginInvoke(null, null);
             
                        Logger.LogFormat("Completed Proccess: {0} \t\t [C]", p);
                        MarkComplete(p);

                        _disk3QueueHandled.Add(p.ProcessNumber);
                    }
                    else
                    {
                        //Logger.Log("Disk 3 Queue is Empty!");
                    }

                    lock (_locker)
                    {
                        continueExpression = _disk1QueueHandled.Count + _disk2QueueHandled.Count + _disk3QueueHandled.Count < _processes.Count();
                    }

                    Thread.Sleep(50);
                }
            });
        }

        private void MarkComplete(Process p)
        {
            p.State = ProcessState.Complete;

            if (!_processes.Any(proc => proc.State == ProcessState.Pending))
            {
                Stop();

                Logger.LogFormat("Throughput Rate Tasks/Time: {0}", Convert.ToDouble(_processes.Count())/_stopwatch.Elapsed.TotalSeconds);
                Logger.LogFormat("Length of Simulated Time : {0}", _stopwatch.Elapsed.TotalSeconds);
                Logger.LogFormat("Tasks Completed : {0}", _processes.Count());
            }
        }

        private void Stop()
        {
            _stopwatch.Stop();
        }

        public void Run()
        {
            //while (true)
            //{
            if (!_executionStop)
            {
                _stopwatch.Start();

                ///Threads
                EnqueueProcessCollection();
                ExecuteProcesses();
                Disk1Executor();
                Disk2Executor();
                Disk3Executor();
            }
            else
            {
                Logger.Log("Queuing Network in Suspended State");
                Thread.Sleep(10000);
            }
            //}
        }
    }
}
