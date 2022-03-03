using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mega.Has.Commons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
/*using Mega.Has.Server.Cluster;*/

namespace Has.Modules.WebSite.ContentFileProvider
{
    public class StaticWebContentFileProvider : IFileProvider, IStaticWebContentModuleInfo
    {
        private readonly string _moduleId;
        private readonly string _resourcesFolder;
        private IFileProvider _physicalProvider;
        private Dictionary<string, FileProviderItem> _modules;

        public string PhysicalStaticContentFolder => (EnsuresProvider() as PhysicalFileProvider)?.Root;


        protected IServiceProvider Services { get; }

        public StaticWebContentFileProvider(IServiceProvider applicationServices, string moduleId, string resourcesFolder = "wwwroot")
        {
            _moduleId = moduleId ?? throw new ArgumentNullException(nameof(moduleId));
            Services = applicationServices ?? throw new ArgumentNullException(nameof(applicationServices));
            _resourcesFolder = resourcesFolder ?? throw new ArgumentNullException(nameof(resourcesFolder));
        }

        public IDirectoryContents GetDirectoryContents(string subpath) => NotFoundDirectoryContents.Singleton;

        public IFileInfo GetFileInfo(string subpath)
        {
            EnsuresProvider();
            var segments = subpath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var relativePath = Remainder(segments, 1);
            return _physicalProvider != null ? _physicalProvider.GetFileInfo(relativePath) : new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter) => NullChangeToken.Singleton;

        public IFileProvider EnsuresProvider()
        {
            if (_physicalProvider == null)
            {
                _physicalProvider = CreateProviderInternal(Services, _moduleId);
            }
            return _physicalProvider;
        }

        private IFileProvider CreateProviderInternal(IServiceProvider services, string moduleId)
        {

            FileProviderItem item = null;
            if (_modules == null || !_modules.TryGetValue(moduleId, out item))
            {
                try
                {
                    _modules = GetModuleInformations(services)
                        .ToDictionary(m => m.Description.Id, m => new FileProviderItem(m.Description, m.DebugFolder));
                    if (!_modules.TryGetValue(moduleId, out item))
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Serilog.Log.Logger.Error(ex, "HopexFileProvider failed");
                    _modules = null;
                }
            }

            if (item == null)
            {
                return null;
            }

            return CreateProvider(item);
        }

        protected IFileProvider CreateProvider(FileProviderItem item)
        {
            var configuration = Services.GetRequiredService<IClusterConfiguration>();
            var moduleFileProvider = CheckValidFolder(configuration.GetShadowsFolder(item.Manifest), _resourcesFolder);

            var providers = new List<IFileProvider>
            {
                // Orders is important
                CheckValidFolder(configuration.GetModuleCustomFolder(item.Manifest), _resourcesFolder),
                moduleFileProvider,
                CheckValidFolder(Path.Combine( configuration.ModulesFolder, item.Manifest.Id), _resourcesFolder)
            };

            var validProviders = providers.Where(p => p != null);
            return validProviders.Count() switch
            {
                0 => null,
                1 => validProviders.First(),
                _ => new CompositeFileProvider(validProviders),
            };
        }

        private static IFileProvider CheckValidFolder(string folder, string resourcesFolder, bool recursive = false)
        {
            if (folder == null)
            {
                return null;
            }
            var dir = new DirectoryInfo(folder);
            if (!dir.Exists)
            {
                return null;
            }

            do
            {
                var root = dir.EnumerateDirectories(resourcesFolder);
                if (root.Any()) return new PhysicalFileProvider(root.First().FullName);
                dir = dir.Parent;
            }
            while (dir != null && recursive);

            return null;
        }

        protected IEnumerable<ModuleInformation> GetModuleInformations(IServiceProvider services)
        {
            var hopexClient = services.GetRequiredService<IClusterAdminClient>();
            IEnumerable<ModuleInformation> modules = hopexClient.GetNodeModulesAsync().GetAwaiter().GetResult();
            return modules;
        }

        private string Remainder(string[] segments, int start)
        {
            var sb = new StringBuilder();
            for (var ix = start; ix < segments.Length; ix++)
            {
                if (ix > start)
                {
                    sb.Append("/");
                }
                sb.Append(segments[ix]);
            }
            return sb.ToString();
        }

        public void Reset()
        {
            _modules?.Clear();
            _physicalProvider = null;
        }
    }
}