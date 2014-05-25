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
    [JsonRpcHelp("Secure JsonHandler", IsSecure = false)]
    public class JsonSecureHandler : JsonRpcHandler
    {
        private static JsonSecureService service = new JsonSecureService();
        public JsonSecureHandler()
        {
            if (service == null) service = new JsonSecureService();
        }
    }

    [JsonRpcHelp("Secure JsonService")]
    public class JsonSecureService : JsonRpcService
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

        #region Login User

        [JsonRpcMethod("LoginUser", Idempotent = false)]
        [JsonRpcHelp("Login User and Returns Json[message, error]")]
        public JsonObject LoginUser(string userName, string userPassword, bool rememberMe)
        {
            JsonObject retMessage = new JsonObject();

            string message = "";
            bool ret = DataSecure.LoginUser(DecodeUrl(userName), DecodeUrl(userPassword), rememberMe, out message);
            if (ret == true)
            {
                retMessage.Add("message", "Successfully Logged the User");
            }
            else
            {
                if (message.Trim().Length == 0)
                {
                    retMessage.Add("error", "Error in Loging the User");
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

        //[JsonRpcMethod("RegisterUser", Idempotent = false)]
        //[JsonRpcHelp("Register User and Returns Json[message, error]")]
        //public JsonObject RegisterUser(string userId, string userEmail, string userPassword)
        //{
        //    JsonObject retMessage = new JsonObject();

        //    string message = "";
        //    bool ret = DataSecure.RegisterUser(DecodeUrl(userId), DecodeUrl(userEmail), DecodeUrl(userPassword), out message);
        //    if (ret == true)
        //    {
        //        retMessage.Add("message", "Successfully Registered User");
        //    }
        //    else
        //    {
        //        if (message.Trim().Length == 0)
        //        {
        //            retMessage.Add("error", "Error in Registering User");
        //        }
        //        else
        //        {
        //            retMessage.Add("error", message);
        //        }
        //    }

        //    return retMessage;
        //}

        #endregion
    }
}
