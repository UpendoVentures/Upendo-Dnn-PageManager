using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Upendo.Modules.DnnPageManager.Components
{

	public interface ISecurityService
	{
		bool IsPagesAdminUser();
	}

	public class SecurityService : ISecurityService
	{
		public static ISecurityService Instance
		{
			get
			{
				var controller = ComponentFactory.GetComponent<ISecurityService>("SecurityService");
				if (controller == null)
				{
					ComponentFactory.RegisterComponent<ISecurityService, SecurityService>("SecurityService");
				}

				return ComponentFactory.GetComponent<ISecurityService>("SecurityService");
			}
		}

		public virtual bool IsPagesAdminUser()
		{
			var user = UserController.Instance.GetCurrentUserInfo();
			return user.IsSuperUser || user.IsInRole(PortalSettings.Current?.AdministratorRoleName);
		}
	}
}
