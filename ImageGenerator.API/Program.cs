using ImageGenerator.API.Errors;
using ImageGenerator.API.Extensions;
using ImageGenerator.API.Helpers;
using ImageGenerator.API.Middlewares;
using ImageGenerator.Core.EmailSettings;
using ImageGenerator.Core.Entities.Identity;
using ImageGenerator.Repository;
using ImageGenerator.Repository.Identity;
using ImageGenerator.Service;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ImageGenerator.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            #region Configure Service

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            
           
            builder.Services.AddScoped<DownloadImages>();

            builder.Services.AddDbContext<AppIdentityDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

            //validationError
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    var erros = actionContext.ModelState.Where(E => E.Value.Errors.Count() > 0)
                                            .SelectMany(E => E.Value.Errors)
                                            .Select(E => E.ErrorMessage)
                                            .ToArray();

                    var validationErrorResponse = new ValidationErrorResponse()
                    {
                        Errors = erros
                    };
                    return new BadRequestObjectResult(validationErrorResponse);
                };
            });
            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
            builder.Services.AddTransient<IEmailService, EmailService>();
            builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = GoogleDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;

            })
                .AddGoogle(o =>
                {
                    IConfiguration GoogleAuthSection = builder.Configuration.GetSection("Authentication:Google");
                    o.ClientId = GoogleAuthSection["ClientId"];
                    o.ClientSecret = GoogleAuthSection["ClientSecret"];
                });


            builder.Services.AddHttpClient();

            // Extension Method
            builder.Services.AddIdentityService(builder.Configuration);

            builder.Services.AddAuthentication();
            #endregion

            var app = builder.Build();



            #region Update database
            
            using var Scope = app.Services.CreateScope();

            var Services = Scope.ServiceProvider;
            var LoggerFactory = Services.GetRequiredService<ILoggerFactory>();
            try
            {
                
                var IdentitydbContext = Services.GetRequiredService<AppIdentityDbContext>();

                await IdentitydbContext.Database.MigrateAsync();

            }
            catch (Exception ex)
            {
                var Logger = LoggerFactory.CreateLogger<Program>();
                Logger.LogError(ex, "An Error while applying Migration");
            }
            #endregion


            app.UseMiddleware<ExceptionMiddleware>();
            app.UseSwagger();
            app.UseSwaggerUI();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                
            }
            app.UseStatusCodePagesWithReExecute("/errors/{0}");
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}