﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Peformance_Model_Queue
{
    public class DiskProcessor : Processor
    {
        public void Process(Process p)
        {
            IsBusy = true;
            Thread.Sleep(TimeSpan.FromSeconds(p.DiskExecutionTime));
            IsBusy = false;
        }
    }
}
