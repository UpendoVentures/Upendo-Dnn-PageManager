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

using DotNetNuke.Web.Api;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Common.Utilities;
using Upendo.Modules.DnnPageManager.Common;
using Upendo.Modules.DnnPageManager.Components;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;

namespace Upendo.Modules.DnnPageManager.Controller
{
    public class PagesController : DnnApiController
    {

        private readonly IPagesControllerImpl pageController;
        private readonly ISecurityService securityService;

        public PagesController() : this(PagesControllerImpl.Instance, SecurityService.Instance)
        {
        }

        public PagesController(IPagesControllerImpl pageController, ISecurityService securityService)
        {
            this.pageController = pageController;
            this.securityService = securityService;
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        [DnnAuthorize]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage GetPagesList(int portalId, string searchKey = "", int pageIndex = -1, int pageSize = 10, string sortBy = "", string sortType = "", bool? deleted = false)
        {
            int total = 0;

            try
            {

                if (this.securityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }


                var pages = pageController.GetPagesList(portalId: portalId,
                                                         total: out total,
                                                         searchKey: searchKey,
                                                         pageIndex: pageIndex,
                                                         pageSize: pageSize,
                                                         deleted: deleted
                                                        );

                var result = pages.Select(p => new
                {
                    Id = p.TabID,
                    Name = p.LocalizedTabName,
                    Title = p.Title,
                    Description = p.Description,
                    Keywords = p.KeyWords,
                    Priority = p.SiteMapPriority,
                    PrimaryUrl = pageController.GetPageUrls(p.PortalID, p.TabID).FirstOrDefault().Path,
                    LastUpdated = string.Format("{0} {1} {2}", (p.LastModifiedByUserID == Null.NullInteger ? "System" : p.LastModifiedByUser(portalId).FirstName), (p.LastModifiedByUserID == Null.NullInteger ? "Account" : p.LastModifiedByUser(portalId).LastName), p.LastModifiedOnDate.ToString("MM/dd/yyyy hh:mm")),
                    IsVisible = p.IsVisible,
                    IsAllowedSearch = p.AllowIndex,
                    IsDisabled = p.DisableLink,
                    IsIndexed = p.Indexed,
                    HasBeenPublished = p.HasBeenPublished
                });

                string sortOn = sortBy.ToLowerInvariant();
                string sortOrder = sortType.ToLowerInvariant();

                if (String.IsNullOrEmpty(sortBy) == false)
                {
                    switch (sortBy.ToLowerInvariant())
                    {
                        case "name":
                            result = sortOrder == "asc" ? result.OrderBy(x => x.Name) : result.OrderByDescending(x => x.Name);
                            break;
                        case "title":
                            result = sortOrder == "asc" ? result.OrderBy(x => x.Title) : result.OrderByDescending(x => x.Title);
                            break;
                        default:
                            break;
                    }
                }

                return Request.CreateResponse<dynamic>(HttpStatusCode.OK, new { Total = total.ToString(), result });
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { "IsSuccess", false } });
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
                if (this.securityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }

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
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { "IsSuccess", false } });
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
                if (this.securityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }

                var pageUrls = pageController.GetPageUrls(portalId, tabId);

                var result = pageUrls.Select(m => new
                {
                    Url = m.Path,
                    UrlType = m.StatusCode.Value,
                    GeneratedBy = m.IsSystem ? "Automatic" : m.UserName
                });

                return Request.CreateResponse<dynamic>(HttpStatusCode.OK, result); ;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { "IsSuccess", false } });
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
                if (this.securityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }

                var pagePermissions = pageController.GetPagePermissions(portalId, tabId);

                var rolesResult = pagePermissions
                                    .Where(p => string.IsNullOrEmpty(p.RoleName) == false)
                                    .GroupBy(p => p.RoleID)
                                    .Select(p => new
                                    {
                                        PermissionID = p.FirstOrDefault().PermissionID,
                                        RoleId = p.Key,
                                        RoleName = p.FirstOrDefault().RoleName,
                                        View = p.Where(x => x.PermissionKey == "VIEW").Count() > 0 ? p.Where(x => x.PermissionKey == "VIEW").FirstOrDefault().AllowAccess : false,
                                        Edit = p.Where(x => x.PermissionKey == "EDIT").Count() > 0 ? p.Where(x => x.PermissionKey == "EDIT").FirstOrDefault().AllowAccess : false
                                    });

                var usersResult = pagePermissions
                                    .Where(p => p.UserID != Null.NullInteger)
                                    .GroupBy(p => p.UserID)
                                    .Select(p => new
                                    {
                                        PermissionID = p.FirstOrDefault().PermissionID,
                                        UserId = p.Key,
                                        UserName = p.FirstOrDefault().Username,
                                        View = p.Where(x => x.PermissionKey == "VIEW").Count() > 0 ? p.Where(x => x.PermissionKey == "VIEW").FirstOrDefault().AllowAccess : false,
                                        Edit = p.Where(x => x.PermissionKey == "EDIT").Count() > 0 ? p.Where(x => x.PermissionKey == "EDIT").FirstOrDefault().AllowAccess : false
                                    });


                return Request.CreateResponse<dynamic>(HttpStatusCode.OK, new { Roles = rolesResult, Users = usersResult }); ;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { "IsSuccess", false } });
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
                if (this.securityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }

                if (string.IsNullOrEmpty(name))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError("Page name is required!") { { "IsSuccess", false } });
                }

                var response = pageController.UpdatePageProperty(portalId, tabId, TabFields.Name, name);
                return Request.CreateResponse<dynamic>(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { "IsSuccess", false } });
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
                if (this.securityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }

                var response = pageController.UpdatePageProperty(portalId, tabId, TabFields.Title, title);
                return Request.CreateResponse<dynamic>(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { "IsSuccess", false } });
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
                if (this.securityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }

                var response = pageController.UpdatePageProperty(portalId, tabId, TabFields.Description, description);
                return Request.CreateResponse<dynamic>(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { "IsSuccess", false } });
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
                if (this.securityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }

                var response = pageController.UpdatePageProperty(portalId, tabId, TabFields.Keywords, keywords);
                return Request.CreateResponse<dynamic>(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { "IsSuccess", false } });
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
                if (this.securityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }

                float pagePriority;
                if (float.TryParse(priority, out pagePriority) == false)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError("Invalid page priority") { { "IsSuccess", false } });
                }

                var response = pageController.UpdatePageProperty(portalId, tabId, TabFields.Priority, priority);
                return Request.CreateResponse<dynamic>(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { "IsSuccess", false } });
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
                if (this.securityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }

                var response = pageController.UpdatePageProperty(portalId, tabId, TabFields.PrimaryURL, url);
                return Request.CreateResponse<dynamic>(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { "IsSuccess", false } });
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
                if (this.securityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }

                var response = pageController.UpdatePageProperty(portalId, tabId, TabFields.Visible, visible.ToString());
                return Request.CreateResponse<dynamic>(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { "IsSuccess", false } });
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
                if (this.securityService.IsPagesAdminUser() == false)
                {
                    return this.GetForbiddenResponse();
                }

                var response = pageController.UpdatePageProperty(portalId, tabId, TabFields.Indexed, indexed.ToString());
                return Request.CreateResponse<dynamic>(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message) { { "IsSuccess", false } });
            }
        }

        private HttpResponseMessage GetForbiddenResponse()
        {
            return this.Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "The user is not allowed to access this." });
        }
    }
}