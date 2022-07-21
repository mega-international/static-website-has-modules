using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Hopex.ApplicationServer.WebServices;
using Mega.Macro.API;

namespace Has.WebMacro
{
    internal class WebsiteService
    {
        public string WebsiteOriginalPath;
        public string FullWebsiteOriginalPath;
        public MegaObject Website;
        private ILogger _logger;
        private string _id; 

        public WebsiteService(MegaRoot root, string websiteId, ILogger logger)
        {
            _id = websiteId;
            _logger = logger;
            Website = root.GetObjectFromId<MegaObject>(websiteId);
            WebsiteOriginalPath = Website.GetPropertyValue("~dAChvzAqqq00[Web Site Path]");
            FullWebsiteOriginalPath = WebsiteOriginalPath.Replace("%ENV%", root.CurrentEnvironment.Path);
        }

        public async Task<ErrorMacroResponse> SingleWebsiteGeneration(MegaRoot root, string languageCode, bool forceContinuOnError, EnvironmentService environmentService, string languagesCode, DirectoryInfo contentModuleVersion, int logMacroId)
        {
            FolderManager folderManager = new FolderManager();
            //Get language from language code
            MegaCollection languageCollection = root.GetSelection($"Select [Language] Where [Language Code] ='{languageCode}'");
            if (languageCollection.Count != 1)
            {
                var languageError = new ErrorMacroResponse(HttpStatusCode.InternalServerError,
                    $"Please check language code : '{languageCode}'");
                _logger.LogInformation($"Please check language code : '{languageCode}'", logMacroId);
                return languageError; ;
            }

            MegaObject language = languageCollection.Item(1);
            root.CurrentEnvironment.NativeObject.SetCurrentLanguage(language.MegaUnnamedField.Substring(0, 13));
            try
            {
                Website.SetPropertyValue("~dAChvzAqqq00[Web Site Path]", $"{FullWebsiteOriginalPath.TrimEnd('\\')}\\{languageCode}");
            }
            catch (Exception e)
            {
                var errorLockResult =
                    new ErrorMacroResponse(HttpStatusCode.InternalServerError, $"Website with id {_id} is locked");
                _logger.LogError(e);
                return errorLockResult; ;
            }

            try
            {
                //Website generation
                _logger.LogInformation($"Start generation of website in '{languageCode}'", logMacroId);
                await GenerateWebsiteAsync();
                _logger.LogInformation("Enf of generation", logMacroId);
            }
            catch
            {
                _logger.LogInformation("Error during the website generation. Check megaerr file for more detail", logMacroId);
                var result = new ErrorMacroResponse(HttpStatusCode.InternalServerError, $"Error during the website's generation");
                if (!forceContinuOnError)
                {
                    return result;
                }
            }
            finally
            {
                environmentService.SetBackToDefaultEnvValues(this, _logger, logMacroId);
                _logger.LogInformation("Initial environment's values have been reset. Transaction has been published",
                    logMacroId);
                if (forceContinuOnError && languageCode.Equals(languagesCode.Split(';').Last()))
                {
                    _logger.LogInformation("Generation errors will not prevent module to package", logMacroId);
                    folderManager.CopyWebSiteFilesToModuleFolder(contentModuleVersion.FullName, FullWebsiteOriginalPath);
                }
            }

            return null;
        }

        private async Task GenerateWebsiteAsync()
        {
            Website.CallMethod("~bPk4JF1Fzu00[GenerateWebSitewithAPI]");
            await Task.CompletedTask;
        }

        public void ClearWebsiteFolder()
        {
            DirectoryInfo directory = new DirectoryInfo(FullWebsiteOriginalPath);
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
    }
}

