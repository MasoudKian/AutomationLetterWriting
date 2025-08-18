using AutomationLetterWriting.Context;
using AutomationLetterWriting.Models;
using Microsoft.EntityFrameworkCore;

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



            return services;
        }
    }
}
