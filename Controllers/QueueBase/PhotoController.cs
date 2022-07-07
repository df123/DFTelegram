using DFTelegram.BackgroupTaskService.QueueService;
using DFTelegram.Models;
using DFTelegram.Services;
using Microsoft.AspNetCore.Mvc;
using TL;

namespace DFTelegram.QueueBase.Controllers
{

    [ApiController]
    [Route("[controller]/[action]")]
    public class PhotoController : QueueBaseController<Photo>
    {
        // private readonly IQueueBase<Photo> _photoQueueBase;
        public PhotoController(IQueueBase<Photo> photoQueueBase)
        :base(photoQueueBase)
        {
            // this._photoQueueBase = photoQueueBase;
        }

        // public IActionResult GetCount()
        // {
        //     return Ok(_photoQueueBase.GetConcurrentQueueCount());
        // }

        // public IActionResult GetArray()
        // {
        //     return Ok(_photoQueueBase.GetArray());
        // }

    }
}