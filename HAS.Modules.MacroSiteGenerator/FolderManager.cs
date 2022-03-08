using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using Hopex.ApplicationServer.WebServices;

namespace Has.WebMacro
{
    internal class FolderManager
    {
        internal DirectoryInfo _shadowFilesDirectoryInfo;
        internal DirectoryInfo _contentDirectoryInfo;
        internal DirectoryInfo _contentModuleVersionDirectoryInfo;
        internal string _hotInstallDirectoryInfo;
        private static ILogger _logger;


        public FolderManager(string location, ILogger logger)
        {
            _logger = logger;
            var shadowfilesFolderPath = Path.GetFullPath(Path.Combine(location, @"..\..\..\..\"));
            _shadowFilesDirectoryInfo = new DirectoryInfo(shadowfilesFolderPath);
            _hotInstallDirectoryInfo = Path.GetFullPath(Path.Combine(location, @"..\..\..\..\..\Modules\.hot-install"));
            var contentModuleFolderPath = Path.GetFullPath(Path.Combine(shadowfilesFolderPath, "website.static.content"));
            _contentDirectoryInfo = new DirectoryInfo(contentModuleFolderPath);
            _contentModuleVersionDirectoryInfo = _contentDirectoryInfo.GetDirectories()
                .OrderByDescending(d => d.LastWriteTimeUtc).First();
        }

        internal bool ShadowFileExists() => _shadowFilesDirectoryInfo.Exists;
        internal bool ContentFileExists() => _contentDirectoryInfo.Exists;

        public HopexResponse VerifyCorrectFilesAvailable()
        {
            ErrorMacroResponse? result = null;
            if (!ShadowFileExists())
            {
                _logger.LogInformation("This feature is only available for a version of HOPEX V5 and above.");
                result = new ErrorMacroResponse(HttpStatusCode.InternalServerError, "This feature is only available for a version of HOPEX V5 and above.");
                return HopexResponse.Json(JsonSerializer.Serialize(result));
            }
            if (!ContentFileExists())
            {
                _logger.LogInformation("This feature is only available for the static website module. Please install it before and/or make sure that the module is running.");
                result = new ErrorMacroResponse(HttpStatusCode.InternalServerError, "This feature is only available for the static website module. Please install it before and/or make sure that the module is running.");
                return HopexResponse.Json(JsonSerializer.Serialize(result));
            }

            return null;

        }

    }
}
