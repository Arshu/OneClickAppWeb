using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Arshu.Web.Common;
using Arshu.Web.IO;
using Arshu.Web.Json;

namespace Arshu.AppGrid
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
    }
}
