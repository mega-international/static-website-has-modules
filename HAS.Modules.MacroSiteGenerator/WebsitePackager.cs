﻿using System;
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
        public async Task PackageModuleAsync(string ModuleFolderPath, string DestinationFolderPath, string ModuleVersion, ILogger logger)
        {
            var packager = new ModulePackager(ModuleFolderPath, "debug");
            var packageResult = await packager.CreatePackageAsync(ModuleVersion, false, false, DestinationFolderPath);
        }
    }
}
