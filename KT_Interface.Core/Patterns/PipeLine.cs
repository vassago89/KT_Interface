using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KT_Interface.Core.Patterns
{
    public abstract class PipeLine<T>
    {
        protected CancellationToken _token { get; set; }
        protected ManualResetEvent _resetEvent = new ManualResetEvent(false);

        protected int _maxCount { get; set; }
        protected bool _islastAccess { get; set; }

        private ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        protected ConcurrentQueue<T> Queue { get { return _queue; } }

        public int Count { get; set; }
        public bool IsRun { get { return _token == null ? false : _token.IsCancellationRequested == false; } }
        public EventHandler<Exception> ExceptionHanlder { get; set; }

        public abstract void Run(CancellationToken token);
        public abstract void Enqueue(T data);
    }

    public class SinglePipeLine<T> : PipeLine<T>
    {
        private Action<T> _job;

        public SinglePipeLine(Action<T> job, int maxCount = -1, bool islastAccess = false)
        {
            _job = job;
            _maxCount = maxCount;
            _islastAccess = islastAccess;
        }

        public override void Run(CancellationToken token)
        {
            _token = token;
            _token.Register(() => _resetEvent.Set());

            Task.Factory.StartNew(() =>
            {
                while (token.IsCancellationRequested == false)
                {
                    if (Queue.IsEmpty)
                        _resetEvent.Reset();

                    _resetEvent.WaitOne();

                    if (Queue.TryDequeue(out T data))
                    {
                        if (_islastAccess && Count > 1)
                        {
                            Count = Queue.Count;
                            continue;
                        }

                        try
                        {
                            _job(data);
                        }
                        catch (Exception e)
                        {
                            ExceptionHanlder?.Invoke(this, e);
                        }

                        Count = Queue.Count;
                    }

                }
                while (!Queue.IsEmpty)
                    Queue.TryDequeue(out var result);

                Count = Queue.Count;

            }, TaskCreationOptions.LongRunning);
        }

        public override void Enqueue(T data)
        {
            if (_token.IsCancellationRequested)
                return;

            if (_maxCount >= 0 && Count >= _maxCount)
                return;

            Queue.Enqueue(data);
            _resetEvent.Set();

            Count = Queue.Count;
        }
    }
}
