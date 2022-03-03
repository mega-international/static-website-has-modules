using Mega.Has.Commons;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Has.Modules.WebSite.ContentFileProvider
{
    public static class StaticWebContentFolderExtensions
    {
        public static IServiceCollection AddHASStaticWebContentFiles(this IServiceCollection services, string moduleId)
        {
            services.AddSingleton<IStaticWebContentModuleInfo>(services => new StaticWebContentFileProvider(services, moduleId));
            return services;
        }

        public static IApplicationBuilder UseHASStaticWebContentFiles(this IApplicationBuilder builder, string path)
        {
            var module = builder.ApplicationServices.GetRequiredService<IModuleConfiguration>();
            builder.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = path,
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Add("Cache-Control", "no-cache");
                    if (!ctx.Context.User.Identity.IsAuthenticated)
                    {
                        ctx.Context.Response.Redirect($"/{module.Manifest.PathPrefix}/home");
                    }
                },
                FileProvider = builder.ApplicationServices.GetRequiredService<IStaticWebContentModuleInfo>() as IFileProvider
            }); 


            return builder;//.UseMiddleware<ProtectFolder>(new ProtectFolderOptions(path, policyName));

        }
    }
}