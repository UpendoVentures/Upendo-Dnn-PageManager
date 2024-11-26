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
using System;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Common.Utilities;
using Upendo.Modules.DnnPageManager.Model;
using Upendo.Modules.DnnPageManager.Common;
using Dnn.PersonaBar.Pages.Components;
using Dnn.PersonaBar.Pages.Services.Dto;
using AutoMapper;
using DotNetNuke.Instrumentation;
using Constants = Upendo.Modules.DnnPageManager.Common.Constants;

namespace Upendo.Modules.DnnPageManager.Components
{
    public class PagesControllerImpl 
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PagesControllerImpl));

        private static IMapper Mapper;

        public PagesControllerImpl() 
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModuleInfo, Module>();
                cfg.CreateMap<TabInfo, Page>();
            });
            Mapper = config.CreateMapper();
        }

        public IEnumerable<Page> GetPagesList(int portalId, out int total, string searchKey = "", int pageIndex = -1, int pageSize = 10,
                                            string sortBy = "", string sortType = "", bool? deleted = false,bool? getAllPages=false)
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
                                                        || t.LocalizedTabName.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) > Null.NullInteger
                                                        || t.Title.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) > Null.NullInteger
                                                        || t.Description.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) > Null.NullInteger
                                                        || t.KeyWords.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) > Null.NullInteger
                                                        || t.Url.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) > Null.NullInteger
                                                        )))
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

                string sortOn = sortBy.ToLowerInvariant();
                string sortOrder = sortType.ToLowerInvariant();

                if (String.IsNullOrEmpty(sortBy) == false)
                {
                    switch (sortBy.ToLowerInvariant())
                    {
                        case Constants.NAME:
                            pages = sortOrder == Constants.ASC ? pages.OrderBy(x => x.LocalizedTabName) : pages.OrderByDescending(x => x.LocalizedTabName);
                            break;
                        case Constants.TITLE:
                            pages = sortOrder == Constants.ASC ? pages.OrderBy(x => x.Title) : pages.OrderByDescending(x => x.Title);
                            break;
                        default:
                            break;
                    }
                }

                finalList.AddRange(pages);

                total = finalList.Count;
               
                return pageIndex == -1 || pageSize == -1 ? finalList : getAllPages==true? finalList : finalList.Skip(pageIndex * pageSize).Take(pageSize);
            }
            catch (Exception ex)
            {
                LogError(ex);
                Exceptions.LogException(ex);
                total = 0;
                return new List<Page>();
            }

        }

      public IEnumerable<Module> GetPageModules(int portalId, int tabId)
        {
            try
            {
                var tabModules = ModuleController.Instance.GetTabModules(tabId)
                                        .Values
                                        .Where(m => !m.IsDeleted)
                                        .Select(m => Mapper.Map<Module>(m));

                return tabModules;
            }
            catch (Exception ex)
            {
                LogError(ex);
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
                        ErrorMessage = Constants.ERROR_PAGE_TABID_INVALID
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
                            ErrorMessage = Constants.ERROR_PAGE_NAME_REQUIRED
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
                            ErrorMessage = Constants.ERROR_PAGE_PRIORITY_RANGE_INVALID
                        };
                    }

                    priority = (float)Math.Round((Decimal)priority, 1);

                    tab.SiteMapPriority = priority;
                }

                else if (field == TabFields.PrimaryURL)
                {
                    var urlUpdateOutcome = UpdateTabUrl(tab, portalId, fieldValue);
                    if (urlUpdateOutcome.Success == false)
                    {
                        return urlUpdateOutcome;
                    }
                }

                TabController.Instance.UpdateTab(tab);

                return new Outcome()
                {
                    Success = true,
                    ErrorMessage = Constants.SUCCESS_UPDATED
                };
            }
            catch (Exception ex)
            {
                LogError(ex);
                Exceptions.LogException(ex);
                return new Outcome()
                {
                    Success = false,
                    ErrorMessage = string.Format(Constants.ERROR_FORMAT_UPDATE_VALUE, field.ToString(), ex.Message)
                };
            }
        }
 public Outcome UpdatePageAllowIndex(int portalId, int tabId, bool fieldValue)
        {
            try
            {
                var tab = TabController.Instance.GetTab(tabId, portalId);
                if (tab == null)
                {
                    return new Outcome()
                    {
                        Success = false,
                        ErrorMessage = Constants.ERROR_PAGE_TABID_INVALID
                    };
                }
               
                if (fieldValue == true)
                {
                    tab.TabSettings["AllowIndex"] = "true";
                }

                if (fieldValue == false)
                {
                    tab.TabSettings["AllowIndex"] = "false";
                }
                                
                TabController.Instance.UpdateTab(tab);

                return new Outcome()
                {
                    Success = true,
                    ErrorMessage = Constants.SUCCESS_UPDATED
                };
            }
            catch (Exception ex)
            {
                LogError(ex);
                Exceptions.LogException(ex);
                return new Outcome()
                {
                    Success = false,
                    ErrorMessage = string.Format(Constants.ERROR_FORMAT_UPDATE_VALUE, "Allow Index", ex.Message)
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
                    ErrorMessage = Constants.ERROR_PAGE_URL_VALUE, //CustomUrlPathCleaned
                    Suggestion = string.Concat(Constants.SLASH, urlPath)
                };
            }

            // Validate for uniqueness
            urlPath = FriendlyUrlController.ValidateUrl(urlPath, -1, portalSettings, out modified);
            if (modified)
            {
                return new Outcome()
                {
                    Success = false,
                    ErrorMessage = Constants.ERROR_PAGE_URL_NOT_UNIQUE, //UrlPathNotUnique
                    Suggestion = string.Concat(Constants.SLASH, urlPath)
                };
            }


            if (tab.TabUrls.Any(u => u.Url.ToLowerInvariant() == urlPath.ValueOrEmpty().ToLowerInvariant()
                             && (u.PortalAliasId == portalSettings.PrimaryAlias.PortalAliasID || u.PortalAliasId == -1)))
            {
                return new Outcome
                {
                    Success = false,
                    ErrorMessage = Constants.ERROR_PAGE_URL_DUPLICATE, //DuplicateUrl
                    Suggestion = string.Concat(Constants.SLASH, urlPath)
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
                HttpStatus = Constants.RESPONSE_STATUS_200,
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
            try
            {
                var tab = TabController.Instance.GetTab(tabId, portalId);
                var pageUrls = PageUrlsController.Instance.GetPageUrls(tab, portalId);

                return pageUrls;
            }
            catch (Exception e)
            {
                LogError(e);
                throw;
            }
        }

        public IEnumerable<PermissionInfoBase> GetPagePermissions(int portalId, int tabId)
        {
            try
            {
                var tab = TabController.Instance.GetTab(tabId, portalId);
                var pagePermissions = tab.TabPermissions.ToList();

                return pagePermissions;
            }
            catch (Exception e)
            {
                LogError(e);
                throw;
            }
        }
        public IEnumerable<Page> GetPagesMissingMetadata(IEnumerable<Model.Page> pages, ModuleInfo ActiveModule, bool? filterMetadata = false)
        {
          var pagesMissingMetadata = new List<Model.Page>();
            if (filterMetadata.HasValue && ActiveModule.ModuleSettings.Count > 1)
            {
                if (filterMetadata.Value)
                {
                    if (ActiveModule.ModuleSettings[Constants.QuickSettings.MODSETTING_Title].ToString().Equals(Constants.QuickSettings.MODSETTING_DefaultTrue))
                    {
                        var filterTitle = pages.Where(tab => string.IsNullOrEmpty(tab.Title));
                        foreach (var item in filterTitle)
                        {
                            if (!pagesMissingMetadata.Any(s => s.KeyID == item.KeyID))
                            {
                                pagesMissingMetadata.Add(item);
                            }
                        }
                    }
                    if (ActiveModule.ModuleSettings[Constants.QuickSettings.MODSETTING_Description].ToString().Equals(Constants.QuickSettings.MODSETTING_DefaultTrue))
                    {
                        var filterDescription = pages.Where(tab => string.IsNullOrEmpty(tab.Description));
                        foreach (var item in filterDescription)
                        {
                            if (!pagesMissingMetadata.Any(s => s.KeyID == item.KeyID))
                            {
                                pagesMissingMetadata.Add(item);
                            }
                        }

                    }
                    if (ActiveModule.ModuleSettings[Constants.QuickSettings.MODSETTING_Keywords].ToString().Equals(Constants.QuickSettings.MODSETTING_DefaultTrue))
                    {
                        var filterKeyWords = pages.Where(tab => string.IsNullOrEmpty(tab.KeyWords));
                        foreach (var item in filterKeyWords)
                        {
                            if (!pagesMissingMetadata.Any(s => s.KeyID == item.KeyID))
                            {
                                pagesMissingMetadata.Add(item);
                            }
                        }
                    }
                }
            }
            return pagesMissingMetadata.ToList();
        }

        private static void LogError(Exception ex)
        {
            if (ex != null)
            {
                Logger.Error(ex.Message, ex);
                if (ex.InnerException != null)
                {
                    Logger.Error(ex.InnerException.Message, ex.InnerException);
                }
            }
        }
    }
}