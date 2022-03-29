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
        private const string WebsiteContentId = "website.static.content";
        private const string HotInstallFolderName = ".hot-install";
        private const string InstanceModulesFolder = "Modules";

        private readonly DirectoryInfo _shadowFilesDirectoryInfo;
        private readonly DirectoryInfo _contentDirectoryInfo;
        internal DirectoryInfo contentModuleVersionDirectoryInfo;
        internal string hotInstallDirectoryInfo;
        private static ILogger _logger;


        public  FolderManager(){}
        public FolderManager(string location, ILogger logger)
        {
            _logger = logger;
            var shadowfilesFolderPath = Path.GetFullPath(Path.Combine(location, @"..\..\..\..\"));
            _shadowFilesDirectoryInfo = new DirectoryInfo(shadowfilesFolderPath);
            hotInstallDirectoryInfo = Path.GetFullPath(Path.Combine(location, String.Format(@"..\..\..\..\..\{0}\{1}", InstanceModulesFolder, HotInstallFolderName)));
            var contentModuleFolderPath = Path.GetFullPath(Path.Combine(shadowfilesFolderPath, WebsiteContentId));
            _contentDirectoryInfo = new DirectoryInfo(contentModuleFolderPath);
            contentModuleVersionDirectoryInfo = _contentDirectoryInfo.GetDirectories()
                .OrderByDescending(d => d.LastWriteTimeUtc).First();
        }

        internal bool ShadowFileExists() => _shadowFilesDirectoryInfo.Exists;
        internal bool ContentFileExists() => _contentDirectoryInfo.Exists;

        public ErrorMacroResponse VerifyCorrectFilesAvailable()
        {
            ErrorMacroResponse? result = null;
            if (!ShadowFileExists())
            {
                _logger.LogInformation("This feature is only available for a version of HOPEX V5 and above.");
                result = new ErrorMacroResponse(HttpStatusCode.InternalServerError, "This feature is only available for a version of HOPEX V5 and above.");
                return result;
            }
            if (!ContentFileExists())
            {
                _logger.LogInformation("This feature is only available for the static website module. Please install it before and/or make sure that the module is running.");
                result = new ErrorMacroResponse(HttpStatusCode.InternalServerError, "This feature is only available for the static website module. Please install it before and/or make sure that the module is running.");
                return result;
            }

            return null;

        }


        private static void ClearFolder(string folderPath)
        {
            DirectoryInfo directory = new DirectoryInfo(folderPath);
            if (directory.Exists)
            {
                FileInfo[] files = directory.GetFiles();
                //Clear all files and directories inside the wwwroot folder
                foreach (FileInfo file in files)
                {
                    file.Delete();
                }
                DirectoryInfo[] subDirectories = directory.GetDirectories();
                foreach (DirectoryInfo subDirectory in subDirectories)
                {
                    subDirectory.Delete(true);
                }
            }
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        public void CopyWebSiteFilesToModuleFolder(string ModuleFolderPath, string WebSiteFolderPath)
        {
            DirectoryInfo wwwrootTemplate = new DirectoryInfo($"{ModuleFolderPath}\\wwwrootTemplate\\");
            DirectoryInfo wwwroot = new DirectoryInfo($"{ModuleFolderPath}\\wwwroot\\");
            ClearFolder(wwwroot.FullName);
            //copy content of the website(s) and wwwrootTemplate folders
            CopyFilesRecursively(WebSiteFolderPath, wwwroot.FullName);
            CopyFilesRecursively(wwwrootTemplate.FullName, wwwroot.FullName);

        }

    }
}
