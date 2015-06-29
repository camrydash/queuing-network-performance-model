using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueuingNetwork_Peformance_Model
{
    internal class SafeExecution
    {
        private readonly string _containerName;
        private readonly object _locker = new object();
        private bool _isBusy = false;

        public SafeExecution()
            : this("Auto Generated")
        { }

        public SafeExecution(string containerName)
        {
            _containerName = containerName;
        }

        private void SetBusy()
        {
            _isBusy = true;
        }

        private void SetNotBusy()
        {
            _isBusy = true;
        }

        public void Execute(Action action)
        {
            Monitor.Enter(_locker);
            try
            {
                lock (_locker)
                {
                    SetBusy();
                    action();
                }
            }
            finally
            {
                SetNotBusy();
                Monitor.Exit(_locker);
            }
        }

        public T Execute<T>(Func<T> func)
        {
            Monitor.Enter(_locker);
            T value = default(T);
            try
            {
                SetBusy();
                value = func();
            }
            finally
            {
                SetNotBusy();
                Monitor.Exit(_locker);
            }
            return value;
        }

        public bool IsBusy
        {
            get { return _isBusy; }
        }

        public override string ToString()
        {
            return _containerName;
        }
    }
}
