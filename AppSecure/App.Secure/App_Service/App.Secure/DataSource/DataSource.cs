
using System;
using System.Threading;
using System.Collections.Generic;

using Arshu.Web.Basic.Log;
using Arshu.Web.IO;
using Arshu.Web.Common;
using Arshu.Web.Json;

using Arshu.Data.Common;
using PetaPoco;

using App.Secure.Entity;

namespace App.Secure.Data
{
    public partial class DataSource 
    {
			
        #region Static Constant

		private const string DataDirectoryName = "DataDirectory";

        public static bool CheckSetupDb = true;
		public static bool UseSharedConnection = false;
        public static string DbName = "secure.v1.db";
        public static string ConnectionString = "AutoFill";
        public static string ProviderNameFactory = "";

        #endregion

        #region Database Related

        public static void BeginTransaction(out string retMessage)
        {
			string message = "";
            Database db = HaveDb(out message);
            if (db != null)
			{
				db.BeginTransaction();
			}
			retMessage = message;
        }

        public static void CompleteTransaction(out string retMessage)
        {
		    string message ="";
            Database db = HaveDb(out message);
            if (db != null)
			{
				db.CompleteTransaction();
			}
			retMessage = message;
        }

        public static void AbortTransaction(out string retMessage)
        {
			string message = "";
            Database db = HaveDb(out message);
            if (db != null)
			{
				db.AbortTransaction();
			}
			retMessage = message;
        }

		private static Dictionary<string, string> ParseConnectionString(string connectionString)
        {
            Dictionary<string, string> connectionStringParamList = new Dictionary<string, string>();

            string[] connectionParams = connectionString.Split(';');
            foreach (string itemParam in connectionParams)
            {
                string[] paramKeyValue = itemParam.Split('=');
                if (paramKeyValue.Length == 2)
                {
                    connectionStringParamList.Add(paramKeyValue[0].Trim(), paramKeyValue[1].Trim());
                }
                //else if (paramKeyValue.Length == 1)
                //{
                //    connectionStringParamList.Add(paramKeyValue[0].Trim(), "");
                //}
            }

            return connectionStringParamList;
        }

		private static string StripDBName(string dbName)
        {
            //Remove Extension
            int idxOfExtension = dbName.ToLower().IndexOf(IOManager.GetExtension(dbName));
            if (idxOfExtension > -1)
            {
                dbName = dbName.Substring(0, idxOfExtension).Trim();
            }           
            //Remove Version
            int idxOfV = dbName.ToLower().IndexOf(".v");
            if (idxOfV > -1)
            {
                dbName = dbName.Substring(0, idxOfV).Trim();
            }
            return dbName;
        }

        //Important : Ensure the DBName without the Extension/Version Number is contained in the App Name
        private static bool InitDb(out string retMessage)
        {
            bool ret = false;
            string message = "";

            if (CheckSetupDb==true)
            {
			    string rootPath = IOManager.RootDirectory;
                string dataPath = IOManager.Combine(rootPath, "App_Data");
                string[] dataFileList = IOManager.GetAllFiles(dataPath, "*.*");                
                string dbPartialName = StripDBName(DbName);

                List<SetupData> setupDataList = SetupData.GetSetupDataList();
                if (setupDataList.Count > 0)
                {
                    foreach (SetupData setupData in setupDataList)
                    {
                        if (setupData != null)
                        {
							#region Get Configured DB Path

                            string dbPath = "";

                            foreach (string itemDataFile in dataFileList)
                            {               
								string itemDataName = IOManager.GetFileName(itemDataFile).ToUpper();                 
                                string dbFileExtension = IOManager.GetExtension(itemDataFile).ToUpper() ;

                                if ((dbFileExtension == ".MDF") || (dbFileExtension == ".DB") || (dbFileExtension == ".SQLITE"))
                                {
                                    #region Get the DB Path

                                    //DB Name is Contained in App Name and file name
                                    if ((setupData.AppName.ToUpper().Contains(dbPartialName.ToUpper()) == true)
                                        && (itemDataName.Contains(dbPartialName.ToUpper()) == true))
                                    {
                                        if (dbPath.Trim().Length == 0)
                                        {
                                            dbPath = itemDataFile;
                                        }
                                        else if (dbFileExtension == "MDF")
                                        {
                                            dbPath = itemDataFile;
                                        }
										break;
                                    }

                                    #endregion
                                }
                            }
                            
                            #endregion

                            if ((string.IsNullOrEmpty(dbPath) == false) || (setupData.AppName.ToUpper().Contains(dbPartialName.ToUpper())))
                            {
                                #region Set the Configured Value from Setup Data

                                if (string.IsNullOrEmpty(setupData.DbProviderNameFactory) == false)
                                {
                                    ProviderNameFactory = setupData.DbProviderNameFactory;
                                }
                                if (string.IsNullOrEmpty(setupData.DbConnectionString) == false)
                                {
                                    ConnectionString = setupData.DbConnectionString;
                                }

                                #endregion

                                #region Process if Configured Value is Empty

                                if ((string.IsNullOrEmpty(dbPath) == false) && ((ConnectionString.Trim().Length == 0) || (ConnectionString.ToUpper() == "AutoFill".ToUpper())))
                                {                                    
                                    //Set for MSSQL Express Database first
                                    if (dbPath.Trim().ToUpper().EndsWith("MDF") == true)
                                    {
                                        string additionalConfig = "Integrated Security=True;MultipleActiveResultSets=true";
                                        ConnectionString = "Server=.\\SQLExpress;AttachDbFilename=" + dbPath + ";" + additionalConfig;
                                        ProviderNameFactory = DataCommon.MSSQLProviderNameFactory;
                                    }
                                    //Set for SQLite Database
                                    else if ((dbPath.Trim().ToUpper().EndsWith("DB") == true)
                                        || (dbPath.Trim().ToUpper().EndsWith("SQLITE") == true))
                                    {
                                        string additionalConfig = "Version=3;FailIfMissing=True;Pooling=True;"; //Synchronous=Off;journal mode=WAL
                                        ConnectionString = "Data Source=" + dbPath + ";" + additionalConfig;
                                        ProviderNameFactory = DataCommon.SQLiteProviderNameFactory;
                                    }                                                                      
                                }

                                #endregion

                                #region Expand Data Directory

                                //Expand the Data Directory of the Connection String                               
                                if (ConnectionString.ToUpper().Contains(DataDirectoryName.ToUpper()) == true)
                                {
                                    int idxOfDataDirectory = ConnectionString.IndexOf(DataDirectoryName);
                                    if (idxOfDataDirectory > 0)
                                    {
                                        string firstPart = ConnectionString.Substring(0, idxOfDataDirectory).Replace("|", "");
                                        string middlePath = dataPath + IOManager.DirectorySeparator;
                                        string lastPart = ConnectionString.Substring(idxOfDataDirectory + DataDirectoryName.Length).Replace("|", "");

                                        ConnectionString = firstPart + middlePath + lastPart ;
                                    }
                                }

                                #endregion

                                #region Validate File Existence

                                if (ProviderNameFactory.ToUpper() == DataCommon.SQLiteProviderNameFactory.ToUpper())
                                {
                                    string sqliteDbPath = "";
                                    Dictionary<string, string> connectionParams = ParseConnectionString(ConnectionString);
                                    foreach (KeyValuePair<string,string> itemParam in connectionParams)
                                    {
                                        if (itemParam.Key.ToUpper().Contains("Data Source".ToUpper()) == true)
                                        {
                                            sqliteDbPath = itemParam.Value;
                                            break;
                                        }                                       
                                    }

                                    if ((string.IsNullOrEmpty(sqliteDbPath) ==false) && (IOManager.CachedFileExists(sqliteDbPath, true, true) == true))
                                    {
                                        ret = true;
                                    }
                                    else
                                    {
                                        LogManager.Log(LogType.Critical, "DataSource-InitDb", "Sqlite Db Not found in Path [" + sqliteDbPath + "]");
                                    }
                                }
                                else if ((ProviderNameFactory.ToUpper() == DataCommon.MSSQLProviderNameFactory.ToUpper())
                                    && (ConnectionString.ToUpper().Contains("SQLExpress".ToUpper()) == true))
                                {
                                    string attachDbPath = "";
                                    Dictionary<string, string> connectionParams = ParseConnectionString(ConnectionString);
                                    foreach (KeyValuePair<string, string> itemParam in connectionParams)
                                    {
                                        if (itemParam.Key.ToUpper().Contains("AttachDbFilename".ToUpper()) == true)
                                        {
                                            attachDbPath = itemParam.Value;
                                            break;
                                        }
                                    }

                                    if ((string.IsNullOrEmpty(attachDbPath) == false) && (IOManager.CachedFileExists(attachDbPath, true, true) == true))
                                    {
                                        ret = true;
                                    }
                                    else
                                    {
                                        LogManager.Log(LogType.Critical, "DataSource-InitDb", "SQLExpress Db Not found in Path [" + attachDbPath + "]");
                                    }
                                }
                                else if ((string.IsNullOrEmpty(ProviderNameFactory) == false) && (string.IsNullOrEmpty(ConnectionString) == false))
                                {
                                    ret = true;
                                }
                                else
                                {
                                    LogManager.Log(LogType.Critical, "DataSource-InitDb", "Provider Name Factory [" + ProviderNameFactory + "] or Connection String [" + ConnectionString + "] is Empty");
                                }

                                #endregion

                                break;
                            }							
                        }
                    }
                }
				else
				{
					LogManager.Log(LogType.Critical, "Priya.Security.DataSource-InitDb", "Unable to Retrieve Setup Data List");
				}
            }

			retMessage = message;
            return ret;
        }

		/*
			Note: for transactions to work, all operations need to use the same instance of the PetaPoco database object. 
			So you'll probably want to use a per-http request, or per-thread IOC container to serve up a shared instance 
			of this object.

			Using a static database will get errors like "There is already an open DataReader associated with this Command"
			because the same connection is used by different request accessing the same resource.

			Solutions 
				1) Create the connection in a MVC controller base class
				2) Static method creating one connection per request using HttpContext.Items as checking store
				3) Shared Database Object using ThreadStatic assuming no thread switches will happen 
				   between start and end of a transaction.

			Warning: Using ThreadStatic in ASP.NET is not correct because of ASP.NET Thread Affinity. The Threads may be
			switched when ever you execute any Async Operations.

			In ASP.NET your code is run on a WorkerThread from the 25 or so threads in the default ASP.NET worker thread 
			pool and the variable that you think is "personal private to your thread" is personal private...to you and 
			every other request that this worker thread has been with.
		*/
        [ThreadStatic]
        private static Database _db;
        private static Database HaveDb(out string retMessage)
        {
			string message = "";
			if (_db == null)
			{
				if (InitDb(out message) == true)
				{
					_db = DataStore.HaveDb(ConnectionString, ProviderNameFactory, out message);
					if (_db != null) 
					{
                        message = "";

                        _db.OnDBException -= new PetaPoco.DBException(OnDBException);
                        _db.OnDBException += new PetaPoco.DBException(OnDBException);

                        if (ProviderNameFactory == DataCommon.SQLiteProviderNameFactory)
                        {
                            _db.KeepConnectionAlive = true;
                            UseSharedConnection = true;
                            //_db.Execute("PRAGMA journal_mode=WAL;");
                            //_db.Execute("PRAGMA journal_mode=DELETE;");
                        }
                        if (ProviderNameFactory == DataCommon.MySQLProviderNameFactory)
                        {
                            _db.KeepConnectionAlive = false;
                            UseSharedConnection = false;
                            //_db.Execute("set wait_timeout=28800");
                            //_db.Execute("set interactive_timeout=28800");
                            //_db.Execute("set net_write_timeout=999");
                        }
                        if (UseSharedConnection == true)
                        {
                            _db.OpenSharedConnection();
                        }
					}
				}
                else
                {
                    LogManager.Log(LogType.Warn, "Priya.Security.DataSource-HaveDb", "Init DB from Setup Data is false [" + message + "]. Using File System");
                }
			}
			
			retMessage = message;
            return _db;
        }

        private static void OnDBException(Exception ex, string lastCommand, string lastSQL, Database db)
        {
            string message = "";
            if (null != ex.InnerException)
            {
                string stackTrace = ex.InnerException.StackTrace;
                message = "DBError:[" + lastSQL + "][" + lastCommand+ "][" + db.SharedConnectionDepth + "]" + Environment.NewLine + "[" + ex.InnerException.Message + "][" + ex.Message + "]" + Environment.NewLine + "[" + stackTrace + "]";
            }
            else
            {
                string stackTrace = ex.StackTrace;
                message = "DBError:[" + lastSQL + "][" + lastCommand+ "][" + db.SharedConnectionDepth + "]" + Environment.NewLine + "[" + ex.Message + "]" + Environment.NewLine + "[" + stackTrace + "]";
            }
            LogManager.Log(LogType.Error, "App.Secure.DataSource-OnDBException", "Error:" + message);
        }

        #endregion

        #region Utilities
      
        private static Dictionary<string, string> GetWhereList(string whereClause, params object[] whereArgs)
        {
            Dictionary<string, string> whereList = new Dictionary<string, string>();

            try
            {
                string parseMessage = "";
                Tokenizer tokenizer = new Tokenizer(whereClause);
                if (tokenizer.Match("WHERE", out parseMessage) == true)
                {
                    do
                    {
                        if (tokenizer.Token == "AND") tokenizer.Match("AND", out parseMessage);

                        string fieldName = tokenizer.Token;
                        tokenizer.GetToken();
                        if (tokenizer.Match("=", out parseMessage) == true)
                        {
                            string fieldValue = tokenizer.Token;
                            for (int i = 0; i < whereArgs.Length; i++)
			                {
                                fieldValue = fieldValue.Replace("@" + i, whereArgs[i].ToString());
			                }
                            whereList.Add(fieldName, fieldValue);

                            tokenizer.GetToken();
                        }
                        else
                        {
                            break;
                        }
                    }
                    while (tokenizer.Token == "AND");
                }
            }
            catch
            {

            }

            return whereList;
        }

        #endregion


  
        #region SYD_User Query Related

        #region Get

        public static SYD_User GetSydUser(out string retMessage, long userID, string orWhereClause, params object[] orWhereArgs)
        {
            SYD_User sydUser = null;
			string message = "";            
            Database db = HaveDb(out message);
            if (db != null)
            {
                if (orWhereClause.Trim().Length == 0)
                {
                    sydUser = db.SingleOrDefault<SYD_User>("SELECT * FROM SYD_User Where UserID=@0", userID);
                }
                else
                {
                    if ((string.IsNullOrEmpty(orWhereClause) ==false) && (orWhereClause.Trim().ToUpper().StartsWith("Where".ToUpper()) == false)) orWhereClause = " WHERE " + orWhereClause;
                    sydUser = db.FirstOrDefault<SYD_User>("SELECT * FROM SYD_User " + orWhereClause, orWhereArgs);
                }
            }
            else
            {
                Dictionary<Guid, SYD_User> fileSYDUserList = FileSource.LoadSYDUserData();
                foreach (KeyValuePair<Guid, SYD_User> item in fileSYDUserList)
                {
                    bool found = true ;
                    if (orWhereClause.Trim().Length > 0)
                    {
                        Dictionary<string, string> whereList = GetWhereList(orWhereClause, orWhereArgs);
                        foreach (KeyValuePair<string, string> whereCol in whereList)
                        {
                            bool match = false;
                            if (item.Value.HaveColumn(whereCol.Key, whereCol.Value, out match) == true)
                            {
                                if (match == false)
                                {
                                    found = false;
                                    break;
                                }
                            }
                            else
                            {
                                found = false;
                            }
                        }
                        if (found ==true)
                        {
                            sydUser = item.Value;
                            break;
                        }
                    }
                    else
                    {
                        if (item.Value.UserID == userID)
                        {
                            sydUser = item.Value;
                            break;
                        }
                    }                    
                }
            }

			retMessage = message;
            return sydUser;
        }

        public static long GetMaxSydUserId(out string retMessage, string whereClause, params object[] whereArgs)
        {
            long maxSydUserId = 0;

            string message = "";
            Database db = HaveDb(out message);
            if (db != null)
            {
                if ((string.IsNullOrEmpty(whereClause) ==false) && (whereClause.Trim().ToUpper().StartsWith("Where".ToUpper()) == false)) whereClause = " WHERE " + whereClause;
                maxSydUserId = db.Single<long>("SELECT MAX(UserID) AS MAX_ID FROM SYD_User " + whereClause, whereArgs);
            }
            else
            {
                Dictionary<Guid, SYD_User> fileSYDUserList = FileSource.LoadSYDUserData();
                foreach (KeyValuePair<Guid, SYD_User> item in fileSYDUserList)
                {
                    bool found = true ;
                    if (whereClause.Trim().Length > 0)
                    {
                        Dictionary<string, string> whereList = GetWhereList(whereClause, whereArgs);
                        foreach (KeyValuePair<string, string> whereCol in whereList)
                        {
                            bool match = false;
                            if (item.Value.HaveColumn(whereCol.Key, whereCol.Value, out match) == true)
                            {
                                if (match == false)
                                {
                                    found = false;
                                    break;
                                }
                            }else
                            {
                                found = false;
                            }
                        }
                    }
                    
                    if (found ==true)
                    {
                        if (item.Value.UserID >= maxSydUserId)
                        {
                            maxSydUserId = item.Value.UserID;
                        }
                    }
                }
            }

			retMessage = message;
            return maxSydUserId;
        }
          

          
		
		/*
        public static long GetSydUserCount(out string retMessage, string whereClause, params object[] whereArgs)
        {
            long sydUserCount = 0;
            string message = "";
            Database db = HaveDb(out message);
            if (db != null)
            {
                if ((string.IsNullOrEmpty(whereClause) ==false) && (whereClause.Trim().ToUpper().StartsWith("Where".ToUpper()) == false)) whereClause = " WHERE " + whereClause;
                sydUserCount = db.Single<long>("SELECT COUNT(*) AS TOTAL_COUNT FROM SYD_User " + whereClause, whereArgs);
            }
            else
            {
                Dictionary<Guid, SYD_User> fileSYDUserList = FileSource.LoadSYDUserData();
                if (whereClause.Trim().Length > 0)
                {
                    Dictionary<string, string> whereList = GetWhereList(whereClause, whereArgs);
                    Dictionary<Guid, SYD_User> filteredSYDUserList = new Dictionary<Guid, SYD_User>();
                    foreach (KeyValuePair<Guid, SYD_User> item in fileSYDUserList)
                    {
                        bool add =true ;
                        foreach (KeyValuePair<string, string> whereCol in whereList)
                        {
                            bool match = false;
                            if (item.Value.HaveColumn(whereCol.Key, whereCol.Value, out match) == true)
                            {
                                if (match == false)
                                {
                                    add = false;
                                    break;
                                }
                            }else
                            {
                                add = false;
                            }
                        }
                        if (add == true) filteredSYDUserList.Add(item.Key, item.Value);
                    }
                    sydUserCount = filteredSYDUserList.Count;
                }
                else
                {
                    sydUserCount = fileSYDUserList.Count;
                }
            }
			retMessage = message;
            return sydUserCount;
        }
		*/

        #endregion

        #region Get All

		/*
        public static List<SYD_User> GetAllSydUser(out string retMessage, string orderByClause, string whereClause, params object[] whereArgs)
        {
            List<SYD_User> allSydUserList = new List<SYD_User>();
			string message = "";
            Database db = HaveDb(out message);
            if (db != null)
            {
                                
                if ((string.IsNullOrEmpty(whereClause) ==false) && (whereClause.Trim().ToUpper().StartsWith("Where".ToUpper()) == false)) whereClause = " WHERE " + whereClause;
                allSydUserList = db.Fetch<SYD_User>("SELECT * FROM SYD_User " + whereClause + " " + orderByClause, whereArgs);
            }
            else
            {
                Dictionary<Guid, SYD_User> fileSydUserList = FileSource.LoadSYDUserData();
                foreach (KeyValuePair<Guid, SYD_User> item in fileSydUserList)
                {
                    if (whereClause.Trim().Length > 0)
                    {
                        Dictionary<string, string> whereList = GetWhereList(whereClause, whereArgs);
                        bool add =true ;
                        foreach (KeyValuePair<string, string> whereCol in whereList)
                        {
                            bool match = false;
                            if (item.Value.HaveColumn(whereCol.Key, whereCol.Value, out match) == true)
                            {
                                if (match == false)
                                {
                                    add = false;
                                    break;
                                }
                            }else
                            {
                                add = false;
                            }
                        }
                        if (add == true) allSydUserList.Add(item.Value);
                    }
                    else
                    {
                        allSydUserList.Add(item.Value);
                    }
                }
            }

			retMessage = message;
            return allSydUserList;
        }
		*/

        #endregion

        #region Get Paged

        public static List<SYD_User> GetPagedSydUser(out string retMessage, long pageNo, long itemsPerPage, out long retTotalPages, out long retTotalItems, string orderByClause, string whereClause, params object[] whereArgs)
        {
            List<SYD_User> pagedSydUserList = new List<SYD_User>();
            if (pageNo <= 0) pageNo = 1;
            if (itemsPerPage <= 0) itemsPerPage = 1;

            long totalPages = 0;
            long totalItems = 0;

			string message = "";
            Database db = HaveDb(out message);
            if (db != null)
            {
                if ((string.IsNullOrEmpty(whereClause) ==false) && (whereClause.Trim().ToUpper().StartsWith("Where".ToUpper()) == false)) whereClause = " WHERE " + whereClause;
                                
                Page<SYD_User> pagedData = db.Page<SYD_User>(pageNo, itemsPerPage, "SELECT * FROM SYD_User " + whereClause + " " + orderByClause, whereArgs);
                totalPages = pagedData.TotalPages;
                totalItems = pagedData.TotalItems;
                if ((pagedData.Items.Count == 0) && (totalPages == 1))
                {
                    pagedData = db.Page<SYD_User>(1, itemsPerPage, "SELECT * FROM SYD_User " + whereClause + " " + orderByClause, whereArgs);
                    totalPages = pagedData.TotalPages;
                    totalItems = pagedData.TotalItems;
                }
                pagedSydUserList = pagedData.Items;
            }
            else
            {
                Dictionary<Guid, SYD_User> fileSydUserList = FileSource.LoadSYDUserData();
                if (whereClause.Trim().Length > 0)
                {
                    Dictionary<string, string> whereList = GetWhereList(whereClause, whereArgs);
                    Dictionary<Guid, SYD_User> filteredSYDUserList = new Dictionary<Guid, SYD_User>();
                    foreach (KeyValuePair<Guid, SYD_User> item in fileSydUserList)
                    {
                        bool add =true ;
                        foreach (KeyValuePair<string, string> whereCol in whereList)
                        {
                            bool match = false;
                            if (item.Value.HaveColumn(whereCol.Key, whereCol.Value, out match) == true)
                            {
                                if (match == false)
                                {
                                    add = false;
                                    break;
                                }
                            }else
                            {
                                add = false;
                            }
                        }
                        if (add == true) filteredSYDUserList.Add(item.Key, item.Value);
                    }
                    pagedSydUserList = FileSource.GetPagedSYDUser(filteredSYDUserList, pageNo, itemsPerPage, out totalPages, out totalItems);
                }
                else
                {
                    pagedSydUserList = FileSource.GetPagedSYDUser(fileSydUserList, pageNo, itemsPerPage, out totalPages, out totalItems);
                }
            }

            retTotalPages = totalPages;
            retTotalItems = totalItems;
			retMessage = message;
            return pagedSydUserList;
        }

        #endregion

        #region Insert

        public static long InsertSydUser(out string retMessage, SYD_User sydUser)
        {
			string message = "";
            long id = 0;
            if (sydUser.UserGUID != Guid.Empty) throw new Exception("Cannot Set the GUID for a Insert");
            sydUser.UserGUID = Guid.NewGuid();


            Database db = HaveDb(out message);
            if (db != null)
            {
                 db.Insert(sydUser);
                 id = sydUser.UserID;
            }
            else
            {
                Dictionary<Guid, SYD_User> fileSydUserList = FileSource.LoadSYDUserData();
	  			sydUser.UserID = GetMaxSydUserId(out message, "") + 1;
  
                fileSydUserList.Add(sydUser.UserGUID, sydUser);
                FileSource.SaveSYDUserData(fileSydUserList);

                id = sydUser.UserID;
            }

			retMessage = message;
            return id;
        }

        #endregion

        #region Update

        public static int UpdateSydUser(out string retMessage, SYD_User sydUser)
        {
			string message = "";
            int retAffectedRecordCount = 0;
            Database db = HaveDb(out message);
            if (db != null)
            {
                SYD_User sydUserExisting = GetSydUser(out message, sydUser.UserID, "");
                if (sydUserExisting != null)
                {
                      
                    retAffectedRecordCount = db.Update(sydUser);
                }
            }
            else
            {
                Dictionary<Guid, SYD_User> fileSydUserList = FileSource.LoadSYDUserData();                
                if (fileSydUserList.ContainsKey(sydUser.UserGUID) == true)
                {
                    fileSydUserList.Remove(sydUser.UserGUID);
                    fileSydUserList.Add(sydUser.UserGUID, sydUser);
					retAffectedRecordCount++;
                    FileSource.SaveSYDUserData(fileSydUserList);
                }
            }

			retMessage = message;
            return retAffectedRecordCount;
        }

        #endregion

        #region Delete

        public static int DeleteSydUser(out string retMessage, long sydUserId)
        {
			int retAffectedRecordCount = 0;
			string message = "";

            Database db = HaveDb(out message);
            if (db != null)
            {
                retAffectedRecordCount = db.Delete<SYD_User>(sydUserId);
            }
            else
            {
                Dictionary<Guid, SYD_User> fileSydUserList = FileSource.LoadSYDUserData();
                Guid sydUserGuidToRemove = Guid.Empty;
                foreach (KeyValuePair<Guid, SYD_User> item in fileSydUserList)
                {
                    if (item.Value.UserID == sydUserId)
                    {
                        sydUserGuidToRemove = item.Key;
                        break;
                    }
                }
                if (sydUserGuidToRemove != Guid.Empty)
                {
                    fileSydUserList.Remove(sydUserGuidToRemove);
					retAffectedRecordCount++;
                    FileSource.SaveSYDUserData(fileSydUserList);
                }
            }

			retMessage = message;
			return retAffectedRecordCount;
        }

        #endregion

        #endregion
  
        #region SYS_Version Query Related

        #region Get

        public static SYS_Version GetSysVersion(out string retMessage, double versionNo, string orWhereClause, params object[] orWhereArgs)
        {
            SYS_Version sysVersion = null;
			string message = "";            
            Database db = HaveDb(out message);
            if (db != null)
            {
                if (orWhereClause.Trim().Length == 0)
                {
                    sysVersion = db.SingleOrDefault<SYS_Version>("SELECT * FROM SYS_Version Where VersionNo=@0", versionNo);
                }
                else
                {
                    if ((string.IsNullOrEmpty(orWhereClause) ==false) && (orWhereClause.Trim().ToUpper().StartsWith("Where".ToUpper()) == false)) orWhereClause = " WHERE " + orWhereClause;
                    sysVersion = db.FirstOrDefault<SYS_Version>("SELECT * FROM SYS_Version " + orWhereClause, orWhereArgs);
                }
            }
            else
            {
                Dictionary<Guid, SYS_Version> fileSYSVersionList = FileSource.LoadSYSVersionData();
                foreach (KeyValuePair<Guid, SYS_Version> item in fileSYSVersionList)
                {
                    bool found = true ;
                    if (orWhereClause.Trim().Length > 0)
                    {
                        Dictionary<string, string> whereList = GetWhereList(orWhereClause, orWhereArgs);
                        foreach (KeyValuePair<string, string> whereCol in whereList)
                        {
                            bool match = false;
                            if (item.Value.HaveColumn(whereCol.Key, whereCol.Value, out match) == true)
                            {
                                if (match == false)
                                {
                                    found = false;
                                    break;
                                }
                            }
                            else
                            {
                                found = false;
                            }
                        }
                        if (found ==true)
                        {
                            sysVersion = item.Value;
                            break;
                        }
                    }
                    else
                    {
                        if (item.Value.VersionNo == versionNo)
                        {
                            sysVersion = item.Value;
                            break;
                        }
                    }                    
                }
            }

			retMessage = message;
            return sysVersion;
        }

        public static double GetMaxSysVersionId(out string retMessage, string whereClause, params object[] whereArgs)
        {
            double maxSysVersionId = 0;

            string message = "";
            Database db = HaveDb(out message);
            if (db != null)
            {
                if ((string.IsNullOrEmpty(whereClause) ==false) && (whereClause.Trim().ToUpper().StartsWith("Where".ToUpper()) == false)) whereClause = " WHERE " + whereClause;
                maxSysVersionId = db.Single<long>("SELECT MAX(VersionNo) AS MAX_ID FROM SYS_Version " + whereClause, whereArgs);
            }
            else
            {
                Dictionary<Guid, SYS_Version> fileSYSVersionList = FileSource.LoadSYSVersionData();
                foreach (KeyValuePair<Guid, SYS_Version> item in fileSYSVersionList)
                {
                    bool found = true ;
                    if (whereClause.Trim().Length > 0)
                    {
                        Dictionary<string, string> whereList = GetWhereList(whereClause, whereArgs);
                        foreach (KeyValuePair<string, string> whereCol in whereList)
                        {
                            bool match = false;
                            if (item.Value.HaveColumn(whereCol.Key, whereCol.Value, out match) == true)
                            {
                                if (match == false)
                                {
                                    found = false;
                                    break;
                                }
                            }else
                            {
                                found = false;
                            }
                        }
                    }
                    
                    if (found ==true)
                    {
                        if (item.Value.VersionNo >= maxSysVersionId)
                        {
                            maxSysVersionId = item.Value.VersionNo;
                        }
                    }
                }
            }

			retMessage = message;
            return maxSysVersionId;
        }
          

          
		
		/*
        public static long GetSysVersionCount(out string retMessage, string whereClause, params object[] whereArgs)
        {
            long sysVersionCount = 0;
            string message = "";
            Database db = HaveDb(out message);
            if (db != null)
            {
                if ((string.IsNullOrEmpty(whereClause) ==false) && (whereClause.Trim().ToUpper().StartsWith("Where".ToUpper()) == false)) whereClause = " WHERE " + whereClause;
                sysVersionCount = db.Single<long>("SELECT COUNT(*) AS TOTAL_COUNT FROM SYS_Version " + whereClause, whereArgs);
            }
            else
            {
                Dictionary<Guid, SYS_Version> fileSYSVersionList = FileSource.LoadSYSVersionData();
                if (whereClause.Trim().Length > 0)
                {
                    Dictionary<string, string> whereList = GetWhereList(whereClause, whereArgs);
                    Dictionary<Guid, SYS_Version> filteredSYSVersionList = new Dictionary<Guid, SYS_Version>();
                    foreach (KeyValuePair<Guid, SYS_Version> item in fileSYSVersionList)
                    {
                        bool add =true ;
                        foreach (KeyValuePair<string, string> whereCol in whereList)
                        {
                            bool match = false;
                            if (item.Value.HaveColumn(whereCol.Key, whereCol.Value, out match) == true)
                            {
                                if (match == false)
                                {
                                    add = false;
                                    break;
                                }
                            }else
                            {
                                add = false;
                            }
                        }
                        if (add == true) filteredSYSVersionList.Add(item.Key, item.Value);
                    }
                    sysVersionCount = filteredSYSVersionList.Count;
                }
                else
                {
                    sysVersionCount = fileSYSVersionList.Count;
                }
            }
			retMessage = message;
            return sysVersionCount;
        }
		*/

        #endregion

        #region Get All

		/*
        public static List<SYS_Version> GetAllSysVersion(out string retMessage, string orderByClause, string whereClause, params object[] whereArgs)
        {
            List<SYS_Version> allSysVersionList = new List<SYS_Version>();
			string message = "";
            Database db = HaveDb(out message);
            if (db != null)
            {
                                
                if ((string.IsNullOrEmpty(whereClause) ==false) && (whereClause.Trim().ToUpper().StartsWith("Where".ToUpper()) == false)) whereClause = " WHERE " + whereClause;
                allSysVersionList = db.Fetch<SYS_Version>("SELECT * FROM SYS_Version " + whereClause + " " + orderByClause, whereArgs);
            }
            else
            {
                Dictionary<Guid, SYS_Version> fileSysVersionList = FileSource.LoadSYSVersionData();
                foreach (KeyValuePair<Guid, SYS_Version> item in fileSysVersionList)
                {
                    if (whereClause.Trim().Length > 0)
                    {
                        Dictionary<string, string> whereList = GetWhereList(whereClause, whereArgs);
                        bool add =true ;
                        foreach (KeyValuePair<string, string> whereCol in whereList)
                        {
                            bool match = false;
                            if (item.Value.HaveColumn(whereCol.Key, whereCol.Value, out match) == true)
                            {
                                if (match == false)
                                {
                                    add = false;
                                    break;
                                }
                            }else
                            {
                                add = false;
                            }
                        }
                        if (add == true) allSysVersionList.Add(item.Value);
                    }
                    else
                    {
                        allSysVersionList.Add(item.Value);
                    }
                }
            }

			retMessage = message;
            return allSysVersionList;
        }
		*/

        #endregion

        #region Get Paged

        public static List<SYS_Version> GetPagedSysVersion(out string retMessage, long pageNo, long itemsPerPage, out long retTotalPages, out long retTotalItems, string orderByClause, string whereClause, params object[] whereArgs)
        {
            List<SYS_Version> pagedSysVersionList = new List<SYS_Version>();
            if (pageNo <= 0) pageNo = 1;
            if (itemsPerPage <= 0) itemsPerPage = 1;

            long totalPages = 0;
            long totalItems = 0;

			string message = "";
            Database db = HaveDb(out message);
            if (db != null)
            {
                if ((string.IsNullOrEmpty(whereClause) ==false) && (whereClause.Trim().ToUpper().StartsWith("Where".ToUpper()) == false)) whereClause = " WHERE " + whereClause;
                                
                Page<SYS_Version> pagedData = db.Page<SYS_Version>(pageNo, itemsPerPage, "SELECT * FROM SYS_Version " + whereClause + " " + orderByClause, whereArgs);
                totalPages = pagedData.TotalPages;
                totalItems = pagedData.TotalItems;
                if ((pagedData.Items.Count == 0) && (totalPages == 1))
                {
                    pagedData = db.Page<SYS_Version>(1, itemsPerPage, "SELECT * FROM SYS_Version " + whereClause + " " + orderByClause, whereArgs);
                    totalPages = pagedData.TotalPages;
                    totalItems = pagedData.TotalItems;
                }
                pagedSysVersionList = pagedData.Items;
            }
            else
            {
                Dictionary<Guid, SYS_Version> fileSysVersionList = FileSource.LoadSYSVersionData();
                if (whereClause.Trim().Length > 0)
                {
                    Dictionary<string, string> whereList = GetWhereList(whereClause, whereArgs);
                    Dictionary<Guid, SYS_Version> filteredSYSVersionList = new Dictionary<Guid, SYS_Version>();
                    foreach (KeyValuePair<Guid, SYS_Version> item in fileSysVersionList)
                    {
                        bool add =true ;
                        foreach (KeyValuePair<string, string> whereCol in whereList)
                        {
                            bool match = false;
                            if (item.Value.HaveColumn(whereCol.Key, whereCol.Value, out match) == true)
                            {
                                if (match == false)
                                {
                                    add = false;
                                    break;
                                }
                            }else
                            {
                                add = false;
                            }
                        }
                        if (add == true) filteredSYSVersionList.Add(item.Key, item.Value);
                    }
                    pagedSysVersionList = FileSource.GetPagedSYSVersion(filteredSYSVersionList, pageNo, itemsPerPage, out totalPages, out totalItems);
                }
                else
                {
                    pagedSysVersionList = FileSource.GetPagedSYSVersion(fileSysVersionList, pageNo, itemsPerPage, out totalPages, out totalItems);
                }
            }

            retTotalPages = totalPages;
            retTotalItems = totalItems;
			retMessage = message;
            return pagedSysVersionList;
        }

        #endregion

        #region Insert

        public static double InsertSysVersion(out string retMessage, SYS_Version sysVersion)
        {
			string message = "";
            double id = 0;
            if (sysVersion.VersionNoGUID != Guid.Empty) throw new Exception("Cannot Set the GUID for a Insert");
            sysVersion.VersionNoGUID = Guid.NewGuid();


            Database db = HaveDb(out message);
            if (db != null)
            {
                 db.Insert(sysVersion);
                 id = sysVersion.VersionNo;
            }
            else
            {
                Dictionary<Guid, SYS_Version> fileSysVersionList = FileSource.LoadSYSVersionData();
  
                fileSysVersionList.Add(sysVersion.VersionNoGUID, sysVersion);
                FileSource.SaveSYSVersionData(fileSysVersionList);

                id = sysVersion.VersionNo;
            }

			retMessage = message;
            return id;
        }

        #endregion

        #region Update

        public static int UpdateSysVersion(out string retMessage, SYS_Version sysVersion)
        {
			string message = "";
            int retAffectedRecordCount = 0;
            Database db = HaveDb(out message);
            if (db != null)
            {
                SYS_Version sysVersionExisting = GetSysVersion(out message, sysVersion.VersionNo, "");
                if (sysVersionExisting != null)
                {
                      
                    retAffectedRecordCount = db.Update(sysVersion);
                }
            }
            else
            {
                Dictionary<Guid, SYS_Version> fileSysVersionList = FileSource.LoadSYSVersionData();                
                if (fileSysVersionList.ContainsKey(sysVersion.VersionNoGUID) == true)
                {
                    fileSysVersionList.Remove(sysVersion.VersionNoGUID);
                    fileSysVersionList.Add(sysVersion.VersionNoGUID, sysVersion);
					retAffectedRecordCount++;
                    FileSource.SaveSYSVersionData(fileSysVersionList);
                }
            }

			retMessage = message;
            return retAffectedRecordCount;
        }

        #endregion

        #region Delete

        public static int DeleteSysVersion(out string retMessage, long sysVersionId)
        {
			int retAffectedRecordCount = 0;
			string message = "";

            Database db = HaveDb(out message);
            if (db != null)
            {
                retAffectedRecordCount = db.Delete<SYS_Version>(sysVersionId);
            }
            else
            {
                Dictionary<Guid, SYS_Version> fileSysVersionList = FileSource.LoadSYSVersionData();
                Guid sysVersionGuidToRemove = Guid.Empty;
                foreach (KeyValuePair<Guid, SYS_Version> item in fileSysVersionList)
                {
                    if (item.Value.VersionNo == sysVersionId)
                    {
                        sysVersionGuidToRemove = item.Key;
                        break;
                    }
                }
                if (sysVersionGuidToRemove != Guid.Empty)
                {
                    fileSysVersionList.Remove(sysVersionGuidToRemove);
					retAffectedRecordCount++;
                    FileSource.SaveSYSVersionData(fileSysVersionList);
                }
            }

			retMessage = message;
			return retAffectedRecordCount;
        }

        #endregion

        #endregion
  
    }
}
