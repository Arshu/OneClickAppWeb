using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Arshu.Web.Basic.Log;
using Arshu.Web.Common;
using Arshu.Web.Security;
using Arshu.Web.Common.SMTP;

using App.Secure.Entity;
using App.Secure.Data;

namespace App.Secure
{
    public static class DataSecure
    {        
        #region Init User

        public static bool InitUser(out string retMessage)
        {
            bool ret = false;
            string message = "";
            string userAlias = "Admin";
            string userId = "Admin";
            string userPwd = "Admin";

            //Only for empty Security Store, the User is auto created 
            //from first login details
            string userEmail = userId;
            if (userEmail.Contains("@") == false) userEmail = userEmail + "@arshu.com";
            
            //User exists in Db but not in Security Store
            if (BaseSecurity.HaveUser(false) == false) 
            {
                ret = BaseSecurity.CreateUser(userId, userPwd, userEmail, true, false, out message);
                if (ret == true)
                {
                    BaseSecurity.CreateRole(BaseSecurity.AdminRole);
                    BaseSecurity.CreateRole(BaseSecurity.AuthorRole);
                    BaseSecurity.CreateRole(BaseSecurity.GuestRole);

                    BaseSecurity.AssignRole(userId, BaseSecurity.AdminRole);

                    //Check if User exist in db
                    string userHash = DataSource.GetUserHash(userId, userPwd);
                    SYD_User sydUser = GetSydUser(userHash);
                    if (sydUser == null)
                    {
                        sydUser = InsertUser(userAlias, userHash, true, out message);
                    }
                }
            }
            //User does not exist in db
            else if (HaveUser(out message) == false)
            {
                if (BaseSecurity.Authenticate(userId, userPwd, false, true) == true)
                {
                    string userHash = DataSource.GetUserHash(userId, userPwd);
                    SYD_User sydUser = InsertUser(userAlias, userHash, true, out message);
                    if (sydUser != null)
                    {
                        //User does not exist in Security Store
                        if (BaseSecurity.HaveUser(userId, out message) == false)
                        {
                            //Create if user not exist in db and in security store
                            ret = BaseSecurity.CreateUser(userId, userPwd, userEmail, true, false, out message);
                            if (ret == true)
                            {
                                BaseSecurity.CreateRole(BaseSecurity.AdminRole);
                                BaseSecurity.CreateRole(BaseSecurity.AuthorRole);
                                BaseSecurity.CreateRole(BaseSecurity.GuestRole);

                                BaseSecurity.AssignRole(userId, BaseSecurity.AdminRole);
                            }
                        }
                        else
                        {
                            ret = true;
                        }
                    }
                }
                else
                {
                    message = "Invalid User id and Password";
                }
            }
            else
            {
                ret = true;
            }

            retMessage = message;
            return ret;
        }

        #endregion

        #region IsAuthenticated

        public static bool IsAuthenticated(out string retMessage)
        {
            bool ret = false;
            string message = "";

            if (BaseSecurity.IsAuthenticated() == true)
            {
                message = "User is logged in";
                ret = true;
            }
            else
            {
                message = "User is not logged in";
            }

            retMessage = message;
            return ret;
        }

        #endregion

        #region LogOff User

        public static bool LogOffUser(out string retMessage)
        {
            bool ret = false;
            string message = "";

            if (BaseSecurity.IsAuthenticated() == true)
            {
                BaseSecurity.SignOut();
                ret = true;
            }
            else
            {
                message = "User is not logged in";
            }

            retMessage = message;
            return ret;
        }

        #endregion

        #region Login User

        public static bool LoginUser(string userId, string userPwd, bool rememberMe, out string retMessage, out string retWarnMessage)
        {
            bool ret = false;
            string message = "";

            string warnMessage = "";
            ret = InitUser(out warnMessage);
            if (ret == true)
            {
                if (BaseSecurity.IsAuthenticated() == true) BaseSecurity.SignOut();
                ret = BaseSecurity.Authenticate(userId, userPwd, rememberMe, false);
            }

            retMessage = message;
            retWarnMessage = warnMessage;
            return ret;
        }

        #endregion

        #region Register User

        private static Regex _validNameRegEx = new Regex(@"^[a-zA-Z][a-zA-Z0-9\._\-]{0,22}?[a-zA-Z0-9]{0,2}$", RegexOptions.Compiled);
        public static bool RegisterUser(string userId, string userEmail, string userPwd, out string retMessage)
        {
            bool ret = false;
            string message = "";

            bool validationFail = false;

            bool haveUser = BaseSecurity.HaveUser(userId, out message);
            if (haveUser == true)
            {
                message = "User Id is already registered [" + message + "]";
                validationFail = true;
            }

            if (validationFail == false)
            {
                if (_validNameRegEx.IsMatch(userId) == false)
                {
                    message = "User id [" + userEmail + "] is not valid.";
                    validationFail = true;
                }
            }

            if (validationFail == false)
            {
                bool emailValid = SmtpDirect.IsValidEmail(userEmail);
                if (emailValid == false)
                {
                    message = "User Email [" + userEmail + "] is not valid ";
                    validationFail = true;
                }
            }

            if (validationFail == false)
            {
                bool haveUserByEmail = BaseSecurity.HaveUserByEmail(userEmail, out message);
                if (haveUserByEmail == true)
                {
                    message = "User Email is already registered [" + message + "]";
                    validationFail = true;
                }
            }

            if (validationFail == false)
            {
                SYD_User existingUser = DataSource.GetSydUserByAlias(userId);
                if (existingUser != null)
                {
                    message = "User Alias [" + userId + "] is already registered";
                    validationFail = true;
                }
            }

            string userHash = DataSource.GetUserHash(userId, userPwd);
            if (validationFail == false)
            {
                SYD_User existingUser = DataSecure.GetSydUser(userHash);
                if (existingUser != null)
                {
                    message = "User [" + userId + "] is already registered";
                    validationFail = true;
                }
            }

            if (validationFail == false)
            {
                bool userCreated = BaseSecurity.CreateUser(userId, userPwd, userEmail, true, false, out message);
                if (userCreated == true)
                {
                    BaseSecurity.AssignRole(userId, BaseSecurity.GuestRole);

                    SYD_User sydUser = new SYD_User();
                    sydUser.UserAlias = userId;
                    sydUser.UserHash = userHash;
                    sydUser.IsAdmin = false;
                    long userNo = DataSource.InsertSydUser(out message, sydUser);
                    if (userNo != 0)
                    {
                        ret = true;
                    }
                    else
                    {
                        message = "Error in Inserting User Details into the Database [" + message + "]";
                    }
                }
                else
                {
                    message = "Error in Creating User in the Security SubSystem [" + message + "]";
                }
            }

            retMessage = message;
            return ret;
        }

        #endregion

        #region Get Login User

        public static string GetLoginUser()
        {
            string loginUserId = "";
            if (BaseSecurity.IsAuthenticated() == true)
            {
                bool isDefault = false;
                loginUserId = BaseSecurity.GetLoginUserID(out isDefault);
            }
            return loginUserId;
        }

        #endregion

        #region Change Password

        public static bool ChangePassword(string userId, string oldPassword, string newPassword, out string retMessage)
        {
            bool ret = false;
            string message = "";
            if (BaseSecurity.IsAuthenticated() == true)
            {
                if (BaseSecurity.ValidateUser(userId, oldPassword) == true)
                {
                    ret = BaseSecurity.ChangePassword(userId, oldPassword, newPassword, out message);
                }
                else
                {
                    message = "Invalid Old Password";
                }
            }
            retMessage = message;
            return ret;
        }

        #endregion

        #region Security Store

        #region Get Roles

        public static string[] GetRoles()
        {
            string[] roleList = BaseSecurity.GetAllRoleList(30, false);

            return roleList;
        }

        #endregion

        #region Get Users in Roles

        public static string[] GetUsersInRole(string roleName)
        {
            string[] userList = BaseSecurity.GetUsersInRole(roleName);

            return userList;
        }

        #endregion

        #region Get Users

        public static string[] GetUsers(string userPattern)
        {
            string[] userList = BaseSecurity.GetAllUserList(userPattern, 30, false);
            return userList;
        }

        #endregion

        #region Add User To Role

        public static bool AddUserToRole(string userId, string roleName)
        {
            bool ret = false;

            ret = BaseSecurity.AssignRole(userId, roleName);

            return ret;
        }

        #endregion

        #region Remove User From Role

        public static bool RemoveUserFromRole(string userId, string roleName)
        {
            bool ret = false;

            ret = BaseSecurity.RemoveRole(userId, roleName);

            return ret;
        }

        #endregion

        #endregion

        #region SYD_User

        #region Get

        public static SYD_User GetSydUser(long userId)
        {
            SYD_User sydUser = null;

            string message = "";
            sydUser = DataSource.GetSydUser(out message, userId, "", ""); ;

            return sydUser;
        }

        public static SYD_User GetSydUser(string userHash)
        {
            SYD_User sydUser = null;

            string message = "";
            sydUser = DataSource.GetSydUser(out message, 0, "Where UserHash =@0", userHash); ;

            return sydUser;
        }

        public static SYD_User GetSydUserByAlias(string userAlias)
        {
            SYD_User sydUser = null;

            sydUser = DataSource.GetSydUserByAlias(userAlias); ;

            return sydUser;
        }

        public static bool HaveUser(out string retMessage)
        {
            bool ret = false;
            string message = "";

            long totalPage = 0;
            long totalItems = 0;
            List<SYD_User> sydUserList = DataSource.GetPagedSydUser(out message, 1, 1, out totalPage, out totalItems, "", "");
            if (sydUserList.Count > 0)
            {
                ret = true;
            }

            retMessage = message;
            return ret;
        }

        #endregion

        #region Save

        public static SYD_User InsertUser(string userAlias, string userHash, bool isAdmin, out string retMessage)
        {
            string message = "";
            SYD_User sydUser = null;

            try
            {
                bool validationFail = false;

                if (validationFail == false)
                {
                    if (string.IsNullOrEmpty(userAlias) == true)
                    {
                        message = "User Alias is Empty";
                        validationFail = true;
                    }
                    else if (string.IsNullOrEmpty(userHash) == true)
                    {
                        message = "User Hash is Empty";
                        validationFail = true;
                    }
                }

                if (validationFail == false)
                {
                    SYD_User sydUserExistingByAlias = GetSydUserByAlias(userAlias);
                    if (sydUserExistingByAlias != null)
                    {
                        message = "User having the User Alias [" + userAlias + "] allready exists";
                        validationFail = true;
                    }
                }

                if (validationFail == false)
                {
                    SYD_User sydUserExistingByHash = GetSydUser(userHash);
                    if (sydUserExistingByHash != null)
                    {
                        message = "User having the User Hash [" + userHash + "] allready exists";
                        validationFail = true;
                    }
                }

                if (validationFail == false)
                {
                    sydUser = new SYD_User();
                    sydUser.UserAlias = userAlias;
                    sydUser.UserHash = userAlias;
                    sydUser.IsAdmin = isAdmin;

                    long userId = DataSource.InsertSydUser(out message, sydUser);
                    if (userId == 0)
                    {
                        sydUser = null;
                    }
                }
            }
            catch (Exception ex)
            {
                sydUser = null;
                message = "DBError:" + ex.Message;
                LogManager.Log(LogType.Error, "DataSecure-InsertSydUser", "Error" + message);
            }

            retMessage = message;
            return sydUser;
        }

        #endregion

        #endregion
    }
}