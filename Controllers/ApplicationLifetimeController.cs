using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace DFTelegram.Controllers
{

    [ApiController]
    [Route("[controller]/[action]")]
    public class ApplicationLifetimeController : ControllerBase
    {
        public readonly IHostApplicationLifetime _hostApplicationLifetime;
        public ApplicationLifetimeController(IHostApplicationLifetime hostApplicationLifetime)
        {
            this._hostApplicationLifetime = hostApplicationLifetime;
        }

        [HttpGet]
        public IActionResult StopApplicaton()
        {
            _hostApplicationLifetime.StopApplication();
            return new EmptyResult();
        }

    }
}