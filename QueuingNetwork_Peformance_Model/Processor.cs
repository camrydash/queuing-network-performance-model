using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peformance_Model_Queue
{
    public abstract class Processor
    {
        public bool IsBusy { get; set; }
    }
}
