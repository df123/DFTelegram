using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DFTelegram.Helper
{
	public class JwtTokenHelper
	{
		public string GeneratorToken(string name)
		{
			JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
			byte[] bytes = Encoding.UTF8.GetBytes(AppsettingsHelper.app("JWT", "Key"));
			SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor();
			securityTokenDescriptor.Subject = new ClaimsIdentity(new Claim[1]
			{
				new Claim(ClaimTypes.Name, name)
			});
			securityTokenDescriptor.Expires = DateTime.Now.AddDays(30.0);
			securityTokenDescriptor.SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(bytes), SecurityAlgorithms.HmacSha256Signature);
			SecurityTokenDescriptor tokenDescriptor = securityTokenDescriptor;
			SecurityToken token = jwtSecurityTokenHandler.CreateToken(tokenDescriptor);
			return jwtSecurityTokenHandler.WriteToken(token);
		}
	}
}
