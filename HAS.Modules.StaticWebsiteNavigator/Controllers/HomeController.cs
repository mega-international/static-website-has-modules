using System.Threading.Tasks;
using Has.Modules.WebSite.ContentFileProvider;
using Has.Modules.WebSite.ViewModels;
using Mega.Has.Commons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Has.Modules.WebSite.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IStaticWebContentModuleInfo staticWebContentModuleInfo;
        private readonly IModuleConfiguration moduleConfiguration;

        public HomeController(IStaticWebContentModuleInfo  staticWebContentModuleInfo, IModuleConfiguration moduleConfiguration) 
        {
            this.staticWebContentModuleInfo = staticWebContentModuleInfo;
            this.moduleConfiguration = moduleConfiguration;
        }

        public IActionResult Index()
        {
            // From here & User is authenticated 
            if(staticWebContentModuleInfo.PhysicalStaticContentFolder == null)
            {
                return View();
            }

            // Module content structure should be:
            //  wwwroot
            //  For multi languages
            //    - [lang1]
            //      - index.htm[l]
            //      - assets for lang1...
            //    - [lang2]
            //      - index.htm[l]
            //      - assets for lang2...
            //
            //  or for default language
            //   - index.htm[l]
            //     - assets for default language...
            return View(new HomeViewModel());
        }


        [HttpGet]
        public async Task<IActionResult> Logout( [FromQuery]string returnUrl)
        {
            var tokenFeature = HttpContext.Features.Get<IHopexSessionFeature>();
            await tokenFeature.SignoutAsync();
            return Redirect(returnUrl ?? "/portal");
        }        
    }
}