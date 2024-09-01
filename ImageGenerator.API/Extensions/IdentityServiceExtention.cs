using ImageGenerator.Core.Entities.Identity;
using ImageGenerator.Core.TokenService;
using ImageGenerator.Repository.Identity;
using ImageGenerator.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ImageGenerator.API.Extensions
{
    public static class IdentityServiceExtention
    {
        public static IServiceCollection AddIdentityService(this IServiceCollection Services, IConfiguration configuration)
        {

            Services.AddIdentity<AppUser, IdentityRole>()
                            .AddEntityFrameworkStores<AppIdentityDbContext>()
                            .AddDefaultTokenProviders();

            Services.AddScoped<ITokenService, TokenService>();

            // I changed the defautl scheme, so I have to estimate it here
            // I have to set the Challenge Scheme
            
            Services.AddAuthentication(Options =>
            {
                Options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                Options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                            .AddJwtBearer(Options =>
                            {
                                Options.TokenValidationParameters = new TokenValidationParameters()
                                {
                                    ValidateIssuer = true,
                                    ValidIssuer = configuration["JWT:ValidIssuer"],
                                    ValidateAudience = true,
                                    ValidAudience = configuration["JWT:ValidAudience"],
                                    ValidateLifetime = true,
                                    ValidateIssuerSigningKey = true,
                                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"])),
                                };
                            }
                );
            return Services;
        }
    }
}
