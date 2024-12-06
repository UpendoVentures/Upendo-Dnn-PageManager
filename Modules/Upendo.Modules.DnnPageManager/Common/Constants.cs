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

namespace Upendo.Modules.DnnPageManager.Common
{
    public static class Constants
    {
        public const string ModuleName = "Upendo.Modules.DnnPageManager";
        public const string ModuleFolderName = "Upendo.Modules.DnnPageManager";

        public const string ModuleProperties = "moduleproperties";
        public const string ALLOW_INDEX = "AllowIndex";

        public const string SECURITY_SERVICE = "SecurityService";
        public const string PAGESCONTROLLERIMPL = "PagesControllerImpl";

        public const string DesktopModules = "DesktopModules";
        public const string Resources = "App_LocalResources";

        public static string APIPath = string.Format("/{0}/{1}/API/", DesktopModules, ModuleName);

        public const string RESPONSE_STATUS_200 = "200";
        public const string RESPONSE_SUCCESS = "IsSuccess";
        public const string SUCCESS_UPDATED = "Successfully updated!";

        public const string ASC = "asc";
        public const string DESC = "desc";

        public const string SLASH = "/";
        public const string DOT = ".";
        public const string UNDERSCORE = "_";

        public const string SYSTEM = "System";
        public const string ACCOUNT = "Account";
        public const string AUTOMATIC = "Automatic";
        public const string VIEW = "VIEW";
        public const string EDIT = "EDIT";

        public const string FORMAT_DATE = "MM/dd/yyyy hh:mm";
        public static string FORMAT_LASTUPDATED = "{0} {1} {2}";
        
        public const string NAME = "name";
        public const string TITLE = "title";

        public const string ERROR_PAGE_NAME_REQUIRED = "Page name is required!";
        public const string ERROR_PAGE_PRIORITY_INVALID = "Invalid page priority";
        public const string ERROR_PAGE_PRIORITY_RANGE_INVALID = "Priority needs to be between 0 to 1";
        public const string ERROR_PAGE_TABID_INVALID = "Invalid Tab ID";
        public const string ERROR_FORBIDDEN = "The user is not allowed to access this.";
        public const string ERROR_PAGE_URL_NOT_UNIQUE = "Page URL value is not unique ";
        public const string ERROR_PAGE_URL_DUPLICATE = "Page URL value is duplicate ";
        public const string ERROR_PAGE_URL_VALUE = "Page Url value ";

        public static string ERROR_FORMAT_UPDATE_VALUE = "Error updating value for {0}. Error: {1}";

        public static class Properties
        {
            public const string PROPERTY_ALL = "all";
            public const string PROPERTY_MODULEPATH = "modulepath";
            public const string PROPERTY_APPROOT_BEGIN = "approotangularbegin";
            public const string PROPERTY_APPROOT_END = "approotangularend";
            public const string PROPERTY_MODULEID = "ModuleId";
            public const string PROPERTY_RAWURL = "rawurl";
            public const string PROPERTY_TEST = "test";

            public static string APPROOT_OPEN = "<app-root-{0}>";
            public static string APPROOT_CLOSE = "</app-root-{0}>";

            public static string LOCALIZATION_PATH_FORMAT = "~/{0}.{1}.resx";
            public static string PHYSICAL_FILE_FORMAT = "{0}/{1}/{2}";
            public static string RELATIVE_FILE_FORMAT = "/{0}/{1}/{2}/{3}";
        }

        public static class QuickSettings
        {
            public const string MODSETTING_Title = "Title";
            public const string MODSETTING_Description = "Description";
            public const string MODSETTING_Keywords = "Keywords";
            public const string MODSETTING_DefaultFalse = "false";
            public const string MODSETTING_DefaultTrue = "true";
            public const string MODSETTING_Success = "success";
        }
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