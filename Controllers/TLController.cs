using Microsoft.AspNetCore.Mvc;
using DFTelegram.Services;
using Microsoft.AspNetCore.Authorization;

namespace DFTelegram.Controllers
{
    [Authorize]
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