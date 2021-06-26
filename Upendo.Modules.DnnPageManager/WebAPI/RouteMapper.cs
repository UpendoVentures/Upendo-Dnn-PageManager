using Upendo.Modules.DnnPageManager.Common;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Upendo.Modules.DnnPageManager.WebAPI
{
    public class RouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            //~/desktopmodules/Upendo.Modules.DnnPageManager.WebAPI/api/{controller}/{action}
            //https://www.dnnsoftware.com/community-blog/cid/142400/getting-started-with-services-framework-webapi-edition 

            mapRouteManager
                    .MapHttpRoute(
                        moduleFolderName: Constants.ModuleFolderName,
                        routeName: "default",
                        url: "{controller}/{action}",
                        namespaces: new[] { "Upendo.Modules.DnnPageManager.Controller" }
                    );
        }
    }
}
