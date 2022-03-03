using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Has.WebMacro
{
    internal class FolderManager
    {
        internal DirectoryInfo _shadowFilesDirectoryInfo;
        internal DirectoryInfo _contentDirectoryInfo;
        internal DirectoryInfo _contentModuleVersionDirectoryInfo;
        internal string _hotInstallDirectoryInfo;

        public FolderManager(string location)
        {
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

    }
}
