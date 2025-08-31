using AutomationLetterWriting.Context;
using AutomationLetterWriting.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AutomationLetterWriting.Extensions
{
    public static class ApiRegistrationServices
    {
        public static IServiceCollection ConfigurationApiService
            (this IServiceCollection services, IConfiguration configuration)
        {

            // ثبت IHttpContextAccessor
            services.AddHttpContextAccessor();

            services.AddDbContext<ApplicationDbContext>(op =>
            op.UseSqlServer(configuration.GetConnectionString("AutomationConnectionString")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]))
                };
            });

            services.AddAuthorization();


            return services;
        }
    }
}
