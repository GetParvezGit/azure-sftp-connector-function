using AzureSftpHelper.Contracts;
using AzureSftpHelper.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AzureSftpHelper
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AzureSftpHelperService(this IServiceCollection services)
        {

            services.AddScoped<ISftpWinSCPService, SftpWinSCPService>();
            services.AddScoped<ISftpSshNetService, SftpSshNetService>();
            services.AddScoped<ISftpSshKeyService, SftpSshKeyService>();

            return services;
        }
    }
}
