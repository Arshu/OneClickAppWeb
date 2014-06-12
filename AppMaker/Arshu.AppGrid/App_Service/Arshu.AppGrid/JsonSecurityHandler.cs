using System;
using System.Collections;
using System.Collections.Generic;
//using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Data;
using System.Data.Common;

using Arshu.Web.Common;
using Arshu.Web.Common.SMTP;
using Arshu.Web.IO;

using Arshu.Web.Http;
using Arshu.Web.Json;
using Arshu.Web.Security;

namespace Arshu.AppGrid
{
    [JsonRpcHelp("Json Security Handler", IsSecure = false)]
    public class JsonSecurityHandler : JsonRpcHandler
    {
        public static string JsonBaseHandlerName = "JsonSecurity.ashx";
        public static string JsonBaseHandlerTypeName = typeof(JsonSecurityHandler).FullName;

        private static JsonSecurityService service = new JsonSecurityService();
        public JsonSecurityHandler()
        {
            if (service == null) service = new JsonSecurityService();
        }
    }

    [JsonRpcHelp("Json Security Service")]
    public class JsonSecurityService : JsonRpcService
    {
        [JsonRpcMethod("RegisterUser")]
        [JsonRpcHelp("Register User and Returns Json[status, message, error]")]
        public JsonObject RegisterUser(RegisterInfo registerInfo) //string registerId, string registerPwd, string registerName, string inviteCode
        {
            JsonObject retMessage = new JsonObject();
            string message = "";

            try
            {
                string registerId = DecodeUrl(registerInfo.RegisterId);
                string registerPwd = DecodeUrl(registerInfo.RegisterPwd);
                //string registerName = DecodeUrl(registerInfo.RegisterName);
                //string inviteCode = DecodeUrl(registerInfo.InviteCode);
                bool checkRemote = registerInfo.CheckRemote;

                bool ret = BaseSecurity.HaveUser(registerId, out message);
                if (ret == false)
                {
                    if (BaseSecurity.IsValidEmail(registerId) == true)
                    {
                        if (BaseSecurity.IsValidUserId(registerId) == true)
                        {
                            if (BaseSecurity.IsSystemUser(registerId) == false)
                            {
                                bool remoteRegistrationValid = true;
                                if (checkRemote == true)
                                {
                                    //JsonRemoteService jsonRemoteService = new JsonRemoteService();
                                    //retMessage = jsonRemoteService.RemoteRegisterUser(registerInfo, out remoteRegistrationValid);
                                }

                                if (remoteRegistrationValid == true)
                                {
                                    ret = BaseSecurity.CreateUser(registerId, registerPwd, registerId, true, false, out message);
                                    if (ret == true)
                                    {
                                        if (BaseSecurity.RoleExists(BaseSecurity.GuestRole) == false)
                                        {
                                            BaseSecurity.CreateRole(BaseSecurity.GuestRole);
                                        }
                                        ret = BaseSecurity.AssignRole(registerId, BaseSecurity.GuestRole);
                                        if (ret == true)
                                        {
                                            message = "Successfully Registered User";
                                            retMessage.Add("status", true);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                retMessage.Add("error", "System User [" + registerId + "] cannot be Registered. Please enter valid user Id");
                            }
                        }
                        else
                        {
                            retMessage.Add("error", "Please enter valid user Id");
                        }
                    }
                    else
                    {
                        retMessage.Add("error", "Please enter valid email");
                    }
                }
                else
                {
                    retMessage.Add("error", "User [" + registerId + "] allready Registered");
                }
            }
            catch (Exception ex)
            {
                retMessage.Add("error", ex.Message);
            }

            if (message.Trim().Length > 0) retMessage.Add("message", message);
            return retMessage;
        }

        [JsonRpcMethod("IsRegistered")]
        [JsonRpcHelp("Check if User has Registered and Returns Json[status, message, error]")]
        public JsonObject IsRegistered()
        {
            JsonObject retMessage = new JsonObject();
            string message = "";

            try
            {
                bool ret = BaseSecurity.HaveUser(false);
                if (ret == true)
                {
                    message = "Have Registered User";
                    retMessage.Add("status", true);
                }
                else
                {
                    retMessage.Add("error", "Registered User was not found");
                }
            }
            catch (Exception ex)
            {
                retMessage.Add("error", ex.Message);
            }

            if (message.Trim().Length > 0) retMessage.Add("message", message);
            return retMessage;
        }

        [JsonRpcMethod("LoginUser")]
        [JsonRpcHelp("Login User and Returns Json[status, message, error]")]
        public JsonObject LoginUser(string userEmail, string userPassword, bool rememberMe, bool allowUnApproved)
        {
            JsonObject retMessage = new JsonObject();
            string message = "";

            try
            {
                userEmail = DecodeUrl(userEmail);
                userPassword = DecodeUrl(userPassword);

                bool ret = BaseSecurity.HaveUser(userEmail, out message);
                if (ret == true)
                {
                    if (BaseSecurity.IsValidEmail(userEmail) == true)
                    {
                        if (BaseSecurity.IsValidUserId(userEmail) == true)
                        {
                            //string cookieValue = "";
                            ret = BaseSecurity.Authenticate(userEmail, userPassword, rememberMe, allowUnApproved);
                            if (ret == true)
                            {
                                message = "Successfully Logged In User";
                                retMessage.Add("status", true);
                            }
                        }
                        else
                        {
                            retMessage.Add("error", "Please enter valid user Id");
                        }
                    }
                    else
                    {
                        retMessage.Add("error", "Please enter valid email");
                    }
                }
                else
                {
                    retMessage.Add("error", "User [" + userEmail + "] has not been Registered");
                }
            }
            catch (Exception ex)
            {
                retMessage.Add("error", ex.Message);
            }

            if (message.Trim().Length > 0) retMessage.Add("message", message);
            return retMessage;
        }

        [JsonRpcMethod("IsLoggedIn")]
        [JsonRpcHelp("Check if User has Logged In and Returns Json[status, message, error]")]
        public JsonObject IsLoggedIn()
        {
            JsonObject retMessage = new JsonObject();
            string message = "";

            try
            {
                bool ret = BaseSecurity.HaveUser(false);
                if (ret == true)
                {
                    bool isDefault = false;
                    string loginUserId = BaseSecurity.GetLoginUserID(out isDefault);
                    if ((string.IsNullOrEmpty(loginUserId) == false) && (isDefault == false))
                    {
                        message = "User is Logged In";
                        retMessage.Add("status", true);
                    }
                    else
                    {
                        retMessage.Add("error", "User has not Logged In");
                    }
                }
                else
                {
                    retMessage.Add("error", "Registered User was not found");
                }
            }
            catch (Exception ex)
            {
                retMessage.Add("error", ex.Message);
            }

            if (message.Trim().Length > 0) retMessage.Add("message", message);
            return retMessage;
        }

        [JsonRpcMethod("LogoutUser")]
        [JsonRpcHelp("Logout the User if Logged In and Returns Json[status, message, error]")]
        public JsonObject LogoutUser()
        {
            JsonObject retMessage = new JsonObject();
            string message = "";

            try
            {
                bool ret = BaseSecurity.HaveUser(false);
                if (ret == true)
                {
                    bool isDefault = false;
                    string loginUserId = BaseSecurity.GetLoginUserID(out isDefault);
                    if ((string.IsNullOrEmpty(loginUserId) == false) && (isDefault == false))
                    {
                        BaseSecurity.SignOut();
                        message = "User has been successfully Logged Out";
                        retMessage.Add("status", true);
                    }
                    else
                    {
                        retMessage.Add("error", "User has not Logged In");
                    }
                }
                else
                {
                    retMessage.Add("error", "Registered User was not found");
                }
            }
            catch (Exception ex)
            {
                retMessage.Add("error", ex.Message);
            }

            if (message.Trim().Length > 0) retMessage.Add("message", message);
            return retMessage;
        }

        //[JsonRpcMethod("Encrypt")]
        //[JsonRpcHelp("Encrypt using the Specified Key and Returns Json[data, error]")]
        //public JsonObject Encrypt(string textToEncrypt, string encryptKey)
        //{
        //    JsonObject retMessage = new JsonObject();
        //    string data = "";

        //    try
        //    {

        //        byte[] dataToEncrypt = Encoding.UTF8.GetBytes(textToEncrypt);
        //        data = CryptoManager.Encrypt(dataToEncrypt, encryptKey);
        //    }
        //    catch (Exception ex)
        //    {
        //        retMessage.Add("error", ex.Message);
        //    }

        //    if (data.Trim().Length > 0) retMessage.Add("data", data);
        //    return retMessage;
        //}

        //[JsonRpcMethod("Decrypt")]
        //[JsonRpcHelp("Dencrypt using the Specified Key and Returns Json[data, error]")]
        //public JsonObject Decrypt(string textToDecrypt, string decryptKey)
        //{
        //    JsonObject retMessage = new JsonObject();
        //    string data = "";

        //    try
        //    {
        //        byte[] dataToDecrypt = Encoding.UTF8.GetBytes(textToDecrypt);
        //        data = CryptoManager.Decrypt(dataToDecrypt, decryptKey);
        //    }
        //    catch (Exception ex)
        //    {
        //        retMessage.Add("error", ex.Message);
        //    }

        //    if (data.Trim().Length > 0) retMessage.Add("data", data);
        //    return retMessage;
        //}

        //[JsonRpcMethod("EncryptUsingFile")]
        //[JsonRpcHelp("Encrypt Using File using the Specified Byte Offset and Byte Length and Returns Json[data, error]")]
        //public JsonObject EncryptUsingFile(string textToEncrypt, string keyFilePath, int byteOffset, int byteLength)
        //{
        //    JsonObject retMessage = new JsonObject();
        //    string data = "";

        //    try
        //    {
        //        if (IOManager.CachedFileExists(keyFilePath, false, false) == true)
        //        {
        //            byte[] encryptKeyData = new byte[byteLength];
        //            byte[] fileData = IOManager.ReadBytes(keyFilePath);
        //            int totalLength = byteOffset + byteLength;
        //            if (fileData.Length > totalLength)
        //            {
        //                Array.Copy(fileData, byteOffset, encryptKeyData, 0, byteLength);
        //                string encryptKey = Encoding.UTF8.GetString(encryptKeyData);
        //                byte[] dataToEncrypt = Encoding.UTF8.GetBytes(textToEncrypt);
        //                data = CryptoManager.Encrypt(dataToEncrypt, encryptKey);
        //            }
        //            else
        //            {
        //                retMessage.Add("error", "Invalid ByteOffset/ByteLength[" + totalLength + "] for the Key File Size [" + fileData.Length + "]");
        //            }
        //        }
        //        else
        //        {
        //            retMessage.Add("error", "Key File Not found in Path [" + keyFilePath + "]");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        retMessage.Add("error", ex.Message);
        //    }

        //    if (data.Trim().Length > 0) retMessage.Add("data", data);
        //    return retMessage;
        //}

        //[JsonRpcMethod("Decrypt")]
        //[JsonRpcHelp("Dencrypt using the Specified Key and Returns Json[data, error]")]
        //public JsonObject DecryptUsingFile(string textToDecrypt, string keyFilePath, int byteOffset, int byteLength)
        //{
        //    JsonObject retMessage = new JsonObject();
        //    string data = "";

        //    try
        //    {
        //        if (IOManager.CachedFileExists(keyFilePath, false, false) == true)
        //        {
        //            byte[] decryptKeyData = new byte[byteLength];
        //            byte[] fileData = IOManager.ReadBytes(keyFilePath);
        //            int totalLength = byteOffset + byteLength;
        //            if (fileData.Length > totalLength)
        //            {
        //                Array.Copy(fileData, byteOffset, decryptKeyData, 0, byteLength);
        //                string decryptKey = Encoding.UTF8.GetString(decryptKeyData);
        //                byte[] dataToDecrypt = Encoding.UTF8.GetBytes(textToDecrypt);
        //                data = CryptoManager.Decrypt(dataToDecrypt, decryptKey);
        //            }
        //            else
        //            {
        //                retMessage.Add("error", "Invalid ByteOffset/ByteLength[" + totalLength + "] for the Key File Size [" + fileData.Length + "]");
        //            }
        //        }
        //        else
        //        {
        //            retMessage.Add("error", "Key File Not found in Path [" + keyFilePath + "]");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        retMessage.Add("error", ex.Message);
        //    }

        //    if (data.Trim().Length > 0) retMessage.Add("data", data);
        //    return retMessage;
        //}

    }
}
