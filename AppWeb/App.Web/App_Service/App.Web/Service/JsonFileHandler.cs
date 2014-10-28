using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Arshu.Web.Common;
using Arshu.Web.IO;
using Arshu.Web.Json;

using App.Web.Views;

namespace Arshu.AppWeb
{
    [JsonRpcHelp("Json File Handler")]
    public class JsonFileHandler : JsonRpcHandler
    {
        public static string JsonBaseHandlerName = "JsonFile.ashx";
        public static string JsonBaseHandlerTypeName = typeof(JsonFileHandler).FullName;

        private static JsonFileService service = new JsonFileService();
        public JsonFileHandler()
        {
            if (service == null) service = new JsonFileService();
        }         
    }

    [JsonRpcHelp("Json File Service")]
    public class JsonFileService : JsonRpcService
    {
        #region Inbox Related

        [JsonRpcMethod("GetInboxAppsListHtml")]
        [JsonRpcHelp("Get Inbox Apps and Return Json[html, totalItems, totalPages]")]
        public JsonObject GetInboxAppsListHtml(long pageNo, long itemsPerPage)
        {
            JsonObject retMessage = new JsonObject();

            try
            {
                long totalPages = 0;
                long totalItems = 0;

                string htmlText = GetPagedInboxAppHtml(pageNo, itemsPerPage, out totalPages, out totalItems);
                if (string.IsNullOrEmpty(htmlText) == true)
                {
                    htmlText = "<li>No Imported Apps found in Inbox. </li>";
                }

                retMessage.Add("html", htmlText);
                retMessage.Add("totalItems", totalItems);
                retMessage.Add("totalPages", totalPages);
            }
            catch (Exception ex)
            {
                retMessage.Add("error", ex.Message);
            }

            return retMessage;
        }

        [JsonRpcMethod("DeleteInboxApp")]
        [JsonRpcHelp("Delete Inbox App using Encrypted AppID")]
        public JsonObject DeleteInboxApp(string appID)
        {
            var retMessage = new JsonObject();
            DateTime startTime = DateTime.UtcNow;

            try
            {
                PathInfo appPath = null;
                List<PathInfo> appPathList = IOManager.GetAppNameList(IOManager.InboxDirectory);
                foreach (PathInfo appInfo in appPathList)
                {
                    string appFilePathHash = MD5Core.GetHashString(appInfo.PathValue);
                    if (appFilePathHash == appID)
                    {
                        appPath = appInfo;
                        break;
                    }
                }

                if (appPath != null)
                {
                    bool ret = IOManager.DeleteAppData(appPath.PathName, appPath.PathValue);
                    if (ret == true)
                    {
                        retMessage.Add("message", "Successfully Deleted Inbox App.");
                    }
                    else
                    {
                        retMessage.Add("message", "Unable to Delete Inbox App");
                    }
                }
                else
                {
                    retMessage.Add("error", "Invalid App ID. Unable to Delete Inbox App");
                }
            }
            catch (Exception ex)
            {
                retMessage.Add("error", "DeleteInboxApp-Error=" + ex.Message);
            }

            return retMessage;
        }

        #endregion

        #region Demo Related

        [JsonRpcMethod("GetDemoAppsListHtml")]
        [JsonRpcHelp("Get Demo Apps and Return Json[html, totalItems, totalPages]")]
        public JsonObject GetDemoAppsListHtml(long pageNo, long itemsPerPage)
        {
            JsonObject retMessage = new JsonObject();

            try
            {
                long totalPages = 0;
                long totalItems = 0;

                string htmlText = GetPagedDemoAppHtml(pageNo, itemsPerPage, out totalPages, out totalItems);
                if (string.IsNullOrEmpty(htmlText) == true)
                {
                    htmlText = "<li>No Demo Apps found. </li>";
                }

                retMessage.Add("html", htmlText);
                retMessage.Add("totalItems", totalItems);
                retMessage.Add("totalPages", totalPages);
            }
            catch (Exception ex)
            {
                retMessage.Add("error", ex.Message);
            }

            return retMessage;
        }

        #endregion

        #region File System Related

        [JsonRpcMethod("GetTopDirectories")]
        [JsonRpcHelp("Get Top Directories from a Specified Relative Path matching the Dir Pattern and Return Json[data, error]")]
        public JsonObject GetTopDirectories(string relativePath, string dirPattern)
        {
            JsonObject retMessage = new JsonObject();

            try
            {
                //System.Threading.Thread.Sleep(5000);
                string rootDirectory = IOManager.RootDirectory;
                if ((relativePath.Trim() != "/") && (relativePath != "\\"))
                {
                    if ((relativePath.StartsWith("/") == true) || (relativePath.StartsWith("\\") == true))
                    {
                        relativePath = relativePath.Substring(1);
                    }
                    rootDirectory = IOManager.Combine(IOManager.RootDirectory, IOManager.ProcessPath(relativePath));
                }
                if (IOManager.CachedDirExists(rootDirectory, false, false) == true)
                {
                    string[] dirList = IOManager.GetTopDirectories(rootDirectory, dirPattern);
                    Dictionary<string, string> dirNameList = new Dictionary<string, string>();
                    if (dirList.Length > 0)
                    {
                        foreach (var item in dirList)
                        {
                            string directoryName = IOManager.GetDirectoryName(item);
                            string directoryPath = IOManager.ProcessPath(item.Replace(IOManager.RootDirectory, "/")).Replace("\\", "/");
                            dirNameList.Add(directoryPath, directoryName);
                        }
                        retMessage.Add("data", dirNameList);
                    }
                    else
                    {
                        retMessage.Add("error", "No Directories found in under Relative Path [" + relativePath + "] matching the Directory Pattern [" + dirPattern + "]");
                    }
                }
                else
                {
                    retMessage.Add("error", "Relative Path does not exist");
                }
            }
            catch (Exception ex)
            {
                retMessage.Add("error", ex.Message);                
            }

            return retMessage;
        }

        [JsonRpcMethod("GetTopFiles", Idempotent = true)]
        [JsonRpcHelp("Get Top Files from a Specified Relative Path matching the File Pattern and Return Json[data, error]")]
        public JsonObject GetTopFiles(string relativePath, string filePattern)
        {
            JsonObject retMessage = new JsonObject();

            string rootDirectory = IOManager.RootDirectory;
            if ((relativePath.Trim() != "/") && (relativePath != "\\"))
            {
                if ((relativePath.StartsWith("/") == true) || (relativePath.StartsWith("\\") == true))
                {
                    relativePath = relativePath.Substring(1);
                }
                rootDirectory = IOManager.Combine(IOManager.RootDirectory, IOManager.ProcessPath(relativePath));
            }
            if (IOManager.CachedDirExists(rootDirectory, false, false) == true)
            {
                string[] fileList = IOManager.GetTopFiles(rootDirectory, filePattern);
                if (fileList.Length > 0)
                {
                    Dictionary<string, string> fileNameList = new Dictionary<string, string>();
                    foreach (var item in fileList)
                    {
                        string fileName = IOManager.GetFileName(item);
                        string filePath = IOManager.ProcessPath(item.Replace(IOManager.RootDirectory, "/")).Replace("\\", "/");
                        fileNameList.Add(filePath, fileName);
                    }
                    retMessage.Add("data", fileNameList);
                }
                else
                {
                    retMessage.Add("error", "No Files found in the Relative Path [" + relativePath + "] matching the File Pattern [" + filePattern + "]");
                }
            }
            else
            {
                retMessage.Add("error", "Relative Path does not exist");
            }

            return retMessage;
        }

        [JsonRpcMethod("GetFileData", Idempotent = true)]
        [JsonRpcHelp("Get File Data from a Specified Relative Path and Return Json[data, error]")]
        public JsonObject GetFileData(string relativePath)
        {
            JsonObject retMessage = new JsonObject();

            string filePath = "";
            if ((relativePath.Trim() != "/") && (relativePath != "\\"))
            {
                if ((relativePath.StartsWith("/") == true) || (relativePath.StartsWith("\\") == true))
                {
                    relativePath = relativePath.Substring(1);
                }
                filePath = IOManager.Combine(IOManager.RootDirectory, relativePath);
            }
            if (IOManager.CachedFileExists(filePath, false, false) == true)
            {
                string fileData = IOManager.ReadFile(filePath);
                retMessage.Add("data", fileData);
            }
            else
            {
                retMessage.Add("error", "File Relative Path does not exist");
            }

            return retMessage;
        }

        [JsonRpcMethod("SaveFileData", Idempotent = true)]
        [JsonRpcHelp("Save File Data to a Specified Relative Path and Return Json[data, error]")]
        public JsonObject SaveFileData(string relativePath, string fileData, bool create)
        {
            JsonObject retMessage = new JsonObject();

            string filePath = "";
            if ((relativePath.Trim() != "/") && (relativePath != "\\"))
            {
                if ((relativePath.StartsWith("/") == true) || (relativePath.StartsWith("\\") == true))
                {
                    relativePath = relativePath.Substring(1);
                }
                filePath = IOManager.Combine(IOManager.RootDirectory, relativePath);
            }
            if (IOManager.CachedFileExists(filePath, false, false) == true)
            {
                IOManager.Delete(filePath);
                IOManager.WriteFile(filePath, fileData);
                retMessage.Add("data", "File Data Saved Successfully");
            }
            else
            {
                if (create == true)
                {
                    IOManager.WriteFile(filePath, fileData);
                    retMessage.Add("data", "File Data Created Successfully");
                }
                else
                {
                    retMessage.Add("error", "File Relative Path Does not exist");
                }
            }

            return retMessage;
        }

        [JsonRpcMethod("AppendFileData", Idempotent = true)]
        [JsonRpcHelp("Append File Data to a Specified Relative Path and Return Json[data, error]")]
        public JsonObject AppendFileData(string relativePath, string fileData)
        {
            JsonObject retMessage = new JsonObject();

            string filePath = "";
            if ((relativePath.Trim() != "/") && (relativePath != "\\"))
            {
                if ((relativePath.StartsWith("/") == true) || (relativePath.StartsWith("\\") == true))
                {
                    relativePath = relativePath.Substring(1);
                }
                filePath = IOManager.Combine(IOManager.RootDirectory, relativePath);
            }
            if (IOManager.CachedFileExists(filePath, false, false) == true)
            {
                IOManager.AppendFile(filePath, fileData);
                retMessage.Add("data", "File Data Saved Successfully");
            }
            else
            {
                retMessage.Add("error", "File Relative Path does not exist");
            }

            return retMessage;
        }

        #endregion

        #region Get Paged Inbox App List Html

        #region Get Paged Inbox App

        public static string GetPagedInboxAppHtml(long pageNo, long itemsPerPage, out long retTotalPages, out long retTotalItems)
        {
            string retHtml = "";
            string message = "";
            long totalPages = 0;
            long totalItems = 0;

            List<PathInfo> appPathList = IOManager.GetAppNameList(IOManager.InboxDirectory);

            foreach (PathInfo appInfo in appPathList)
            {
                TemplateInboxColumn templateInboxColumn = new TemplateInboxColumn();
                templateInboxColumn.AppName = appInfo.PathName;
                templateInboxColumn.AppId = MD5Core.GetHashString(appInfo.PathValue);
                retHtml += templateInboxColumn.GetFilled("", false, false, out message);
            }

            retTotalItems = appPathList.Count;
            if (appPathList.Count <= itemsPerPage)
            {
                retTotalPages = 1;
            }
            else
            {
                retTotalPages = Convert.ToInt32(Math.Floor(Convert.ToDecimal(retTotalItems) / Convert.ToDecimal(itemsPerPage)));
            }

            retTotalPages = totalPages;
            retTotalItems = totalItems;

            return retHtml;
        }

        #endregion        

        #endregion

        #region Get Paged Demo App List Html

        #region Get Paged Demo App

        public static string GetPagedDemoAppHtml(long pageNo, long itemsPerPage, out long retTotalPages, out long retTotalItems)
        {
            string retHtml = "";
            string message = "";
            long totalPages = 0;
            long totalItems = 0;

            List<PathInfo> appPathList = IOManager.GetAppNameList(IOManager.WebSitePagesDemoDirName);

            foreach (PathInfo appInfo in appPathList)
            {
                TemplateInboxColumn templateInboxColumn = new TemplateInboxColumn();
                templateInboxColumn.AppName = appInfo.PathName;
                templateInboxColumn.AppId = MD5Core.GetHashString(appInfo.PathValue);
                retHtml += templateInboxColumn.GetFilled("", false, false, out message);
            }

            retTotalItems = appPathList.Count;
            if (appPathList.Count <= itemsPerPage)
            {
                retTotalPages = 1;
            }
            else
            {
                retTotalPages = Convert.ToInt32(Math.Floor(Convert.ToDecimal(retTotalItems) / Convert.ToDecimal(itemsPerPage)));
            }

            retTotalPages = totalPages;
            retTotalItems = totalItems;

            return retHtml;
        }

        #endregion

        #endregion

    }
}
