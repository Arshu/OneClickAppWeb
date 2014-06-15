using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Text;

using Arshu.Web.Basic.Action;
using Arshu.Web.Basic.Log;
using Arshu.Web.Common;
using Arshu.Web.IO;

using Arshu.Web.Json;
using Arshu.Web.Http;

using App.Secure.Views;

namespace App.Secure
{
    [JsonRpcHelp("Secure Auth JsonHandler", IsSecure = false)]
    public class JsonAuthSecureHandler : JsonRpcSecureHandler
    {
        private static JsonAuthSecureService service = new JsonAuthSecureService();
        public JsonAuthSecureHandler()
        {
            if (service == null) service = new JsonAuthSecureService();
        }
    }

    [JsonRpcHelp("Secure JsonService")]
    public class JsonAuthSecureService : JsonRpcService
    {
        #region IsAuthenticated

        [JsonRpcMethod("IsAuthenticated", Idempotent = true)]
        [JsonRpcHelp("Check if User is Authenticated and Returns Json[message, error]")]
        public JsonObject IsAuthenticated()
        {
            JsonObject retMessage = new JsonObject();

            string message = "";
            bool ret = DataSecure.IsAuthenticated(out message);
            if (ret == false)
            {
                if (message.Trim().Length == 0)
                {
                    retMessage.Add("message", "User is authenticated");
                }
                else
                {
                    retMessage.Add("message", message);
                }
            }
            else
            {
                if (message.Trim().Length == 0)
                {
                    retMessage.Add("error", "User is not authenticated");
                }
                else
                {
                    retMessage.Add("error", message);
                }
            }

            return retMessage;
        }

        #endregion

        #region LogOff User

        [JsonRpcMethod("LogOffUser", Idempotent = true)]
        [JsonRpcHelp("LogOff User and Returns Json[message, error]")]
        public JsonObject LogOffUser()
        {
            JsonObject retMessage = new JsonObject();

            string message = "";
            bool ret = DataSecure.LogOffUser(out message);
            if (ret == true)
            {
                retMessage.Add("message", "Successfully Logged Off the User");
            }
            else
            {
                if (message.Trim().Length == 0)
                {
                    retMessage.Add("error", "Error in Loging Off the User");
                }
                else
                {
                    retMessage.Add("error", message);
                }
            }

            return retMessage;
        }

        #endregion

        #region RegisterUser

        [JsonRpcMethod("RegisterUser", Idempotent = false)]
        [JsonRpcHelp("Register User and Returns Json[message, error]")]
        public JsonObject RegisterUser(string userId, string userEmail, string userPassword)
        {
            JsonObject retMessage = new JsonObject();

            string message = "";
            bool ret = DataSecure.RegisterUser(DecodeUrl(userId), DecodeUrl(userEmail), DecodeUrl(userPassword), out message);
            if (ret == true)
            {
                retMessage.Add("message", "Successfully Registered User");
            }
            else
            {
                if (message.Trim().Length == 0)
                {
                    retMessage.Add("error", "Error in Registering User");
                }
                else
                {
                    retMessage.Add("error", message);
                }
            }

            return retMessage;
        }

        #endregion

        #region Get Login User

        [JsonRpcMethod("GetLoginUser", Idempotent = true)]
        [JsonRpcHelp("Get Login User and Returns Json[data, message, error]")]
        public JsonObject GetLoginUser()
        {
            JsonObject retMessage = new JsonObject();

            string message = "";
            bool ret = DataSecure.IsAuthenticated(out message);
            if (ret == true)
            {
                string loginUserId = DataSecure.GetLoginUser();
                retMessage.Add("data", loginUserId);
                retMessage.Add("message", "Successfully Retrieved User");
            }
            else
            {
                if (message.Trim().Length == 0)
                {
                    retMessage.Add("error", "User is not Authenticated");
                }
                else
                {
                    retMessage.Add("error", message);
                }
            }

            return retMessage;
        }

        #endregion

        #region Change Password

        [JsonRpcMethod("ChangePassword", Idempotent = false)]
        [JsonRpcHelp("Change Password and Returns Json[message, error]")]
        public JsonObject ChangePassword(string userId, string oldPassword, string newPassword)
        {
            JsonObject retMessage = new JsonObject();

            string message = "";
            bool ret = DataSecure.IsAuthenticated(out message);
            if (ret == true)
            {
                if (DataSecure.ChangePassword(userId, oldPassword, newPassword, out message) == true)
                {
                    retMessage.Add("message", "Successfully Changed Password");
                }
                else
                {
                    if (message.Trim().Length == 0)
                    {
                        retMessage.Add("error", "Error in Changing Password");
                    }
                    else
                    {
                        retMessage.Add("error", message);
                    }
                }
            }
            else
            {
                if (message.Trim().Length == 0)
                {
                    retMessage.Add("error", "User is not Authenticated");
                }
                else
                {
                    retMessage.Add("error", message);
                }
            }

            return retMessage;
        }

        #endregion

        #region Get Role Option List

        [JsonRpcMethod("GetRoleOptionListHtml", Idempotent = true)]
        [JsonRpcHelp("Get the Role Option List Html and Returns Json[html, totalItems, totalPages]")]
        public JsonObject GetRoleOptionListHtml(long pageNo, long itemsPerPage)
        {
            JsonObject retMessage = new JsonObject();

            long totalPages = 0;
            long totalItems = 0;

            string htmlText = GetPagedRoleHtml(pageNo, itemsPerPage, out totalPages, out totalItems);
            if (string.IsNullOrEmpty(htmlText) == true)
            {
                htmlText = "<option>No Role found</option>";
            }

            retMessage.Add("html", htmlText);
            retMessage.Add("totalItems", totalItems);
            retMessage.Add("totalPages", totalPages);

            return retMessage;
        }

        [JsonRpcMethod("GetRoleOptionList", Idempotent = true)]
        [JsonRpcHelp("Get the Role Option List and Returns Json[data, totalItems, totalPages]")]
        public JsonObject GetRoleOptionList(long pageNo, long itemsPerPage)
        {
            JsonObject retMessage = new JsonObject();

            long totalPages = 0;
            long totalItems = 0;

            Dictionary<string, OptionInfo> optionInfoList = GetPagedRoleList(pageNo, itemsPerPage, out totalPages, out totalItems);

            retMessage.Add("data", optionInfoList);
            retMessage.Add("totalItems", totalItems);
            retMessage.Add("totalPages", totalPages);

            return retMessage;
        }

        #endregion

        #region Get Role User List Html

        [JsonRpcMethod("GetRoleUserListHtml", Idempotent = true)]
        [JsonRpcHelp("Get the Role User List and Returns Json[html, totalItems, totalPages]")]
        public JsonObject GetRoleUserListHtml(long pageNo, long itemsPerPage)
        {
            JsonObject retMessage = new JsonObject();

            long totalPages = 0;
            long totalItems = 0;

            string htmlText = GetPagedRoleUserHtml(pageNo, itemsPerPage, out totalPages, out totalItems);
            if (string.IsNullOrEmpty(htmlText) == true)
            {
                htmlText = "<li>No User Assigned to Role(s) found</li>";
            }

            retMessage.Add("html", htmlText);
            retMessage.Add("totalItems", totalItems);
            retMessage.Add("totalPages", totalPages);

            return retMessage;
        }

        #endregion

        #region Search User List

        [JsonRpcMethod("SearchUserList", Idempotent = true)]
        [JsonRpcHelp("Get the User List Array and Returns JsonArray[{value,label}...]")]
        public JsonArray SearchUserList(string userPattern)
        {
            JsonArray retJsonArray = new JsonArray();

            retJsonArray = SearchUsers(userPattern);

            return retJsonArray;
        }


        #endregion

        #region Add User To Role

        [JsonRpcMethod("AddUserToRole", Idempotent = true)]
        [JsonRpcHelp("Add the User to the Specified Role and Returns JsonArray[{value,label}...]")]
        public JsonObject AddUserToRole(string userId, string roleName)
        {
            JsonObject retMessage = new JsonObject();

            bool ret = DataSecure.AddUserToRole(DecodeUrl(userId), DecodeUrl(roleName));
            if (ret == true)
            {
                retMessage.Add("message", "Successfully Added the User to Role");
            }
            else
            {
                retMessage.Add("error", "Error in Adding the User to Role");
            }

            return retMessage;
        }


        #endregion

        #region Remove User From Role

        [JsonRpcMethod("RemoveUserFromRole", Idempotent = true)]
        [JsonRpcHelp("Remove the User from the Specified Role and Returns JsonArray[{value,label}...]")]
        public JsonObject RemoveUserFromRole(string userId, string roleName)
        {
            JsonObject retMessage = new JsonObject();

            bool ret = DataSecure.RemoveUserFromRole(DecodeUrl(userId), DecodeUrl(roleName));
            if (ret == true)
            {
                retMessage.Add("message", "Successfully Removed the User from Role");
            }
            else
            {
                retMessage.Add("error", "Error in Removing the User from Role");
            }

            return retMessage;
        }


        #endregion



        #region Get Paged Roles

        public static string GetPagedRoleHtml(long pageNo, long itemsPerPage, out long retTotalPages, out long retTotalItems)
        {
            string retHtml = "";
            string message = "";
            long totalPages = 0;
            long totalItems = 0;

            string[] roleList = DataSecure.GetRoles();

            foreach (string roleName in roleList)
            {
                TemplateSelectOption templateOption = new TemplateSelectOption();
                templateOption.OptionText = roleName;
                templateOption.OptionValue = roleName;
                retHtml += templateOption.GetFilled("", false, false, out message);
            }

            retTotalItems = roleList.Length;
            if (roleList.Length > 0)
            {
                retTotalPages = 1;
            }

            retTotalPages = totalPages;
            retTotalItems = totalItems;

            return retHtml;
        }

        public static Dictionary<string, OptionInfo> GetPagedRoleList(long pageNo, long itemsPerPage, out long retTotalPages, out long retTotalItems)
        {
            Dictionary<string, OptionInfo> retOptionInfoList = new Dictionary<string, OptionInfo>();
            long totalPages = 0;
            long totalItems = 0;

            string[] roleList = DataSecure.GetRoles();

            foreach (string roleName in roleList)
            {
                OptionInfo optionInfo = new OptionInfo();
                optionInfo.OptionName = roleName;
                optionInfo.OptionValue = roleName;

                retOptionInfoList.Add(optionInfo.OptionValue, optionInfo);
            }

            retTotalItems = roleList.Length;
            if (roleList.Length > 0)
            {
                retTotalPages = 1;
            }

            retTotalPages = totalPages;
            retTotalItems = totalItems;

            return retOptionInfoList;
        }

        #endregion

        #region Get Paged Roles User

        public static string GetPagedRoleUserHtml(long pageNo, long itemsPerPage, out long retTotalPages, out long retTotalItems)
        {
            string retHtml = "";
            string message = "";
            long totalPages = 0;
            long totalItems = 0;

            string[] roleList = DataSecure.GetRoles();

            foreach (string roleName in roleList)
            {
                string[] userList = DataSecure.GetUsersInRole(roleName);
                if (userList.Length > 0)
                {
                    TemplateLiTwoColumn templateLiTwoColumn = new TemplateLiTwoColumn();
                    templateLiTwoColumn.Column1Value = roleName;
                    templateLiTwoColumn.Column2Value = string.Join(", ", userList);
                    retHtml += templateLiTwoColumn.GetFilled("", false, false, out message);
                }
            }

            retTotalItems = roleList.Length;
            if (roleList.Length > 0)
            {
                retTotalPages = 1;
            }

            retTotalPages = totalPages;
            retTotalItems = totalItems;

            return retHtml;
        }

        #endregion        

        #region Search Users

        public static JsonArray SearchUsers(string userPattern)
        {
            JsonArray retJsonArray = new JsonArray();

            string[] userList = DataSecure.GetUsers(userPattern);

            foreach (string userName in userList)
            {
                JsonObject data1 = new JsonObject();
                data1.Add("value", userName);
                data1.Add("label", userName);
                retJsonArray.Add(data1);
            }

            return retJsonArray;
        }

        #endregion

        #region Get US State List //TO DELETE

        private JsonArray GetUSStateList()
        {
            JsonArray retJsonArray = new JsonArray();

            //{"value":"AL","label":"Alabama"},
            JsonObject data1 = new JsonObject();
            data1.Add("value", "AL");
            data1.Add("label", "Alabama");
            retJsonArray.Add(data1);

            //{"value":"AK","label":"Alaska"},
            JsonObject data2 = new JsonObject();
            data2.Add("value", "AK");
            data2.Add("label", "Alaska");
            retJsonArray.Add(data2);

            //{"value":"AS","label":"American Samoa"},
            JsonObject data3 = new JsonObject();
            data3.Add("value", "AS");
            data3.Add("label", "American Samoa");
            retJsonArray.Add(data3);

            //{"value":"AZ","label":"Arizona"},
            JsonObject data4 = new JsonObject();
            data4.Add("value", "AZ");
            data4.Add("label", "Arizona");
            retJsonArray.Add(data4);

            //{"value":"AR","label":"Arkansas"},
            JsonObject data5 = new JsonObject();
            data5.Add("value", "AR");
            data5.Add("label", "Arkansas");
            retJsonArray.Add(data5);

            //{"value":"CA","label":"California"},
            JsonObject data6 = new JsonObject();
            data6.Add("value", "CA");
            data6.Add("label", "California");
            retJsonArray.Add(data6);

            //{"value":"CO","label":"Colorado"},
            JsonObject data7 = new JsonObject();
            data7.Add("value", "CO");
            data7.Add("label", "Colorado");
            retJsonArray.Add(data7);

            //{"value":"CT","label":"Connecticut"},
            JsonObject data8 = new JsonObject();
            data8.Add("value", "CT");
            data8.Add("label", "Connecticut");
            retJsonArray.Add(data8);

            //{"value":"DE","label":"Delaware"},
            JsonObject data9 = new JsonObject();
            data9.Add("value", "DE");
            data9.Add("label", "Delaware");
            retJsonArray.Add(data9);

            //{"value":"DC","label":"District of Columbia"},
            JsonObject data10 = new JsonObject();
            data10.Add("value", "DC");
            data10.Add("label", "District of Columbia");
            retJsonArray.Add(data10);

            //{"value":"FL","label":"Florida"},
            JsonObject data11 = new JsonObject();
            data11.Add("value", "FL");
            data11.Add("label", "Florida");
            retJsonArray.Add(data11);

            //{"value":"GA","label":"Georgia"},
            JsonObject data12 = new JsonObject();
            data12.Add("value", "GA");
            data12.Add("label", "Georgia");
            retJsonArray.Add(data12);

            //{"value":"GU","label":"Guam"},
            JsonObject data13 = new JsonObject();
            data13.Add("value", "GU");
            data13.Add("label", "Guam");
            retJsonArray.Add(data13);

            //{"value":"HI","label":"Hawaii"},
            JsonObject data14 = new JsonObject();
            data14.Add("value", "HI");
            data14.Add("label", "Hawaii");
            retJsonArray.Add(data14);

            //{"value":"ID","label":"Idaho"},
            JsonObject data15 = new JsonObject();
            data15.Add("value", "ID");
            data15.Add("label", "Idaho");
            retJsonArray.Add(data15);

            //{"value":"IL","label":"Illinois"},
            JsonObject data16 = new JsonObject();
            data16.Add("value", "IL");
            data16.Add("label", "Illinois");
            retJsonArray.Add(data16);

            //{"value":"IN","label":"Indiana"},
            JsonObject data17 = new JsonObject();
            data17.Add("value", "IN");
            data17.Add("label", "Indiana");
            retJsonArray.Add(data17);

            //{"value":"IA","label":"Iowa"},
            JsonObject data18 = new JsonObject();
            data18.Add("value", "IA");
            data18.Add("label", "Iowa");
            retJsonArray.Add(data18);

            //{"value":"KS","label":"Kansas"},
            JsonObject data19 = new JsonObject();
            data19.Add("value", "KS");
            data19.Add("label", "Kansas");
            retJsonArray.Add(data19);

            //{"value":"KY","label":"Kentucky"},
            JsonObject data20 = new JsonObject();
            data20.Add("value", "KY");
            data20.Add("label", "Kentucky");
            retJsonArray.Add(data20);

            //{"value":"LA","label":"Louisiana"},
            JsonObject data22 = new JsonObject();
            data22.Add("value", "LA");
            data22.Add("label", "Louisiana");
            retJsonArray.Add(data22);

            //{"value":"ME","label":"Maine"},
            JsonObject data23 = new JsonObject();
            data23.Add("value", "ME");
            data23.Add("label", "Maine");
            retJsonArray.Add(data23);

            //{"value":"MD","label":"Maryland"},
            JsonObject data24 = new JsonObject();
            data24.Add("value", "MD");
            data24.Add("label", "Maryland");
            retJsonArray.Add(data24);

            //{"value":"MA","label":"Massachusetts"},
            JsonObject data25 = new JsonObject();
            data25.Add("value", "MA");
            data25.Add("label", "Massachusetts");
            retJsonArray.Add(data25);

            //{"value":"MI","label":"Michigan"},
            JsonObject data26 = new JsonObject();
            data26.Add("value", "MI");
            data26.Add("label", "Michigan");
            retJsonArray.Add(data26);

            //{"value":"MN","label":"Minnesota"},
            JsonObject data27 = new JsonObject();
            data27.Add("value", "MN");
            data27.Add("label", "Minnesota");
            retJsonArray.Add(data27);

            //{"value":"MS","label":"Mississippi"},
            JsonObject data28 = new JsonObject();
            data28.Add("value", "MS");
            data28.Add("label", "Mississippi");
            retJsonArray.Add(data28);

            //{"value":"MO","label":"Missouri"},
            JsonObject data29 = new JsonObject();
            data29.Add("value", "MO");
            data29.Add("label", "Missouri");
            retJsonArray.Add(data29);

            //{"value":"MT","label":"Montana"},
            JsonObject data30 = new JsonObject();
            data30.Add("value", "MT");
            data30.Add("label", "Montana");
            retJsonArray.Add(data30);

            //{"value":"NE","label":"Nebraska"},
            JsonObject data31 = new JsonObject();
            data31.Add("value", "NE");
            data31.Add("label", "Nebraska");
            retJsonArray.Add(data31);

            //{"value":"NV","label":"Nevada"},
            JsonObject data32 = new JsonObject();
            data32.Add("value", "NV");
            data32.Add("label", "Nevada");
            retJsonArray.Add(data32);

            //{"value":"NH","label":"New Hampshire"},
            JsonObject data33 = new JsonObject();
            data33.Add("value", "NH");
            data33.Add("label", "New Hampshire");
            retJsonArray.Add(data33);

            //{"value":"NJ","label":"New Jersey"},
            JsonObject data34 = new JsonObject();
            data34.Add("value", "NJ");
            data34.Add("label", "New Jersey");
            retJsonArray.Add(data34);

            //{"value":"NM","label":"New Mexico"},
            JsonObject data35 = new JsonObject();
            data35.Add("value", "NM");
            data35.Add("label", "New Mexico");
            retJsonArray.Add(data35);

            //{"value":"NY","label":"New York"},
            JsonObject data36 = new JsonObject();
            data36.Add("value", "NY");
            data36.Add("label", "New York");
            retJsonArray.Add(data36);

            //{"value":"NC","label":"North Carolina"},
            JsonObject data37 = new JsonObject();
            data37.Add("value", "NC");
            data37.Add("label", "North Carolina");
            retJsonArray.Add(data37);

            //{"value":"ND","label":"North Dakota"},
            JsonObject data38 = new JsonObject();
            data38.Add("value", "ND");
            data38.Add("label", "North Dakota");
            retJsonArray.Add(data38);

            //{"value":"NI","label":"Northern Marianas Islands"},
            JsonObject data39 = new JsonObject();
            data39.Add("value", "NI");
            data39.Add("label", "Northern Marianas Islands");
            retJsonArray.Add(data39);

            //{"value":"OH","label":"Ohio"},
            JsonObject data40 = new JsonObject();
            data40.Add("value", "OH");
            data40.Add("label", "Ohio");
            retJsonArray.Add(data40);

            //{"value":"OK","label":"Oklahoma"},
            JsonObject data41 = new JsonObject();
            data41.Add("value", "OK");
            data41.Add("label", "Oklahoma");
            retJsonArray.Add(data41);

            //{"value":"OR","label":"Oregon"},
            JsonObject data42 = new JsonObject();
            data42.Add("value", "OR");
            data42.Add("label", "Oregon");
            retJsonArray.Add(data42);

            //{"value":"PA","label":"Pennsylvania"},
            JsonObject data43 = new JsonObject();
            data43.Add("value", "PA");
            data43.Add("label", "Pennsylvania");
            retJsonArray.Add(data2);

            //{"value":"PR","label":"Puerto Rico"},
            JsonObject data44 = new JsonObject();
            data44.Add("value", "PR");
            data44.Add("label", "Puerto Rico");
            retJsonArray.Add(data44);

            //{"value":"RI","label":"Rhode Island"},
            JsonObject data45 = new JsonObject();
            data45.Add("value", "RI");
            data45.Add("label", "Rhode Island");
            retJsonArray.Add(data45);

            //{"value":"SC","label":"South Carolina"},
            JsonObject data46 = new JsonObject();
            data46.Add("value", "SC");
            data46.Add("label", "South Carolina");
            retJsonArray.Add(data46);

            //{"value":"SD","label":"South Dakota"},
            JsonObject data47 = new JsonObject();
            data47.Add("value", "SD");
            data47.Add("label", "South Dakota");
            retJsonArray.Add(data47);

            //{"value":"TN","label":"Tennessee"},
            JsonObject data48 = new JsonObject();
            data48.Add("value", "TN");
            data48.Add("label", "Tennessee");
            retJsonArray.Add(data48);

            //{"value":"TX","label":"Texas"},
            JsonObject data49 = new JsonObject();
            data49.Add("value", "TX");
            data49.Add("label", "Texas");
            retJsonArray.Add(data49);

            //{"value":"UT","label":"Utah"},
            JsonObject data50 = new JsonObject();
            data50.Add("value", "UT");
            data50.Add("label", "Utah");
            retJsonArray.Add(data50);

            //{"value":"VT","label":"Vermont"},
            JsonObject data51 = new JsonObject();
            data51.Add("value", "VT");
            data51.Add("label", "Vermont");
            retJsonArray.Add(data51);

            //{"value":"VI","label":"Virgin Islands"},
            JsonObject data52 = new JsonObject();
            data52.Add("value", "VI");
            data52.Add("label", "Virgin Islands");
            retJsonArray.Add(data52);

            //{"value":"VA","label":"Virginia"},
            JsonObject data53 = new JsonObject();
            data53.Add("value", "VA");
            data53.Add("label", "Virginia");
            retJsonArray.Add(data53);

            //{"value":"WA","label":"Washington"},
            JsonObject data54 = new JsonObject();
            data54.Add("value", "WA");
            data54.Add("label", "Washington");
            retJsonArray.Add(data54);

            //{"value":"WV","label":"West Virginia"},
            JsonObject data55 = new JsonObject();
            data55.Add("value", "WV");
            data55.Add("label", "West Virginia");
            retJsonArray.Add(data55);

            //{"value":"WI","label":"Wisconsin"},
            JsonObject data56 = new JsonObject();
            data56.Add("value", "WI");
            data56.Add("label", "Wisconsin");
            retJsonArray.Add(data56);

            //{"value":"WY","label":"Wyoming"}
            JsonObject data57 = new JsonObject();
            data57.Add("value", "WY");
            data57.Add("label", "Wyoming");
            retJsonArray.Add(data57);


            //var autocompleteData = $.parseJSON('[{"value":"AL","label":"Alabama"},{"value":"AK","label":"Alaska"},{"value":"AS","label":"American Samoa"},{"value":"AZ","label":"Arizona"},{"value":"AR","label":"Arkansas"},{"value":"CA","label":"California"},{"value":"CO","label":"Colorado"},{"value":"CT","label":"Connecticut"},{"value":"DE","label":"Delaware"},{"value":"DC","label":"District of Columbia"},{"value":"FL","label":"Florida"},{"value":"GA","label":"Georgia"},{"value":"GU","label":"Guam"},{"value":"HI","label":"Hawaii"},{"value":"ID","label":"Idaho"},{"value":"IL","label":"Illinois"},{"value":"IN","label":"Indiana"},{"value":"IA","label":"Iowa"},{"value":"KS","label":"Kansas"},{"value":"KY","label":"Kentucky"},{"value":"LA","label":"Louisiana"},{"value":"ME","label":"Maine"},{"value":"MD","label":"Maryland"},{"value":"MA","label":"Massachusetts"},{"value":"MI","label":"Michigan"},{"value":"MN","label":"Minnesota"},{"value":"MS","label":"Mississippi"},{"value":"MO","label":"Missouri"},{"value":"MT","label":"Montana"},{"value":"NE","label":"Nebraska"},{"value":"NV","label":"Nevada"},{"value":"NH","label":"New Hampshire"},{"value":"NJ","label":"New Jersey"},{"value":"NM","label":"New Mexico"},{"value":"NY","label":"New York"},{"value":"NC","label":"North Carolina"},{"value":"ND","label":"North Dakota"},{"value":"NI","label":"Northern Marianas Islands"},{"value":"OH","label":"Ohio"},{"value":"OK","label":"Oklahoma"},{"value":"OR","label":"Oregon"},{"value":"PA","label":"Pennsylvania"},{"value":"PR","label":"Puerto Rico"},{"value":"RI","label":"Rhode Island"},{"value":"SC","label":"South Carolina"},{"value":"SD","label":"South Dakota"},{"value":"TN","label":"Tennessee"},{"value":"TX","label":"Texas"},{"value":"UT","label":"Utah"},{"value":"VT","label":"Vermont"},{"value":"VI","label":"Virgin Islands"},{"value":"VA","label":"Virginia"},{"value":"WA","label":"Washington"},{"value":"WV","label":"West Virginia"},{"value":"WI","label":"Wisconsin"},{"value":"WY","label":"Wyoming"}]');

            return retJsonArray;
        }

        #endregion
    }
}
