using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Hopex.ApplicationServer.WebServices;
using Mega.Macro.API;

namespace Has.WebMacro
{
    [HopexWebService(WebServiceRoute)]
    public class EntryPoint : HopexWebService<WebServiceArgument>
    {
        // The final path is "<host>/api/generate-package-website"
        private const string WebServiceRoute = "generate-package-website";
        private static int _logMacroId;
        public async override Task<HopexResponse> Execute(WebServiceArgument args)
        {
            _logMacroId = Logger.InitMacroId("WEBSITEGENERATE");
            MegaRoot root = MegaWrapperObject.Cast<MegaRoot>(HopexContext.NativeRoot);
            Logger.LogInformation("GraphQL website generation start");
            ErrorMacroResponse result = new ErrorMacroResponse();

            var location = Assembly.GetExecutingAssembly().Location;
            FolderManager folderManager = new FolderManager(location, Logger);

            if (!folderManager.ShadowFileExists())
            {
                Logger.LogInformation("This feature is only available for a version of HOPEX V5 and above.");
                result = new ErrorMacroResponse(HttpStatusCode.InternalServerError, "This feature is only available for a version of HOPEX V5 and above.");
                return HopexResponse.Json(JsonSerializer.Serialize(result));
            }
            if (!folderManager.ContentFileExists())
            {
                Logger.LogInformation("This feature is only available for the static website module. Please install it before and/or make sure that the module is running.");
                result = new ErrorMacroResponse(HttpStatusCode.InternalServerError, "This feature is only available for the static website module. Please install it before and/or make sure that the module is running.");
                return HopexResponse.Json(JsonSerializer.Serialize(result));
            }
            DirectoryInfo contentModuleVersion = folderManager._contentModuleVersionDirectoryInfo;

            string webSiteId = args.WebSiteId;
            string languagesCode = args.LanguagesCode;
            bool forceContinuOnError = args.ForceContinuOnError;

            MegaObject website = root.GetObjectFromId<MegaObject>(webSiteId);
            if (website.Id == null)
            {
                var errorIdResult = new ErrorMacroResponse(HttpStatusCode.InternalServerError, $"Website with id {webSiteId} doesn't exist. Please check the id of your website object");
                Logger.LogInformation($"Website with id {webSiteId} doesn't exist. Please check the id of your website object", _logMacroId);
                return HopexResponse.Json(JsonSerializer.Serialize(errorIdResult));
            }

            //Original properties of the environment and website
            MegaLanguage originalEnvironmentMegaLanguage = root.CurrentEnvironment.CurrentLanguage;
            MegaObject originalEnvironmentLanguage = root.GetObjectFromId<MegaObject>(originalEnvironmentMegaLanguage.Id);
            string websiteOriginalPath = website.GetPropertyValue("~dAChvzAqqq00[Web Site Path]");
            string fullWebsiteOriginalPath = websiteOriginalPath.Replace("%ENV%", root.CurrentEnvironment.Path);

            ClearFolder(fullWebsiteOriginalPath);

            foreach (string languageCode in languagesCode.Split(';'))
            {
                await SingleWebsiteGeneration(root, languageCode, website, fullWebsiteOriginalPath, webSiteId, forceContinuOnError, originalEnvironmentLanguage, websiteOriginalPath, languagesCode, contentModuleVersion);
            }
            if (!forceContinuOnError)
            {
                CopyAndPackageWebSiteModule(contentModuleVersion, fullWebsiteOriginalPath);
                string newModuleVersion = $"15.{DateTime.Now.Year}.{DateTime.Now.Month}+{DateTime.Now.Day}{DateTime.Now.Hour}{DateTime.Now.Minute}";
                Logger.LogInformation($"Start packaging the module version {newModuleVersion}", _logMacroId);
                await PackageModuleAsync(contentModuleVersion.FullName, folderManager._hotInstallDirectoryInfo, newModuleVersion, Logger);
                Logger.LogInformation("End packaging the module", _logMacroId);

                result = new ErrorMacroResponse(HttpStatusCode.OK, $"Website was successfully generated and packaged in '{languagesCode}'.");
            }
            Logger.LogInformation("GraphQL website generation end");
            return HopexResponse.Json(JsonSerializer.Serialize(result));
        }

        private async Task<HopexResponse> SingleWebsiteGeneration(MegaRoot root, string languageCode, MegaObject website,
            string fullWebsiteOriginalPath, string webSiteId, bool forceContinuOnError, MegaObject originalEnvironmentLanguage,
            string websiteOriginalPath, string languagesCode, DirectoryInfo contentModuleVersion)
        {
            //Get language from language code
            MegaCollection languageCollection = root.GetSelection($"Select [Language] Where [Language Code] ='{languageCode}'");
            if (languageCollection.Count != 1)
            {
                var languageError = new ErrorMacroResponse(HttpStatusCode.InternalServerError,
                    $"Please check language code : '{languageCode}'");
                Logger.LogInformation($"Please check language code : '{languageCode}'", _logMacroId);
                return HopexResponse.Json(JsonSerializer.Serialize(languageError)); ;
            }

            MegaObject language = languageCollection.Item(1);
            root.CurrentEnvironment.NativeObject.SetCurrentLanguage(language.MegaUnnamedField.Substring(0, 13));
            try
            {
                website.SetPropertyValue("~dAChvzAqqq00[Web Site Path]", $"{fullWebsiteOriginalPath}\\{languageCode}");
            }
            catch (Exception e)
            {
                var errorLockResult =
                    new ErrorMacroResponse(HttpStatusCode.InternalServerError, $"Website with id {webSiteId} is locked");
                Logger.LogError(e);
                return HopexResponse.Json(JsonSerializer.Serialize(errorLockResult)); ;
            }

            try
            {
                //Website generation
                Logger.LogInformation($"Start generation of website in '{languageCode}'", _logMacroId);
                await GenerateWebsiteAsync(website);
                Logger.LogInformation("Enf of generation", _logMacroId);
            }
            catch
            {
                Logger.LogInformation("Error during the website generation. Check megaerr file for more detail", _logMacroId);
                var result = new ErrorMacroResponse(HttpStatusCode.InternalServerError, $"Error during the website's generation");
                if (!forceContinuOnError)
                {
                    return HopexResponse.Json(JsonSerializer.Serialize(result));
                }
            }
            finally
            {
                SetBackToDefaultEnvValues(root, website, originalEnvironmentLanguage, websiteOriginalPath);
                Logger.LogInformation("Initial environment's values have been reset. Transaction has been published",
                    _logMacroId);
                if (forceContinuOnError && languageCode.Equals(languagesCode.Split(';').Last()))
                {
                    Logger.LogInformation("Generation errors will not prevent module to package", _logMacroId);
                    CopyAndPackageWebSiteModule(contentModuleVersion, fullWebsiteOriginalPath);
                }
            }

            return null;
        }

        private void CopyAndPackageWebSiteModule(DirectoryInfo contentModuleVersion, string fullWebsiteOriginalPath)
        {
            Logger.LogInformation("Start copying website(s) files to the module's folder", _logMacroId);
            CopyWebSiteFilesToModule(contentModuleVersion.FullName, fullWebsiteOriginalPath);
            Logger.LogInformation("End copying website(s) files to the module's folder", _logMacroId);
        }

        //Reassign original environment and website properies
        private static void SetBackToDefaultEnvValues(MegaRoot root, MegaObject website, MegaObject originalEnvironmentLanguage, string websiteOriginalPath)
        {
            root.CurrentEnvironment.NativeObject.SetCurrentLanguage(originalEnvironmentLanguage.MegaUnnamedField.Substring(0, 13));
            website.SetPropertyValue("~dAChvzAqqq00[Web Site Path]", websiteOriginalPath);
            root.CallFunction("~lcE6jbH9G5cK", "{\"instruction\":\"POSTPUBLISHINSESSION\"}");
        }

        private async static Task PackageModuleAsync(string ModuleFolderPath, string DestinationFolderPath, string ModuleVersion, ILogger logger)
        {
            //TODO
            //For .Net 6.0 use the ModulePackger instead of dotnet-has

            /*var packager = new ModulePackager(ModuleFolderPath, "debug");
            var packageResult = await packager.CreatePackageAsync("16.0.0", false, false, DestinationFolderPath);*/


           
            var info = new ProcessStartInfo

            {
                UseShellExecute = false,
                FileName = @"dotnet-has.exe",
                Arguments = $"module create -p \"{ModuleFolderPath}\" --copy-to \"{DestinationFolderPath}\" --version \"{ModuleVersion}\"",
                ErrorDialog = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            var sb = new StringBuilder();
            var process = Process.Start(info);
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.OutputDataReceived += (p, e) =>
            {
                sb.AppendLine(e.Data);
            };
            process.ErrorDataReceived += (p, e) =>
            {
                sb.AppendLine(e.Data);
            };
            process.WaitForExit();
            var output = sb.ToString();
           
        }
        private static void CopyWebSiteFilesToModule(string ModuleFolderPath, string WebSiteFolderPath)
        {
            DirectoryInfo wwwrootTemplate = new DirectoryInfo($"{ModuleFolderPath}\\wwwrootTemplate\\");
            DirectoryInfo wwwroot = new DirectoryInfo($"{ModuleFolderPath}\\wwwroot\\");
            ClearFolder(wwwroot.FullName);
            //copy content of the website(s) and wwwrootTemplate folders
            CopyFilesRecursively(WebSiteFolderPath, wwwroot.FullName);
            CopyFilesRecursively(wwwrootTemplate.FullName, wwwroot.FullName);

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
        private static void ClearFolder(string FolderPath)
        {
            DirectoryInfo directory = new DirectoryInfo(FolderPath);
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

        private async Task GenerateWebsiteAsync(MegaItem website)
        {
            website.CallMethod("~bPk4JF1Fzu00[GenerateWebSitewithAPI]");
            await Task.CompletedTask;
        }
    }
}