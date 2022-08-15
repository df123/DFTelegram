using DFTelegram.Helper;

namespace DFTelegram.Services
{
	public class UserService
	{
		private readonly JwtTokenHelper _jwtTokenHelper;

		public UserService(JwtTokenHelper jwtTokenHelper)
		{
			_jwtTokenHelper = jwtTokenHelper;
		}

		public string Login(string name, string password)
		{
			if (name != AppsettingsHelper.app("User", "Name") && password != AppsettingsHelper.app("User", "Password"))
			{
				return string.Empty;
			}
			return _jwtTokenHelper.GeneratorToken(name);
		}
	}
}