using System.Reflection;
using Application.Services;
using Core;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Bootstrapper
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, string engineConfigurationFilePath)
        {
            EngineRouteConfiguration? engineConfiguration = null;
            var engineRouteConfigurationFileInfo = new FileInfo(engineConfigurationFilePath);

            var engineRouteConfigurationJsonStr = File.ReadAllText(engineConfigurationFilePath);
            engineConfiguration = JsonSerializer.Deserialize<EngineRouteConfiguration>(engineRouteConfigurationJsonStr);

            var universeDbFilePath = Path.Combine(engineRouteConfigurationFileInfo.Directory.FullName, engineConfiguration.routes_db);
            var universeDbFile = new FileInfo(universeDbFilePath);
            
            services.AddDbContext<UniverseDbContext>(options => options.UseSqlite($"DataSource={universeDbFile}"));

            services.AddTransient(typeof(IUniverseRepository), typeof(UniverseRepository));
            services.AddTransient<ITravelService>(m => new TravelService(m.GetRequiredService<IUniverseRepository>(), engineConfiguration));

            return services;
        }
    }
} 