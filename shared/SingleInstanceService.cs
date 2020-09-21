using System;
using System.Threading;

namespace shared
{
    /// <summary>
    /// Features:
    ///     Sets a local mutext for the first running instance.
    ///     The option to send command(s) from the second instance the first.
    ///
    /// </summary>
    public class SingleInstanceService : IDisposable
    {
        private Mutex _mutex;
        
        public bool Start()
        {
            if (_mutex != null)
                throw new InvalidOperationException("Mutext has all ready been set. Please call Stop to release the mutext.");

            _mutex = new Mutex(true, "some-unique_id-" + Environment.UserName, out var firstInstance);

            return firstInstance;
        }

        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_mutex == null) return;
            
            _mutex.ReleaseMutex();
            _mutex.Close();
            _mutex = null;
        }
    }
}
