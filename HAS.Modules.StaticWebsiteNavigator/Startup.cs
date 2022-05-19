using System.IO.Abstractions;
using Has.Modules.WebSite.ContentFileProvider;
using Mega.Has.Commons;
using Mega.Has.Instrumentation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Has.Modules.WebSite
{
    public class Startup
    {
        public const string StaticFilesFolder = "_static";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFileSystem>(new FileSystem());
            services.AddHASModule("1D961811-96A1-4FFF-B539-9A6DEED01617", opts =>
            {
                //opts.AuthenticationMode = AuthenticationMode.Cookie;
            });
           
            services.AddHASStaticWebContentFiles("website.static.content");

            var mvcBuilder = services
                .AddControllersWithViews()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();
        }

        public void Configure(IApplicationBuilder app, IModuleConfiguration moduleConfiguration, ITraceInstrumentation traceInstrumentation)
        {
            app.UseHASModule(moduleConfiguration, traceInstrumentation);

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHASStaticWebContentFiles($"/{StaticFilesFolder}");
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}