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
        // The final path is "<host>/api/website/static/generate"
        private const string WebServiceRoute = "website/static/generate";
        private static int _logMacroId;

        public async override Task<HopexResponse> Execute(WebServiceArgument args)
        {
            Logger.LogInformation("Start of macro website generation and packaging");

            var root = MegaWrapperObject.Cast<MegaRoot>(HopexContext.NativeRoot);
            var result = new ErrorMacroResponse();

            _logMacroId = Logger.InitMacroId("WEBSITEGENERATE");

            //Folder/files elements
            var location = Assembly.GetExecutingAssembly().Location;
            var folderManager = new FolderManager(location, Logger);
            result = folderManager.VerifyCorrectFilesAvailable();
            var contentModuleVersionDirectoryInfo = folderManager.contentModuleVersionDirectoryInfo;

            //Fetch json body arguments
            var webSiteId = args.WebSiteId;
            var languagesCode = args.LanguagesCode;
            var forceContinuOnError = args.ForceContinuOnError;

            var websiteService = new WebsiteService(root, webSiteId, Logger);
            if (websiteService.Website.Id == null)
            {
                var errorIdResult = new ErrorMacroResponse(HttpStatusCode.InternalServerError, $"Website with id {webSiteId} doesn't exist. Please check the id of your website object");
                Logger.LogInformation($"Website with id {webSiteId} doesn't exist. Please check the id of your website object", _logMacroId);
                return HopexResponse.Json(JsonSerializer.Serialize(errorIdResult));
            }

            websiteService.ClearWebsiteFolder();

            var environmentService = new EnvironmentService(root);

            foreach (var languageCode in languagesCode.Split(';'))
            {
                result = await websiteService.SingleWebsiteGeneration(root, languageCode, forceContinuOnError,  environmentService, languagesCode, contentModuleVersionDirectoryInfo, _logMacroId);
                if(result != null) return HopexResponse.Json(JsonSerializer.Serialize(result));
            }

          
            folderManager.CopyWebSiteFilesToModuleFolder(contentModuleVersionDirectoryInfo.FullName, websiteService.FullWebsiteOriginalPath);
            var newModuleVersion = $"15.{DateTime.Now.Year}.{DateTime.Now.Month}+{DateTime.Now.Day}{DateTime.Now.Hour}{DateTime.Now.Minute}";
            Logger.LogInformation($"Start packaging the module version {newModuleVersion}", _logMacroId);
            var websitePackager = new WebsitePackager();
            await websitePackager.PackageModuleAsync(contentModuleVersionDirectoryInfo.FullName,
            folderManager.hotInstallDirectoryInfo, newModuleVersion, Logger);
            Logger.LogInformation("End packaging the module", _logMacroId);
            result = new ErrorMacroResponse(HttpStatusCode.OK, $"Website was successfully generated and packaged in '{languagesCode}'.");

            Logger.LogInformation("End of macro website generation and packaging");
            return HopexResponse.Json(JsonSerializer.Serialize(result));
        }
    }
}