using ImageGenerator.Core.Entities.Identity;
using ImageGenerator.Core.TokenService;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ImageGenerator.Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> CreateTokenAsync(AppUser User, UserManager<AppUser> UserManager)
        {
            var AuthClaims = new List<Claim>()
                {
                new Claim(ClaimTypes.GivenName, User.Name),
                new Claim(ClaimTypes.Email, User.Email)
            };

            var Roles = await UserManager.GetRolesAsync(User);
            foreach (var Role in Roles)
            {
                AuthClaims.Add(new Claim(ClaimTypes.Role, Role));
            }
            var AuthKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

            var Token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddDays(double.Parse(_configuration["JWT:DuraionInDays"])),
                claims: AuthClaims,
                signingCredentials: new SigningCredentials(AuthKey, SecurityAlgorithms.HmacSha256Signature)
                );
            return new JwtSecurityTokenHandler().WriteToken(Token);
        }
    }
}
