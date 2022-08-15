using DFTelegram.BackgroupTaskService.QueueService;
using DFTelegram.Models;
using DFTelegram.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TL;

namespace DFTelegram.QueueBase.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]/[action]")]
    public class DocumentController : QueueBaseController<Document>
    {
        // private readonly IQueueBase<Document> _mediaQueueBase;
        public DocumentController(IQueueBase<Document> mediaQueueBase)
        : base(mediaQueueBase)
        {
            // this._mediaQueueBase = mediaQueueBase;
        }
    }
}