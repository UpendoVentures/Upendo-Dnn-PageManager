﻿/*
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

using DotNetNuke.Web.Api;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Instrumentation;
using Upendo.Modules.DnnPageManager.Common;
using Upendo.Modules.DnnPageManager.Components;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using System.Collections.Generic;

namespace Upendo.Modules.DnnPageManager.Controller
{
    public class PagesController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PagesController));

        public PagesController()
        {
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        [DnnAuthorize]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage GetPagesList(int portalId, string searchKey = "", int pageIndex = -1, int pageSize = 10, string sortBy = "",
                                                string sortType = "", bool? deleted = false, bool? filterMetadata = false, bool? filterUnpublished = false)
        {
            //Method to generate the Settings_Names on the first load
            if (ActiveModule.ModuleSettings.Count <= 1)
            {
                var settings = new SettingsViewModel();
                ModuleController.Instance.UpdateModuleSetting(ActiveModule.ModuleID, Constants.QuickSettings.MODSETTING_Title, Constants.QuickSettings.MODSETTING_DefaultTrue);
                ModuleController.Instance.UpdateModuleSetting(ActiveModule.ModuleID, Constants.QuickSettings.MODSETTING_Description, Constants.QuickSettings.MODSETTING_DefaultTrue);
                ModuleController.Instance.UpdateModuleSetting(ActiveModule.ModuleID, Constants.QuickSettings.MODSETTING_Keywords, Constants.QuickSettings.MODSETTING_DefaultFalse);

                settings.Title = Constants.QuickSettings.MODSETTING_DefaultTrue;
                settings.Description = Constants.QuickSettings.MODSETTING_DefaultTrue;
                settings.Keywords = Constants.QuickSettings.MODSETTING_DefaultFalse;
            }
            int total = 0;
            try
            {
                if (SecurityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }

                PagesControllerImpl pageController = new PagesControllerImpl();
                var pages = pageController.GetPagesList(portalId: portalId,
                                                                         total: out total,
                                                                         searchKey: searchKey,
                                                                         pageIndex: pageIndex,
                                                                         pageSize: pageSize,
                                                                         sortBy: sortBy,
                                                                         sortType: sortType,
                                                                         deleted: deleted
                                                                         );
                var allPages = pageController.GetPagesList(portalId: portalId, total: out total, searchKey: searchKey, pageIndex: pageIndex, pageSize: pageSize,
                                                            sortBy: sortBy, sortType: sortType, deleted: deleted, getAllPages: true);
                object result;
                if (filterMetadata.HasValue && filterMetadata.Value)
                {
                    var pagesMissingMetadata = pageController.GetPagesMissingMetadata(pages, ActiveModule, filterMetadata).ToList();

                    result = pagesMissingMetadata.OrderBy(s => s.LocalizedTabName).Select(p => new
                    {
                        Id = p.TabID,
                        Name = p.LocalizedTabName,
                        Title = p.Title,
                        Description = p.Description,
                        Keywords = p.KeyWords,
                        Priority = p.SiteMapPriority,
                        PrimaryUrl = pageController.GetPageUrls(p.PortalID, p.TabID).FirstOrDefault().Path,
                        LastUpdated = string.Format(Constants.FORMAT_LASTUPDATED, (p.LastModifiedByUserID == Null.NullInteger ? Constants.SYSTEM : p.LastModifiedByUser(portalId).FirstName), (p.LastModifiedByUserID == Null.NullInteger ? Constants.ACCOUNT : p.LastModifiedByUser(portalId).LastName), p.LastModifiedOnDate.ToString(Constants.FORMAT_DATE)),
                        IsVisible = p.IsVisible,
                        IsAllowedSearch = p.AllowIndex,
                        IsDisabled = p.DisableLink,
                        HasBeenPublished = p.HasBeenPublished
                    });

                    pagesMissingMetadata = pageController.GetPagesMissingMetadata(allPages, ActiveModule, filterMetadata).ToList();

                    return Request.CreateResponse<dynamic>(HttpStatusCode.OK, new { Total = filterMetadata.Value ? pagesMissingMetadata.Count() : allPages.Count(), result });
                }
                if (filterUnpublished.HasValue && filterUnpublished.Value)
                {
                    var pagesUnpublished = pageController.GetPagesUnpublished(pages, ActiveModule, filterUnpublished).ToList();

                    result = pagesUnpublished.OrderBy(s => s.LocalizedTabName).Select(p => new
                    {
                        Id = p.TabID,
                        Name = p.LocalizedTabName,
                        Title = p.Title,
                        Description = p.Description,
                        Keywords = p.KeyWords,
                        Priority = p.SiteMapPriority,
                        PrimaryUrl = pageController.GetPageUrls(p.PortalID, p.TabID).FirstOrDefault().Path,
                        LastUpdated = string.Format(Constants.FORMAT_LASTUPDATED, (p.LastModifiedByUserID == Null.NullInteger ? Constants.SYSTEM : p.LastModifiedByUser(portalId).FirstName), (p.LastModifiedByUserID == Null.NullInteger ? Constants.ACCOUNT : p.LastModifiedByUser(portalId).LastName), p.LastModifiedOnDate.ToString(Constants.FORMAT_DATE)),
                        IsVisible = p.IsVisible,
                        IsAllowedSearch = p.AllowIndex,
                        IsDisabled = p.DisableLink,
                        HasBeenPublished = p.HasBeenPublished
                    });

                    pagesUnpublished = pageController.GetPagesUnpublished(allPages, ActiveModule, filterUnpublished).ToList();

                    return Request.CreateResponse<dynamic>(HttpStatusCode.OK, new { Total = filterUnpublished.Value ? pagesUnpublished.Count() : allPages.Count(), result });
                }

                result = pages.OrderBy(s => s.LocalizedTabName).Select(p => new
                {
                    Id = p.TabID,
                    Name = p.LocalizedTabName,
                    Title = p.Title,
                    Description = p.Description,
                    Keywords = p.KeyWords,
                    Priority = p.SiteMapPriority,
                    PrimaryUrl = pageController.GetPageUrls(p.PortalID, p.TabID).FirstOrDefault().Path,
                    LastUpdated = string.Format(Constants.FORMAT_LASTUPDATED, (p.LastModifiedByUserID == Null.NullInteger ? Constants.SYSTEM : p.LastModifiedByUser(portalId).FirstName), (p.LastModifiedByUserID == Null.NullInteger ? Constants.ACCOUNT : p.LastModifiedByUser(portalId).LastName), p.LastModifiedOnDate.ToString(Constants.FORMAT_DATE)),
                    IsVisible = p.IsVisible,
                    IsAllowedSearch = p.AllowIndex,
                    IsDisabled = p.DisableLink,
                    HasBeenPublished = p.HasBeenPublished
                }).ToList();

                return Request.CreateResponse<dynamic>(HttpStatusCode.OK, new { Total = allPages.Count(), result });

            }

            catch (Exception ex)
            {
                LogError(ex);
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { Constants.RESPONSE_SUCCESS, false } });
            }

        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        [DnnAuthorize]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage GetPageModules(int portalId, int tabId)
        {
            try
            {
                if (SecurityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }

                PagesControllerImpl pageController = new PagesControllerImpl();

                var pageModules = pageController.GetPageModules(portalId, tabId);
                var result = pageModules.Select(m => new
                {
                    Title = m.ModuleTitle,
                    ModuleType = m.DesktopModule.FriendlyName
                });

                return Request.CreateResponse<dynamic>(HttpStatusCode.OK, result); ;
            }
            catch (Exception ex)
            {
                LogError(ex);
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { Constants.RESPONSE_SUCCESS, false } });
            }
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        [DnnAuthorize]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage GetPageUrls(int portalId, int tabId)
        {
            try
            {
                if (SecurityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }
                PagesControllerImpl pageController = new PagesControllerImpl();

                var pageUrls = pageController.GetPageUrls(portalId, tabId);

                var result = pageUrls.Select(m => new
                {
                    Url = m.Path,
                    UrlType = m.StatusCode.Value,
                    GeneratedBy = m.IsSystem ? Constants.AUTOMATIC : m.UserName
                });

                return Request.CreateResponse<dynamic>(HttpStatusCode.OK, result); ;
            }
            catch (Exception ex)
            {
                LogError(ex);
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { Constants.RESPONSE_SUCCESS, false } });
            }
        }


        [HttpGet]
        [ValidateAntiForgeryToken]
        [DnnAuthorize]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage GetPagePermissions(int portalId, int tabId)
        {
            try
            {
                if (SecurityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }

                PagesControllerImpl pageController = new PagesControllerImpl();

                var pagePermissions = pageController.GetPagePermissions(portalId, tabId);

                var rolesResult = pagePermissions
                                    .Where(p => string.IsNullOrEmpty(p.RoleName) == false)
                                    .GroupBy(p => p.RoleID)
                                    .Select(p => new
                                    {
                                        PermissionID = p.FirstOrDefault().PermissionID,
                                        RoleId = p.Key,
                                        RoleName = p.FirstOrDefault().RoleName,
                                        View = p.Where(x => x.PermissionKey == Constants.VIEW).Count() > 0 ? p.Where(x => x.PermissionKey == Constants.VIEW).FirstOrDefault().AllowAccess : false,
                                        Edit = p.Where(x => x.PermissionKey == Constants.EDIT).Count() > 0 ? p.Where(x => x.PermissionKey == Constants.EDIT).FirstOrDefault().AllowAccess : false
                                    });

                var usersResult = pagePermissions
                                    .Where(p => p.UserID != Null.NullInteger)
                                    .GroupBy(p => p.UserID)
                                    .Select(p => new
                                    {
                                        PermissionID = p.FirstOrDefault().PermissionID,
                                        UserId = p.Key,
                                        UserName = p.FirstOrDefault().Username,
                                        View = p.Where(x => x.PermissionKey == Constants.VIEW).Count() > 0 ? p.Where(x => x.PermissionKey == Constants.VIEW).FirstOrDefault().AllowAccess : false,
                                        Edit = p.Where(x => x.PermissionKey == Constants.EDIT).Count() > 0 ? p.Where(x => x.PermissionKey == Constants.EDIT).FirstOrDefault().AllowAccess : false
                                    });


                return Request.CreateResponse<dynamic>(HttpStatusCode.OK, new { Roles = rolesResult, Users = usersResult }); ;
            }
            catch (Exception ex)
            {
                LogError(ex);
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { Constants.RESPONSE_SUCCESS, false } });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage SetPageName(int portalId, int tabId, string name)
        {
            try
            {
                if (SecurityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }
                PagesControllerImpl pageController = new PagesControllerImpl();

                if (string.IsNullOrEmpty(name))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(Constants.ERROR_PAGE_NAME_REQUIRED) { { Constants.RESPONSE_SUCCESS, false } });
                }

                var response = pageController.UpdatePageProperty(portalId, tabId, TabFields.Name, name);
                return Request.CreateResponse<dynamic>(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogError(ex);
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { Constants.RESPONSE_SUCCESS, false } });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage SetPageTitle(int portalId, int tabId, string title)
        {
            try
            {
                if (SecurityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }

                PagesControllerImpl pageController = new PagesControllerImpl();
                var response = pageController.UpdatePageProperty(portalId, tabId, TabFields.Title, title);
                return Request.CreateResponse<dynamic>(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogError(ex);
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { Constants.RESPONSE_SUCCESS, false } });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage SetPageDescription(int portalId, int tabId, string description)
        {
            try
            {
                if (SecurityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }
                PagesControllerImpl pageController = new PagesControllerImpl();

                var response = pageController.UpdatePageProperty(portalId, tabId, TabFields.Description, description);
                return Request.CreateResponse<dynamic>(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogError(ex);
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { Constants.RESPONSE_SUCCESS, false } });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage SetPageKeywords(int portalId, int tabId, string keywords)
        {
            try
            {
                if (SecurityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }
                PagesControllerImpl pageController = new PagesControllerImpl();

                var response = pageController.UpdatePageProperty(portalId, tabId, TabFields.Keywords, keywords);
                return Request.CreateResponse<dynamic>(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogError(ex);
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { Constants.RESPONSE_SUCCESS, false } });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage SetPagePriority(int portalId, int tabId, string priority)
        {
            try
            {
                if (SecurityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }

                PagesControllerImpl pageController = new PagesControllerImpl();

                float pagePriority;
                if (float.TryParse(priority, out pagePriority) == false)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(Constants.ERROR_PAGE_PRIORITY_INVALID) { { Constants.RESPONSE_SUCCESS, false } });
                }

                var response = pageController.UpdatePageProperty(portalId, tabId, TabFields.Priority, priority);
                return Request.CreateResponse<dynamic>(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogError(ex);
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { Constants.RESPONSE_SUCCESS, false } });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage SetPagePrimaryURL(int portalId, int tabId, string url)
        {
            try
            {
                if (SecurityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }

                PagesControllerImpl pageController = new PagesControllerImpl();

                var response = pageController.UpdatePageProperty(portalId, tabId, TabFields.PrimaryURL, url);
                return Request.CreateResponse<dynamic>(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogError(ex);
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { Constants.RESPONSE_SUCCESS, false } });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage SetPageVisibility(int portalId, int tabId, bool visible)
        {
            try
            {
                if (SecurityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }

                PagesControllerImpl pageController = new PagesControllerImpl();

                var response = pageController.UpdatePageProperty(portalId, tabId, TabFields.Visible, visible.ToString());
                return Request.CreateResponse<dynamic>(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogError(ex);
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { Constants.RESPONSE_SUCCESS, false } });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage SetPageIndexed(int portalId, int tabId, bool indexed)
        {
            try
            {
                if (SecurityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }

                PagesControllerImpl pageController = new PagesControllerImpl();

                var response = pageController.UpdatePageAllowIndex(portalId, tabId, indexed);
                return Request.CreateResponse<dynamic>(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogError(ex);
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { Constants.RESPONSE_SUCCESS, false } });
            }
        }

        private HttpResponseMessage GetForbiddenResponse()
        {
            return this.Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = Constants.ERROR_FORBIDDEN });
        }

        private void LogError(Exception ex)
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
