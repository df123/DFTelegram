using DFTelegram.BackgroupTaskService.QueueService;
using DFTelegram.Models;
using DFTelegram.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TL;

namespace DFTelegram.QueueBase.Controllers
{
    [Authorize]
    public class QueueBaseController<T> : ControllerBase
    {
        private readonly IQueueBase<T> _queueBase;
        public QueueBaseController(IQueueBase<T> queueBase)
        {
            this._queueBase = queueBase;
        }

        [HttpGet]
        public IActionResult GetCount()
        {
            return Ok(_queueBase.GetConcurrentQueueCount());
        }

        [HttpGet]
        public IActionResult GetArray()
        {
            return Ok(_queueBase.GetArray());
        }

    }
}