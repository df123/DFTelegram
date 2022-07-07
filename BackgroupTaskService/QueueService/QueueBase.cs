using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DFTelegram.BackgroupTaskService.QueueService
{
    public class QueueBase<T> : IQueueBase<T>
    {
        private ConcurrentQueue<T> _receiveItems;
        private SemaphoreSlim _signal;

        public QueueBase()
        {
            _receiveItems = new ConcurrentQueue<T>();
            _signal = new SemaphoreSlim(0);
        }

        public async Task<T> GetItemAsync(CancellationToken cancellationToken)
        {
            #nullable disable
            await _signal.WaitAsync(cancellationToken);
            _receiveItems.TryDequeue(out T item);
            return item;
            #nullable restore
        }

        public int GetConcurrentQueueCount()
        {
            return _receiveItems.Count;
        }

        public int GetSemaphoreSlimCount()
        {
            return _signal.CurrentCount;
        }

        public void AddItem(T model)
        {
            if (model == null)
            {
                return;
            }

            _receiveItems.Enqueue(model);
            _signal.Release();
        }

        public T[] GetArray()
        {
            return _receiveItems.ToArray<T>();
        }
    }
}
