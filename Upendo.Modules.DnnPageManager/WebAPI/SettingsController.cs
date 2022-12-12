using System.Net.Http;
using System.Net;
using System.Web.Http;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Web.Api;
using DotNetNuke.Security;

namespace Upendo.Modules.DnnPageManager.Controller
{
    
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
    public class SettingsController : DnnApiController
    {
        public const string MODSETTING_Title = "Title";
        public const string MODSETTING_Description = "Description";
        public const string MODSETTING_Keywords = "Keywords";
        public SettingsController() { }

        [HttpGet]  //[baseURL]/settings/load
        
        public HttpResponseMessage LoadSettings()
        {
            var settings = new SettingsViewModel();
           
            if (ActiveModule.ModuleSettings.ContainsKey(MODSETTING_Title))
            {
                settings.Title = ActiveModule.ModuleSettings[MODSETTING_Title].ToString();
            }
            if (ActiveModule.ModuleSettings.ContainsKey(MODSETTING_Description))
            {
                settings.Description = ActiveModule.ModuleSettings[MODSETTING_Description].ToString();
            }
            if (ActiveModule.ModuleSettings.ContainsKey(MODSETTING_Keywords))
            {
                settings.Keywords = ActiveModule.ModuleSettings[MODSETTING_Keywords].ToString();
            }

            return Request.CreateResponse(HttpStatusCode.OK, settings);
        }

        [HttpPost]  //[baseURL]/settings/save
        [ActionName("save")]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveSettings(SettingsViewModel settings)
        {
            ModuleController.Instance.UpdateModuleSetting(ActiveModule.ModuleID, MODSETTING_Title, settings.Title);
            ModuleController.Instance.UpdateModuleSetting(ActiveModule.ModuleID, MODSETTING_Description, settings.Description);
            ModuleController.Instance.UpdateModuleSetting(ActiveModule.ModuleID, MODSETTING_Keywords, settings.Keywords);

            return Request.CreateResponse(HttpStatusCode.OK, "success");
        }
    }
    
}
