using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DFTelegram.BackgroupTaskService.QueueService
{
    public interface IQueueBase<T>
    {
        public int GetConcurrentQueueCount();
        public int GetSemaphoreSlimCount();
        public void AddItem(T model);
        public Task<T> GetItemAsync(CancellationToken cancellationToken);
        public T[] GetArray();
    }
}
