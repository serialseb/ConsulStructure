using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsulStructure.Tests.Infrastructure
{
    public class AwaitableQueue<T>
    {
        readonly AutoResetAwaitable awaitable = new AutoResetAwaitable(){};
        readonly Queue<T> queue = new Queue<T>();

        public void Enqueue(T value)
        {
            queue.Enqueue(value);
            awaitable.Signal();
        }

        public async Task<T> Dequeue()
        {
            await awaitable.WaitOne();
            return queue.Dequeue();
        }
    }
    public class AutoResetAwaitable
    {
        readonly ConcurrentQueue<TaskCompletionSource<bool>> _onwrite;
        bool _signaled;

        public AutoResetAwaitable()
        {
            _onwrite = new ConcurrentQueue<TaskCompletionSource<bool>>();
        }

        public void Signal()
        {
            _signaled = true;
            TaskCompletionSource<bool> signal;
            if (!_onwrite.TryDequeue(out signal)) return;

            signal.TrySetResult(true);
            _signaled = false;
        }
        public Task WaitOne()
        {
            if (_signaled)
            {
                _signaled = false;
                return Task.CompletedTask;
            }
            var tcs = new TaskCompletionSource<bool>();
            _onwrite.Enqueue(tcs);
            return tcs.Task;
        }
    }
}