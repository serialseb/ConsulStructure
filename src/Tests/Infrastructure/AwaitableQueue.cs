using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests.Infrastructure
{
  public class AwaitableQueue<T>
  {
    readonly AutoResetAwaitable awaitable = new AutoResetAwaitable() { };
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

    public int Count => queue.Count;
  }
}