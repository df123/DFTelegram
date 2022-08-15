using DFTelegram.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DFTelegram.Controllers
{

    [Authorize]
    [ApiController]
    [Route("[controller]/[action]")]
    public class SpaceController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetRootAvailableMB()
        {
            return Ok(SpaceHelper.GetRootAvailableMB());
        }
    }
}