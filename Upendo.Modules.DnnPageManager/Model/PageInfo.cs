using DotNetNuke.Entities.Tabs;
using System;
using Upendo.Modules.DnnPageManager.Common;

namespace Upendo.Modules.DnnPageManager.Model
{
	public class Page : TabInfo
	{
		public string PublishingStatus
		{
			get
			{
				return GetPublishingStatus();
			}
		}

		public string GetPublishingStatus()
		{

			if (this.HasBeenPublished && this.IsWorkflowCompleted() && 
				this.GetTabLastPublishedOn() <= DateTime.Now && this.GetTabLastPublishedOn() >= DateTime.Now)
			{
				return PublishStatus.Published.ToString();
			}
			else if (this.HasBeenPublished == false && this.IsWorkflowCompleted() == false)
			{
				return PublishStatus.Draft.ToString();
			}
			return PublishStatus.Draft.ToString();
		}

        public bool AllowIndex
        {
            get
            {
                bool allowIndex = default(bool);
                return (!this.TabSettings.ContainsKey("AllowIndex") || !bool.TryParse(this.TabSettings["AllowIndex"].ToString(), out allowIndex)) | allowIndex;
            }
        }
	}
}
