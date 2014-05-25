using System;
using System.Globalization;
using System.Collections.Generic;

using PetaPoco;
using Arshu.Web.Common;
using Arshu.Web.Json;
using Arshu.Web.Basic.Action;
using Arshu.Web.Security;

using App.Secure.Entity;

namespace App.Secure.Data
{
    public partial class DataSource
    {
        #region User Hash

        public static string GetUserHash(string userId, string userSalt)
        {
            int hashIteration = 100;
            if (string.IsNullOrEmpty(userSalt) == true)
            {
                SetupData setupData = SetupData.GetSetupData();
                if (setupData != null)
                {
                    userSalt = setupData.GetSecureSalt();
                    hashIteration = setupData.SecureIterations;
                    if (hashIteration < 100) hashIteration = 100;
                }
            }
            string userHash = BaseSecurity.GetPBKDF2Hash(userId, userSalt, hashIteration);
            return userHash;
        }

        #endregion

		#region Get User Info

        public const string DefaultUserAlias = "System";
        public static SYD_User GetDefaultUser()
        {
            string message = "";
            SYD_User sydUser = GetSydUserByAlias(DefaultUserAlias);

            if (sydUser == null)
            {
                string userHash = GetUserHash(DefaultUserAlias, "");

                sydUser = new SYD_User();
                sydUser.UserAlias = DefaultUserAlias;
                sydUser.UserHash = userHash;
                sydUser.IsAdmin = false;

                long id = DataSource.InsertSydUser(out message, sydUser);
                if (id > 0)
                {
                    sydUser = GetSydUserByAlias(DefaultUserAlias);
                }
            }

            return sydUser;
        }

        public static UserInfo GetUserInfo()
        {
            //Get the Default Anonymous User
            SYD_User sydUser = GetDefaultUser();

            bool isDefault = false;
            string userLoginName = BaseSecurity.GetLoginUserID(out isDefault);
            if (string.IsNullOrEmpty(userLoginName) == false)
            {
                SYD_User loginSydUser = GetSydUserByAlias(userLoginName);
                if (loginSydUser != null)
                {
                    sydUser = loginSydUser;
                }
            }

            UserInfo userInfo = null;
            if (sydUser != null)
            {
                userInfo = new UserInfo();
                userInfo.Id = sydUser.UserID;
                userInfo.Guid = sydUser.UserGUID;
                userInfo.Alias = sydUser.UserAlias;
                userInfo.Hash = sydUser.UserHash;
            }

            return userInfo;
        }

        #endregion

        #region SYD_User

        public static SYD_User GetSydUserByAlias(string userAlias)
        {
            SYD_User sydUser = null;

            string message = "";
            sydUser = DataSource.GetSydUser(out message, 0, "Where UserAlias =@0", userAlias); ;

            return sydUser;
        }

        #endregion      
    }
}
