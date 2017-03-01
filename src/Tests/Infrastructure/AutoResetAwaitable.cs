using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Tests.Infrastructure
{
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