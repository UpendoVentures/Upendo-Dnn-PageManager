using DotNetNuke.Entities.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Services.Tokens;
using DotNetNuke.UI.Modules;
using System.Web.UI;

namespace Upendo.Modules.DnnPageManager.Controller
{
    public class BusinessController : ICustomTokenProvider
    {
        private static BusinessController _instance;

        public static BusinessController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BusinessController();
                }
                return _instance;
            }
        }

        public IDictionary<string, IPropertyAccess> GetTokens(Page page, ModuleInstanceContext moduleContext)
        {
            var tokens = new Dictionary<string, IPropertyAccess>();
            tokens["moduleproperties"] = new ModulePropertiesPropertyAccess(moduleContext);
            return tokens;
        }
    }
}
