using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Workflow;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Tabs.TabVersions;
using Dnn.PersonaBar.Pages.Components;
using Dnn.PersonaBar.Pages.Services.Dto;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Upendo.Modules.DnnPageManager.Model;

namespace Upendo.Modules.DnnPageManager.Common
{
	public static class Extensions
	{
		public static bool IsWorkflowCompleted(this TabInfo tab)
		{
			if (tab.ContentItemId == Null.NullInteger && tab.TabID != Null.NullInteger)
			{
				TabController.Instance.CreateContentItem(tab);
				TabController.Instance.UpdateTab(tab);
			}
			return WorkflowEngine.Instance.IsWorkflowCompleted(tab);
		}


		public static DateTime GetTabLastPublishedOn(this TabInfo tab)
		{
			if (tab.HasBeenPublished)
			{
				IEnumerable<TabVersion> tabVersions = TabVersionController.Instance.GetTabVersions(tab.TabID, false);
				if (tabVersions != null)
				{
					return (from v in tabVersions
							where v.IsPublished
							orderby v.Version descending
							select v).FirstOrDefault()?.LastModifiedOnDate ?? tab.LastModifiedOnDate;
				}
				return tab.LastModifiedOnDate;
			}
			return DateTime.MinValue;
		}

		public static IEnumerable<Url> PageUrls(this Page tabInfo)
		{
			return PageUrlsController.Instance.GetPageUrls((TabInfo)tabInfo, tabInfo.PortalID);
		}
	}
}
