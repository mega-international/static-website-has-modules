
using Mega.Extensions.Packages;

namespace Has.Modules.WebSite.ContentFileProvider
{
    public class FileProviderItem
    {
        public FileProviderItem(ModuleManifest manifest, string debugFolder)
        {
            Manifest = manifest;
            DebugFolder = debugFolder;
        }

        public ModuleManifest Manifest { get; }
        public string DebugFolder { get; }
    }
}