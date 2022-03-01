using System;
using System.Threading.Tasks;
using Mega.Has.Commons;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Has.Modules.WebSite
{
    public class Program
    {
        private static ModuleConfiguration _moduleConfiguration;

        public static async Task Main(string[] args)
        {
            try
            {
                // For debugging in visual studio
                // Start this project directly with a --attach-to=<has-address> argument
                // has-address must be a local running HAS instance address
                _moduleConfiguration = await ModuleConfiguration.CreateAsync(args);
                await CreateHostBuilder(args).Build().RunAsync();
            }
            catch(Exception ex)
            {
                PreloadLogger.LogError(ex.Message);
                Log.CloseAndFlush();
                throw;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var mc = _moduleConfiguration ?? new ModuleConfiguration(args);
            var folder = mc.Folder;

            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseHASInstrumentation(mc)
                        .UseUrls(mc.ServerInstanceUrl)
                        .UseContentRoot(folder)
                        .UseKestrel((options) =>
                        {
                            // Do not add the Server HTTP header.
                            options.AddServerHeader = false;
                        })
                        .UseStartup<Startup>();
                });

            return builder;
        }
    }
}