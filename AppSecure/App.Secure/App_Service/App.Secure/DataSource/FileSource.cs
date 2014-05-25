
using System;
using System.Collections.Generic;
using Arshu.Web.Json;
using App.Secure.Entity;

namespace App.Secure.Data
{
    internal static partial class FileSource
    {

        public static Dictionary<Guid, SYD_User> LoadSYDUserData()
        {
            Dictionary<Guid, SYD_User> keysydUserList = new Dictionary<Guid, SYD_User>();

            List<SYD_User> sydUserList = JsonStore<SYD_User>.LoadData(false);
            foreach (SYD_User item in sydUserList)
            {
                keysydUserList.Add(item.UserGUID, item);
            }
            return keysydUserList;
        }

        public static bool SaveSYDUserData(Dictionary<Guid, SYD_User> sydUserList)
        {
            bool ret = false;
            if (sydUserList.Count > 0)
            {
                List<SYD_User> sydUserValueList = new List<SYD_User>();
                SYD_User[] sydUserArray = new SYD_User[sydUserList.Values.Count];
                sydUserList.Values.CopyTo(sydUserArray, 0);
                sydUserValueList.AddRange(sydUserArray);
                ret = JsonStore<SYD_User>.SaveData(sydUserValueList, true, false);
            }
			else
			{
                ret = JsonStore<SYD_User>.DeleteData();
			}

            return ret;
        }

        public static List<SYD_User> GetPagedSYDUser(Dictionary<Guid, SYD_User> allSYDUserList, long pageNo, long itemsPerPage, out long totalPages, out long totalItems)
        {
            List<SYD_User> pagedSYDUserList = new List<SYD_User>();
            totalItems = allSYDUserList.Count;
            totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)itemsPerPage);

            if (pageNo > 0) pageNo = pageNo - 1;
            long startIndex = pageNo * itemsPerPage;
            if (startIndex > totalItems) startIndex = 0;
            long endIndex = startIndex + itemsPerPage;
            if (endIndex > totalItems) endIndex = totalItems;

            int i = 0;
            foreach (KeyValuePair<Guid, SYD_User> item in allSYDUserList)
            {
                if ((i >= (int)startIndex) && (i < endIndex))
                {
                    pagedSYDUserList.Add(item.Value);
                }
                i++;
            }

            return pagedSYDUserList;
        }

        public static Dictionary<Guid, SYS_Version> LoadSYSVersionData()
        {
            Dictionary<Guid, SYS_Version> keysysVersionList = new Dictionary<Guid, SYS_Version>();

            List<SYS_Version> sysVersionList = JsonStore<SYS_Version>.LoadData(false);
            foreach (SYS_Version item in sysVersionList)
            {
                keysysVersionList.Add(item.VersionNoGUID, item);
            }
            return keysysVersionList;
        }

        public static bool SaveSYSVersionData(Dictionary<Guid, SYS_Version> sysVersionList)
        {
            bool ret = false;
            if (sysVersionList.Count > 0)
            {
                List<SYS_Version> sysVersionValueList = new List<SYS_Version>();
                SYS_Version[] sysVersionArray = new SYS_Version[sysVersionList.Values.Count];
                sysVersionList.Values.CopyTo(sysVersionArray, 0);
                sysVersionValueList.AddRange(sysVersionArray);
                ret = JsonStore<SYS_Version>.SaveData(sysVersionValueList, true, false);
            }
			else
			{
                ret = JsonStore<SYS_Version>.DeleteData();
			}

            return ret;
        }

        public static List<SYS_Version> GetPagedSYSVersion(Dictionary<Guid, SYS_Version> allSYSVersionList, long pageNo, long itemsPerPage, out long totalPages, out long totalItems)
        {
            List<SYS_Version> pagedSYSVersionList = new List<SYS_Version>();
            totalItems = allSYSVersionList.Count;
            totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)itemsPerPage);

            if (pageNo > 0) pageNo = pageNo - 1;
            long startIndex = pageNo * itemsPerPage;
            if (startIndex > totalItems) startIndex = 0;
            long endIndex = startIndex + itemsPerPage;
            if (endIndex > totalItems) endIndex = totalItems;

            int i = 0;
            foreach (KeyValuePair<Guid, SYS_Version> item in allSYSVersionList)
            {
                if ((i >= (int)startIndex) && (i < endIndex))
                {
                    pagedSYSVersionList.Add(item.Value);
                }
                i++;
            }

            return pagedSYSVersionList;
        }
    }
}
