using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;

using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Framework;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Personalization;
using static DotNetNuke.Entities.Modules.DesktopModuleInfo;
using System.Text.RegularExpressions;
using DotNetNuke.Common.Utilities;
using Upendo.Modules.DnnPageManager.Model;
using Upendo.Modules.DnnPageManager.Common;
using DotNetNuke.ComponentModel;
using System.Globalization;
using DotNetNuke.Abstractions.Portals;
using Dnn.PersonaBar.Pages.Components;
using Dnn.PersonaBar.Pages.Services.Dto;
using AutoMapper;

namespace Upendo.Modules.DnnPageManager.Components
{
	public interface IPagesControllerImpl
	{
		IEnumerable<Page> GetPagesList(int portalId, out int total, string searchKey, int pageIndex, int pageSize, bool? deleted);
		IEnumerable<Module> GetPageModules(int portalId, int tabId);
		Outcome UpdatePageProperty(int portalId, int tabId, TabFields field, string fieldValue);
		IEnumerable<Url> GetPageUrls(int portalId, int tabId);
		IEnumerable<PermissionInfoBase> GetPagePermissions(int portalId, int tabId);
	}

	public class PagesControllerImpl : IPagesControllerImpl
	{
		private readonly ITabController _tabController;
		private readonly IModuleController _moduleController;
		private readonly IPortalController _portalController;
		private static IMapper Mapper;

		public PagesControllerImpl() : this(TabController.Instance, ModuleController.Instance, PortalController.Instance)
		{
			var config = new MapperConfiguration(cfg => {
				cfg.CreateMap<ModuleInfo, Module>();
				cfg.CreateMap<TabInfo, Page>();
			});
			Mapper = config.CreateMapper();
		}

		public PagesControllerImpl(ITabController tabController, IModuleController moduleController, IPortalController portalController)
		{
			this._tabController = tabController;
			this._moduleController = moduleController;
			this._portalController = portalController;
		}

		public static IPagesControllerImpl Instance
		{
			get
			{
				var controller = ComponentFactory.GetComponent<IPagesControllerImpl>("PagesControllerImpl");
				if (controller == null)
				{
					ComponentFactory.RegisterComponent<IPagesControllerImpl, PagesControllerImpl>("PagesControllerImpl");
				}

				return ComponentFactory.GetComponent<IPagesControllerImpl>("PagesControllerImpl");
			}
		}

		public IEnumerable<Page> GetPagesList(int portalId, out int total, string searchKey = "", int pageIndex = -1, int pageSize = 10, bool? deleted = false)
		{
			try
			{
				pageIndex = ((pageIndex > 0) ? pageIndex : 0);
				pageSize = ((pageSize > 0 && pageSize <= 100) ? pageSize : 10);

				var portalSettings = new PortalSettings(portalId);
				var adminTabId = portalSettings.AdminTabId;

				bool includeHidden = true;
				bool includeDeleted = true;
				bool includeSubpages = true;
				bool? visible = null;

				var tabs = TabController.GetPortalTabs(portalSettings.PortalId, adminTabId, false, includeHidden, includeDeleted, true);
				var pages = from t in tabs
							where (t.ParentId != adminTabId || t.ParentId == Null.NullInteger) &&
									t.IsSystem == false &&
										((string.IsNullOrEmpty(searchKey) && includeSubpages)
											|| (string.IsNullOrEmpty(searchKey) == false &&
													(t.TabName.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) > Null.NullInteger
														|| t.LocalizedTabName.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) > Null.NullInteger)))
							select Mapper.Map<Page>(t);

				pages = includeSubpages ? pages.OrderBy(x => x.ParentId > -1 ? x.ParentId : x.TabID).ThenBy(x => x.TabID) : pages;

				var finalList = new List<Page>();
				if (deleted.HasValue)
				{
					pages = pages.Where(tab => tab.IsDeleted == deleted);
				}

				if (visible.HasValue)
				{
					pages = pages.Where(tab => tab.IsVisible == visible);
				}

				finalList.AddRange(pages);

				total = finalList.Count;
				return pageIndex == -1 || pageSize == -1 ? finalList : finalList.Skip(pageIndex * pageSize).Take(pageSize);
			}
			catch (Exception ex)
			{
                Exceptions.LogException(ex);
                total = 0;
				return new List<Page>();
			}

		}

		public IEnumerable<Module> GetPageModules(int portalId, int tabId)
		{
			try
			{
				var tabModules = this._moduleController.GetTabModules(tabId)
										.Values
										.Where(m => !m.IsDeleted)
										.Select(m => Mapper.Map<Module>(m));

				return tabModules;
			}
			catch (Exception ex)
			{
                Exceptions.LogException(ex);
            }

			return new List<Module>();
		}

		public Outcome UpdatePageProperty(int portalId, int tabId, TabFields field, string fieldValue)
		{
			try
			{
				var tab = TabController.Instance.GetTab(tabId, portalId);
				if (tab == null)
				{
					return new Outcome()
					{
						Success = false,
						ErrorMessage = "Invalid TabId"
					};
				}

				if (field == TabFields.Title)
				{
					tab.Title = fieldValue;
				}

				else if (field == TabFields.Name)
				{
					if (string.IsNullOrEmpty(fieldValue))
					{
						return new Outcome()
						{
							Success = false,
							ErrorMessage = "Page Name is required."
						};
					}
					tab.TabName = fieldValue;
				}

				else if (field == TabFields.Description)
				{
					tab.Description = fieldValue;
				}

				else if (field == TabFields.Keywords)
				{
					tab.KeyWords = fieldValue;
				}

				else if (field == TabFields.Visible)
				{
					tab.IsVisible = bool.Parse(fieldValue);
				}

				else if (field == TabFields.Indexed)
				{
					tab.Indexed = bool.Parse(fieldValue);
				}

				else if (field == TabFields.Priority)
				{
					float priority = float.Parse(fieldValue);
					if (priority < 0 || priority > 1)
					{
						return new Outcome()
						{
							Success = false,
							ErrorMessage = "Priority needs to be between 0 to 1"
						};
					}

					priority = (float)Math.Round((Decimal)priority, 1);

					tab.SiteMapPriority = priority;
				}

				else if (field == TabFields.PrimaryURL)
				{
					var urlUpdateOutcome = UpdateTabUrl(tab, portalId, fieldValue);
					if(urlUpdateOutcome.Success == false)
					{
						return urlUpdateOutcome;
					}
				}

				TabController.Instance.UpdateTab(tab);

				return new Outcome()
				{
					Success = true,
					ErrorMessage = "Successfully updated!"
				};
			}
			catch (Exception ex)
			{
                Exceptions.LogException(ex);
                return new Outcome()
				{
					Success = false,
					ErrorMessage = string.Format("Error updating value for {0}. Error: ", field.ToString(), ex.ToString())
				};
			}
		}

		private Outcome UpdateTabUrl(TabInfo tab, int portalId, string fieldValue)
		{
			var portalSettings = new PortalSettings(portalId);

			var urlPath = fieldValue.ValueOrEmpty().TrimStart('/');

			bool modified;
			// Clean Url
			var options = UrlRewriterUtils.ExtendOptionsForCustomURLs(UrlRewriterUtils.GetOptionsFromSettings(new FriendlyUrlSettings(portalSettings.PortalId)));
			urlPath = FriendlyUrlController.CleanNameForUrl(urlPath, options, out modified);
			if (modified)
			{
				return new Outcome()
				{
					Success = false,
					ErrorMessage = "Page Url value ", //CustomUrlPathCleaned
					Suggestion = "/" + urlPath
				};
			}

			// Validate for uniqueness
			urlPath = FriendlyUrlController.ValidateUrl(urlPath, -1, portalSettings, out modified);
			if (modified)
			{
				return new Outcome()
				{
					Success = false,
					ErrorMessage = "Page Url value is not unique", //UrlPathNotUnique
					Suggestion = "/" + urlPath
				};
			}


			if (tab.TabUrls.Any(u => u.Url.ToLowerInvariant() == urlPath.ValueOrEmpty().ToLowerInvariant()
							 && (u.PortalAliasId == portalSettings.PrimaryAlias.PortalAliasID || u.PortalAliasId == -1)))
			{
				return new Outcome
				{
					Success = false,
					ErrorMessage = "Page Url value is duplicate", //DuplicateUrl
					Suggestion = "/" + urlPath
				};
			}

			var seqNum = (tab.TabUrls.Count > 0) ? tab.TabUrls.Max(t => t.SeqNum) + 1 : 1;

			var tabUrl = new TabUrlInfo
			{
				TabId = tab.TabID,
				SeqNum = seqNum,
				PortalAliasId = portalSettings.PrimaryAlias.PortalAliasID,
				PortalAliasUsage = PortalAliasUsageType.Default,
				QueryString = string.Empty,
				Url = urlPath.ValueOrEmpty(),
				CultureCode = portalSettings.CultureCode,
				HttpStatus = "200",
				IsSystem = false,
			};

			TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
			//tab.Url = fieldValue;
			return new Outcome
			{
				Success = true,
				ErrorMessage = string.Empty,
				Suggestion = string.Empty
			};
		}

		public IEnumerable<Url> GetPageUrls(int portalId, int tabId)
		{
			var tab = TabController.Instance.GetTab(tabId, portalId);
			var pageUrls = PageUrlsController.Instance.GetPageUrls(tab, portalId);

			return pageUrls;
		}

		public IEnumerable<PermissionInfoBase> GetPagePermissions(int portalId, int tabId)
		{
			var tab = TabController.Instance.GetTab(tabId, portalId);
			var pagePermissions = tab.TabPermissions.ToList();

			return pagePermissions;

		}
	}
}
