/*
Copyright Upendo Ventures, LLC 

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
DEALINGS IN THE SOFTWARE.
*/

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Workflow;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Tabs.TabVersions;
using Dnn.PersonaBar.Pages.Components;
using Dnn.PersonaBar.Pages.Services.Dto;

using System;
using System.Collections.Generic;
using System.Linq;
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