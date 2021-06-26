using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Upendo.Modules.DnnPageManager.Common
{
    public static class Constants
    {
        public static string ModuleName = "Upendo.Modules.DnnPageManager";
        public static string ModuleFolderName = "Upendo.Modules.DnnPageManager";

        public static string DesktopModules = "DesktopModules";
        public static string Resources = "App_LocalResources";

        public static string APIPath = string.Format("/{0}/{1}/API/", DesktopModules, ModuleName);

    }

	public enum PublishStatus
	{
		All,
		Published,
		Draft
	}


	public enum TabFields
	{
		Name,
		Title,
		Description,
		Keywords,
		Priority,
		PrimaryURL,
		Visible,
		Indexed,
		Publishing
	}

}
