using Hopex.ApplicationServer.WebServices;
using Mega.Macro.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace Has.WebMacro
{
    internal class EnvironmentService
    {
        public MegaObject OriginalLanguage;
        private readonly MegaRoot _root;

        public EnvironmentService(MegaRoot root)
        {
            _root = root;
            var originalMegaLanguage = root.CurrentEnvironment.CurrentLanguage;
            OriginalLanguage = root.GetObjectFromId<MegaObject>(originalMegaLanguage.Id);
        }

        public void SetBackToDefaultEnvValues(WebsiteService websiteService, ILogger logger, int logMacroId)
        {
            try
            {
                _root.CurrentEnvironment.NativeObject.SetCurrentLanguage(OriginalLanguage.MegaUnnamedField.Substring(0, 13));
                websiteService.Website.SetPropertyValue("~dAChvzAqqq00[Web Site Path]", websiteService.WebsiteOriginalPath);
                _root.CallFunction("~lcE6jbH9G5cK", "{\"instruction\":\"POSTPUBLISHINSESSION\"}");
            }
            catch(Exception ex)
            {
                logger.LogError(ex, logMacroId, ex.Message);
            }
        }
    }
}
