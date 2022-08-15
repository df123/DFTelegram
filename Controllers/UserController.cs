using DFTelegram.Models.DTO.Input;
using DFTelegram.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DFTelegram.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]/[action]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(LoginInputDTO loginInput)
        {
            if (loginInput == null || string.IsNullOrEmpty(loginInput.Name) || string.IsNullOrEmpty(loginInput.Password))
            {
                return BadRequest();
            }
            string value = _userService.Login(loginInput.Name, loginInput.Password);
            if (string.IsNullOrEmpty(value))
            {
                return NotFound("用户名或者密码错误");
            }
            return Ok(value);
        }

        [HttpGet]
        public IActionResult Test()
        {
            return Ok("ok");
        }
    }
}
