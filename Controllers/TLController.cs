using Microsoft.AspNetCore.Mvc;
using DFTelegram.Services;

namespace DFTelegram.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TLController : ControllerBase
    {
        private readonly TLConfigService _tlConfigService;
        public TLController(TLConfigService tlConfigService)
        {
            _tlConfigService = tlConfigService;
        }

        [HttpPost]
        public IActionResult SetVerificationCode(string verificationCode)
        {
            _tlConfigService.SetVerificationCode(verificationCode);
            return Ok();
        }

    }
}