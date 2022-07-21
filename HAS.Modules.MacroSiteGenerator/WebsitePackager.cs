using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Hopex.ApplicationServer.WebServices;
using Mega.Extensions.Packages;

namespace Has.WebMacro
{
    internal class WebsitePackager
    {
        public WebsitePackager(){}
        public async Task PackageModuleAsync(string ModuleFolderPath, string DestinationFolderPath, string ModuleVersion)
        {
            var packager = new ModulePackager(ModuleFolderPath, "Release");
            var packageResult = await packager.CreatePackageAsync(ModuleVersion, false, false, DestinationFolderPath);
        }
    }
}
