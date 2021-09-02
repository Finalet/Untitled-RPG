using System.Collections.Concurrent;

namespace QFSW.QC
{
    public class CustomLogQueue : ILogQueue
    {
        private readonly ConcurrentQueue<ILog> _queuedLogs = new ConcurrentQueue<ILog>();
        
        public int MaxStoredLogs { get; set; }
        public bool IsEmpty => _queuedLogs.IsEmpty;

        public CustomLogQueue(int maxStoredLogs = -1)
        {
            MaxStoredLogs = maxStoredLogs;
        }

        public void QueueLog(ILog log)
        {
            _queuedLogs.Enqueue(log);
            if (MaxStoredLogs > 0 && _queuedLogs.Count > MaxStoredLogs)
            {
                _queuedLogs.TryDequeue(out _);
            }
        }

        public bool TryDequeue(out ILog log)
        {
            return _queuedLogs.TryDequeue(out log);
        }

        public void Clear()
        {
            while (TryDequeue(out ILog _)) { }
        }
    }
}