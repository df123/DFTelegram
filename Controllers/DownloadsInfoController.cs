using System.Text;
using DFTelegram.BackgroupTaskService.QueueService;
using DFTelegram.Helper;
using DFTelegram.Models;
using DFTelegram.Models.DTO.Input;
using DFTelegram.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TL;

namespace DFTelegram.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]/[action]")]
    public class DownloadsInfoController : ControllerBase
    {
        private readonly DownloadsInfoService _downloadsInfoService;
        private readonly ILogger<DownloadsInfoController> _logger;

        public DownloadsInfoController(ILogger<DownloadsInfoController> logger,
        DownloadsInfoService downloadsInfoService)
        {
            this._downloadsInfoService = downloadsInfoService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Delete(DownloadsInfo downloadsInfo)
        {
            int result = await _downloadsInfoService.DeleteByAccessHashID(downloadsInfo);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            DownloadsInfo[] downloadsInfos = await _downloadsInfoService.GetAll();
            return Ok(downloadsInfos);
        }

        [HttpPost]
        public async Task<IActionResult> GetByConditional(QueryConditionalsDTO dto)
        {
            if(dto == null || dto.conditionals == null){
                return BadRequest();
            }
            DownloadsInfo[] result = await _downloadsInfoService.GetByConditional(dto.conditionals);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetBandwidth()
        {
            return Ok(StorageUnitConversionHelper.ByteToMB(await _downloadsInfoService.GetDownloadsSize()));
        }

        [HttpGet]
        public async Task<IActionResult> GetDownloadsAddress()
        {
            DownloadsInfo[] downloads = await _downloadsInfoService.GetDownloadsNotDelete();
            StringBuilder sb = new StringBuilder(1024);
            foreach (DownloadsInfo info in downloads)
            {
                sb.Append(AppsettingsHelper.app(new string[] { "RunConfig", "ReturnDownloadUrlPrefix" }));
                if (info.SavePath.Contains("Photo"))
                {
                    sb.Append("Photo/");
                }
                else
                {
                    sb.Append("Video/");
                }
                sb.Append(Path.GetFileName(info.SavePath));
                sb.AppendLine();
            }
            return Ok(sb.ToString());
        }
    }
}