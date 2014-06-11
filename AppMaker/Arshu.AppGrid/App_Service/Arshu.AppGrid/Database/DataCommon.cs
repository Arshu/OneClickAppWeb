using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data.Common;

using Arshu.Web.Basic.Utility;
using Arshu.Web.Common;

namespace Arshu.Data.Common
{   
    public class DataCommon
    {
        #region DBProvider Constants

        public const string SQLiteProviderNameFactoryCommunity = "Community.CsharpSqlite.SQLiteClient.SqliteClientFactory, Community.CsharpSqlite.SQLiteClient";
        public const string SQLiteProviderNameFactory = "System.Data.SQLite.SQLiteFactory, System.Data.SQLite";
        public const string SQLiteProviderNameFactoryMono = "Mono.Data.Sqlite.SqliteFactory, Mono.Data.Sqlite";
        public const string MSSQLProviderNameFactory = "System.Data.SqlClient.SqlClientFactory, System.Data";
        public const string MSSQLCEProviderNameFactory = "System.Data.SqlServerCe.SqlCeProviderFactory, System.Data.SqlServerCe";
        public const string OracleProviderNameFactory = "System.Data.OracleClient.OracleClientFactory, System.Data.OracleClient";
        public const string ODBCProviderNameFactory = "System.Data.Odbc.OdbcFactory, System.Data";
        public const string OLEDBProviderNameFactory = "System.Data.OleDb.OleDbFactory, System.Data";
        public const string MySQLProviderNameFactory = "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data";
        public const string PostgreSQLProviderNameFactory = "Npgsql.NpgsqlFactory, Npgsql";

        #endregion

        #region Get Provider

        private static bool _executingDbProviderFactory = false;
        private static int _maxSleepCount = 50;
        static SafeDictionary<string, DbProviderFactory> _providerFactoryList = new SafeDictionary<string, DbProviderFactory>();
        public static DbProviderFactory GetDBProviderFactory(string providerNameFactory, out string retMessage, out string retErrorMessage)
        {
            string message = "";
            string errorMessage = "";
            DbProviderFactory factory = null;
            string monoProviderNameFactory = "";
            string monoProviderAssemblyName = "";
            string monoProviderFactoryTypeName = "";

            bool IsRunningOnMono = (Type.GetType("Mono.Runtime") != null);
#if __ANDROID__ || __IOS__
			IsRunningOnMono =true;
#endif
            if (IsRunningOnMono == true)
            {
                if (providerNameFactory.ToUpper().Contains("SQLite".ToUpper()) == true)
                {
                    monoProviderAssemblyName = "Mono.Data.Sqlite";
                    monoProviderNameFactory = "Mono.Data.Sqlite.SqliteFactory, Mono.Data.Sqlite";
                }
            }

            #region Sleep Delay

            int sleepCount = 0;
            while (_executingDbProviderFactory == true)
            {
                System.Threading.Thread.Sleep(100);
                sleepCount++;
                if (sleepCount >= _maxSleepCount) break;
                if (_executingDbProviderFactory == false) break;
            }

            #endregion

            #region Get Provider Factory

            if (_providerFactoryList.TryGetValue(providerNameFactory, out factory) == false)
            {
                if (_executingDbProviderFactory == false)
                {
                    _executingDbProviderFactory = true;

                    try
                    {
                        Type providerFactoryType = null;
                        string providerFactoryTypeName = "";
                        string providerAssemblyName = "";

                        #region Get the Provider Factory from the Loaded Assemblies

                        if (providerFactoryType == null)
                        {
                            string[] providerNameFactoryParts = providerNameFactory.Split(',');
                            if (providerNameFactoryParts.Length >= 2)
                            {
                                providerAssemblyName = providerNameFactoryParts[1].Trim();
                                providerFactoryTypeName = providerNameFactoryParts[0].Trim();

                                Dictionary<string, Assembly> checkAssemblyList = new Dictionary<string, Assembly>();

                                #region Find Assembly List

                                Assembly[] loadedAssemblyList = AssemblyCommon.GetApplicationLoadedAssemblies(false);
                                message += String.Format("No Of Loaded Assemblies Found : `{0}`", loadedAssemblyList.Length.ToString());
                                foreach (Assembly item in loadedAssemblyList)
                                {
                                    try
                                    {
                                        string currentAssemblyName = item.FullName.Substring(0, item.FullName.IndexOf(",", StringComparison.Ordinal));
                                        if (checkAssemblyList.ContainsKey(currentAssemblyName) == false)
                                        {
                                            checkAssemblyList.Add(currentAssemblyName, item);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        errorMessage += Environment.NewLine + "Error in Adding Loaded Assembly to List [" + ex.Message + "]";
                                    }
                                }

                                Dictionary<string, string> dirAssemblyList = AssemblyCommon.GetApplicationDllAssemblies();
                                message += Environment.NewLine + String.Format("No Of Dir Assemblies Found : `{0}`", dirAssemblyList.Count.ToString());
                                foreach (KeyValuePair<string, string> item in dirAssemblyList)
                                {
                                    if (checkAssemblyList.ContainsKey(item.Key) == false)
                                    {
                                        try
                                        {
                                            Assembly dirAssembly = Assembly.LoadFile(item.Value);
                                            if (dirAssembly != null)
                                            {
                                                checkAssemblyList.Add(item.Key, dirAssembly);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            errorMessage += Environment.NewLine + "Error in Adding Directory Assembly to List [" + ex.Message + "]";
                                        }
                                    }
                                }

                                #endregion

                                #region Search the Provider Factory in the Assembly

                                message += Environment.NewLine + String.Format("No Of Assemblies Checked : `{0}`", checkAssemblyList.Count.ToString());
                                foreach (KeyValuePair<string, Assembly> item in checkAssemblyList)
                                {
                                    Assembly itemAssembly = item.Value;
                                    message += Environment.NewLine + string.Format("Check Assembly : `{0}`", itemAssembly.FullName.ToString());

                                    if (itemAssembly != null) //&& (itemAssembly.IsDynamic == false))
                                    {
                                        #region Check if the Assembly is Provider Factory Assembly

                                        string currentAssemblyName = itemAssembly.FullName.Substring(0, itemAssembly.FullName.IndexOf(",", StringComparison.Ordinal));

                                        if (monoProviderAssemblyName.Trim().Length > 0)
                                        {
                                            if (monoProviderAssemblyName.Trim().ToUpper() != currentAssemblyName.Trim().ToUpper())
                                            {
                                                if (providerAssemblyName.Trim().ToUpper() != currentAssemblyName.Trim().ToUpper())
                                                {
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                monoProviderFactoryTypeName = "Mono.Data.Sqlite.SqliteFactory";
                                            }
                                        }
                                        else if (providerAssemblyName.Trim().ToUpper() != currentAssemblyName.Trim().ToUpper())
                                        {
                                            continue;
                                        }

                                        #endregion

                                        message += Environment.NewLine + string.Format("Processing Assembly : `{0}`", itemAssembly.FullName.ToString());

                                        #region Get the Provider Factory Type from the Provider Factory Assembly

                                        if (monoProviderFactoryTypeName.Trim().Length > 0)
                                        {
                                            providerFactoryType = itemAssembly.GetType(monoProviderFactoryTypeName, false, true);
                                        }
                                        else
                                        {
                                            providerFactoryType = itemAssembly.GetType(providerFactoryTypeName, false, true);
                                        }
                                        if (providerFactoryType != null)
                                        {
                                            message += Environment.NewLine + String.Format("Found Provider in Assembly : `{0}/{1}`", providerFactoryType.ToString(), item.Key);
                                            break;
                                        }

                                        #endregion

                                        if (providerFactoryType != null) break;
                                    }
                                }

                                #endregion
                            }
                        }

                        #endregion

                        #region Get the Provider Factory from the Loaded Types

                        if ((providerFactoryType == null) && (monoProviderNameFactory.Trim().Length > 0))
                        {
                            try
                            {
                                message += Environment.NewLine + String.Format("Load Provider Factory From Loaded Types : `{0}`", providerFactoryType.ToString());
                                providerFactoryType = Type.GetType(monoProviderNameFactory, false, true);
                            }
                            catch (Exception ex)
                            {
                                string error = ex.Message.Replace("\r\n", "\n").Replace("\n", " ");
                                if (monoProviderNameFactory.Trim().Length == 0)
                                {
                                    errorMessage += Environment.NewLine + "Exception:" + String.Format("Failed to load provider by Type `{0}` - {1}", providerNameFactory, error);
                                }
                                else
                                {
                                    errorMessage += Environment.NewLine + "Exception:" + String.Format("Failed to load provider by Type `{0}` - {1}", monoProviderNameFactory, error);
                                }
                            }
                        }
                       
                        #endregion

                        #region Create the Provider Factory

                        if (providerFactoryType != null && providerFactoryType.IsSubclassOf(typeof(DbProviderFactory)))
                        {
                            FieldInfo field = providerFactoryType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
                            if (field != null)
                            {
                                if (monoProviderFactoryTypeName.Trim().Length > 0)
                                {
                                    message += Environment.NewLine + String.Format("Creating Provider :`{0}`", monoProviderFactoryTypeName);
                                }
                                else
                                {
                                    message += Environment.NewLine + String.Format("Creating Provider :`{0}`", providerFactoryTypeName);
                                }
                                factory = field.GetValue(null) as DbProviderFactory;
                                //message += Environment.NewLine + String.Format("Provider Created:`{0}`", factory.ToString());
                            }
                        }
                        else
                        {
                            //throw new Exception("Invalid Provider Name Factory or Could Not Get the Provider Factory [" + providerNameFactory + "]");
                            errorMessage += Environment.NewLine + "Error:" + String.Format(" Invalid Provider Name Factory or Could Not Get the Provider Factory [ `{0}` ", providerNameFactory);
                        }

                        #endregion

                        #region Cache the Provider Factory

                        if (factory != null)
                        {
                            if (_providerFactoryList.ContainsKey(providerNameFactory) == false)
                            {
                                _providerFactoryList.Add(providerNameFactory, factory);
                            }
                        }

                        #endregion

                        _executingDbProviderFactory = false;
                    }
                    catch (Exception x)
                    {
                        _executingDbProviderFactory = false;

                        string error = x.Message.Replace("\r\n", "\n").Replace("\n", " ");
                        if (monoProviderNameFactory.Trim().Length == 0)
                        {
                            errorMessage += Environment.NewLine + "Exception:" + String.Format("Failed to load provider `{0}` - {1}", providerNameFactory, error);
                        }
                        else
                        {
                            errorMessage += Environment.NewLine + "Exception:" + String.Format("Failed to load provider `{0}` - {1}", monoProviderNameFactory, error);
                        }
                    }
                }

            }

            #endregion

            retMessage = message;
            retErrorMessage = errorMessage;
            return factory;
        }

        #endregion
    }
}
