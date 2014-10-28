using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Data;
using System.Data.Common;

using Arshu.Web.Basic.Utility;
using Arshu.Web.Common;
using Arshu.Web.IO;
using Arshu.Data.Common;

using Arshu.Web.Json;

namespace Arshu.AppWeb
{
    [JsonRpcHelp("Json Db Handler")]
    public class JsonDbHandler : JsonRpcHandler
    {
        public static string JsonBaseHandlerName = "JsonDb.ashx";
        public static string JsonBaseHandlerTypeName = typeof(JsonDbHandler).FullName;

        private static JsonDbService service = new JsonDbService();
        public JsonDbHandler()
        {
            if (service == null) service = new JsonDbService();
        }
    }

    [JsonRpcHelp("Json Db Service")]
    public class JsonDbService : JsonRpcService
    {
        private static SafeDictionary<string, IDbConnection> _dbList = new SafeDictionary<string, IDbConnection>();
        private IDbConnection GetDbConnection(string dbName)
        {
            IDbConnection dbConn = null;
            dbName = dbName.Trim();
            if (_dbList.ContainsKey(dbName) == false)
            {
                dbName = IOManager.ProcessPath(dbName);
                if (dbName.StartsWith(IOManager.DirectorySeparator) == true) dbName = dbName.Substring(1);
                string dbPath = IOManager.Combine(IOManager.RootDirectory, dbName);
                string providerNameFactory = DataCommon.SQLiteProviderNameFactory;
                string additionalConfig = "Version=3;FailIfMissing=True;"; //Pooling=True;Synchronous=Off;journal mode=WAL
                string connectionString = "Data Source=" + dbPath + ";" + additionalConfig;
                string message = "";
                string errorMessage = "";
                DbProviderFactory _factory = DataCommon.GetDBProviderFactory(providerNameFactory, out message, out errorMessage);
                if (_factory != null)
                {
                    dbConn = _factory.CreateConnection();

                    #region Open Connection

                    dbConn.ConnectionString = connectionString;
                    if (dbConn.State == ConnectionState.Closed)
                    {
                        dbConn.Open();
                    }

                    #endregion

                    #region Check Connection

                    if (dbConn.State == ConnectionState.Open)
                    {
                        _dbList.Add(dbName, dbConn);
                    }

                    #endregion
                }
            }
            else
            {
                #region Get Connection

                _dbList.TryGetValue(dbName, out dbConn);

                if (dbConn.State == ConnectionState.Closed)
                {
                    dbConn.Open();
                }

                #endregion
            }
            return dbConn;
        }

        [JsonRpcMethod("GetSQLiteDatabases")]
        [JsonRpcHelp("Get the Databases in the App Data Directory and Returns Json[data, error]")]
        public JsonObject GetSQLiteDatabases()
        {
            JsonObject retMessage = new JsonObject();

            string[] dbFileList = IOManager.GetTopFiles(IOManager.AppDataDirectory, "*.db");
            if (dbFileList.Length > 0)
            {
                Dictionary<string, string> fileNameList = new Dictionary<string, string>();
                foreach (string item in dbFileList)
                {
                    string fileName = IOManager.GetFileName(item);
                    string filePath = IOManager.ProcessPath(item.Replace(IOManager.RootDirectory, "/")).Replace("\\", "/");
                    fileNameList.Add(filePath, fileName);
                }
                retMessage.Add("data", fileNameList);
            }
            else
            {
                retMessage.Add("error", "No Db Files having db extension found in App Data Directory");
            }
            return retMessage;
        }

        [JsonRpcMethod("GetTables")]
        [JsonRpcHelp("Get the Tables from the Database and Return Json[data, error]")]
        public JsonObject GetTables(string dbName)
        {
            JsonObject retMessage = new JsonObject();
            try
            {
                IDbConnection dbConn = GetDbConnection(dbName);
                if (dbConn != null)
                {
                    IDbCommand queryCommand = dbConn.CreateCommand();
                    queryCommand.CommandText = @"SELECT * FROM SQLITE_MASTER ORDER BY tbl_name, type desc";

                    List<string> tableList = new List<string>();
                    using (IDataReader reader = queryCommand.ExecuteReader())
                    {
                        string tableName = "";
                        string type = "";
                        string sysversionTable = "";
                        while (reader.Read())
                        {
                            type = (string)reader["type"];
                            if (type.ToUpper() == "table".ToUpper())
                            {
                                tableName = (string)reader["name"];
                                if (string.IsNullOrEmpty(tableName) == false)
                                {
                                    if (tableName.ToUpper() != "SYS_VERSION".ToUpper())
                                    {
                                        tableList.Add(tableName);
                                    }
                                    else if (tableName.ToUpper() == "SYS_VERSION".ToUpper())
                                    {
                                        sysversionTable = tableName;
                                    }
                                }
                            }
                        }
                        if (sysversionTable.Trim().Length > 0)
                        {
                            tableList.Insert(0, sysversionTable);
                        }
                    }
                    retMessage.Add("data", tableList);
                }
                else
                {
                    retMessage.Add("error", "Invalid Db [" + dbName + "]");
                }

            }
            catch (Exception ex)
            {
                retMessage.Add("error", ex.Message);
            }

            return retMessage;
        }

        [JsonRpcMethod("DoGenSql")]
        [JsonRpcHelp("Generate Sql Statement and Returns Json[recordCount, selectSql, insertSql, updateSql, deleteSql, error]")]
        public JsonObject DoGenSql(string dbName, string dbTableName, long recordNo, string fieldSeparator)
        {
            JsonObject retMessage = new JsonObject();

            try
            {
                string statementSeparator = Environment.NewLine;
                //if (string.IsNullOrEmpty(separator) == true) separator = Environment.NewLine;
                IDbConnection dbConn = GetDbConnection(dbName);
                if (dbConn != null)
                {
                    long recordCount = 0;
                    string selectSql = "";
                    string insertSql = "";
                    string updateSql = "";
                    string deleteSql = "";

                    #region Get the Record Count

                    string sqlCount = "SELECT Count(*) FROM " + dbTableName;
                    using (IDbCommand countCommand = dbConn.CreateCommand())
                    {
                        countCommand.CommandText = sqlCount;
                        using (IDataReader reader = countCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                recordCount = reader.GetInt64(0);
                            }
                        }
                    }

                    #endregion

                    #region Generate the SQL Statement

                    string sqlSelect = "SELECT * FROM " + dbTableName;
                    using (IDbCommand queryCommand = dbConn.CreateCommand())
                    {
                        queryCommand.CommandText = sqlSelect;

                        using (IDataReader reader = queryCommand.ExecuteReader())
                        {
                            #region Generate Field Names

                            string fieldNames = "";
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string fieldName = reader.GetName(i);
                                if (i > 0) fieldNames += "," + fieldSeparator;
                                fieldNames += fieldName;
                            }

                            #endregion

                            #region Get the Particular Record or Last Record

                            long currentRecordNo = 0;
                            while (reader.Read())
                            {
                                currentRecordNo++;
                                if (currentRecordNo >= recordCount) break;
                                if (currentRecordNo >= recordNo) break;
                            }

                            #endregion

                            #region Get the Field Values

                            string fieldValues = "";
                            string fieldNameValues = "";
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string fieldName = reader.GetName(i);
                                Type fieldType = reader.GetFieldType(i);
                                object fieldValueObj = null;
                                if (fieldType.Name.Contains(typeof(DateTime).Name) == true)
                                {
                                    try
                                    {
                                        string columnValue = reader.GetString(i);
                                        DateTime columnDateValue = DateTime.MinValue;
                                        if (DateTime.TryParse(columnValue, out columnDateValue) == true)
                                        {
                                            fieldValueObj = columnDateValue;
                                        }
                                    }
                                    catch
                                    {

                                    }
                                }
                                else
                                {
                                    fieldValueObj = reader.GetValue(i);
                                }
                                string fieldValue = "";
                                if (fieldValueObj != null) fieldValue = fieldValueObj.ToString();

                                if (i > 0) fieldValues += "," + fieldSeparator;
                                if (i > 0) fieldNameValues += "," + fieldSeparator;

                                if (fieldType.Name.Contains(typeof(string).Name) == true)
                                {
                                    if (string.IsNullOrEmpty(fieldValue) == true)
                                    {
                                        fieldValues += "''";
                                        fieldNameValues += fieldName + "=''";
                                    }
                                    else
                                    {
                                        fieldValues += "'" + fieldValue.ToString().Replace("'", "''") + "'";
                                        fieldNameValues += fieldName + "='" + fieldValue.ToString().Replace("'", "''") + "'";
                                    }
                                }
                                else if ((fieldType.Name.Contains(typeof(Int64).Name) == true) || (fieldType.Name.Contains(typeof(Int32).Name) == true))
                                {
                                    if (string.IsNullOrEmpty(fieldValue) == true)
                                    {
                                        fieldValues += "0";
                                        fieldNameValues += fieldName + "=0";
                                    }
                                    else
                                    {
                                        fieldValues += fieldValue;
                                        fieldNameValues += fieldName + "=" + fieldValue;
                                    }
                                }
                                else if (fieldType.Name.Contains(typeof(bool).Name) == true)
                                {
                                    if (string.IsNullOrEmpty(fieldValue) == true)
                                    {
                                        fieldValues += "0";
                                        fieldNameValues += fieldName + "=0";
                                    }
                                    else
                                    {
                                        if (fieldValue.ToUpper().Contains("TRUE") == true)
                                        {
                                            fieldValues += "1";
                                            fieldNameValues += fieldName + "=" + 1;
                                        }
                                        else
                                        {
                                            fieldValues += "0";
                                            fieldNameValues += fieldName + "=" + 0;
                                        }
                                    }
                                }
                                else if (fieldType.Name.Contains(typeof(DateTime).Name) == true)
                                {
                                    if (string.IsNullOrEmpty(fieldValue) == true)
                                    {
                                        fieldValues += "'" + DateTime.Now.ToString() + "'";
                                        fieldNameValues += fieldName + "='" + DateTime.Now.ToString() + "'";
                                    }
                                    else
                                    {
                                        fieldValues += "'" + fieldValue + "'";
                                        fieldNameValues += fieldName + "='" + fieldValue + "'";
                                    }
                                }
                                else if (fieldType.Name.Contains(typeof(Guid).Name) == true)
                                {
                                    if (string.IsNullOrEmpty(fieldValue) == true)
                                    {
                                        fieldValues += "'" + Guid.NewGuid().ToString() + "'";
                                        fieldNameValues += fieldName + "='" + Guid.NewGuid().ToString() + "'";
                                    }
                                    else
                                    {
                                        fieldValues += "'" + fieldValue + "'";
                                        fieldNameValues += fieldName + "='" + fieldValue + "'";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(fieldValue) == true)
                                    {
                                        fieldValues += "NULL";
                                        fieldNameValues += fieldName + "=NULL";
                                    }
                                    else
                                    {
                                        fieldValues += fieldValue;
                                        fieldNameValues += fieldName + "=" + fieldValue;
                                    }
                                }
                            }

                            #endregion

                            selectSql = "SELECT " + statementSeparator + fieldNames + statementSeparator + "FROM " + dbTableName + statementSeparator + "WHERE " + statementSeparator + fieldNameValues.Replace(",", " AND ");
                            insertSql = "INSERT INTO " + dbTableName + "(" + statementSeparator + fieldNames + statementSeparator + ") VALUES (" + statementSeparator + fieldValues + statementSeparator + ")";
                            updateSql = "UPDATE " + dbTableName + " SET " + statementSeparator + fieldNameValues + statementSeparator + "WHERE " + statementSeparator + fieldNameValues.Replace(",", " AND ");
                            deleteSql = "DELETE FROM " + dbTableName + statementSeparator + "WHERE " + statementSeparator + fieldNameValues.Replace(",", " AND ");
                        }
                    }

                    #endregion

                    retMessage.Add("recordCount", recordCount);
                    retMessage.Add("selectSql", selectSql);
                    retMessage.Add("insertSql", insertSql);
                    retMessage.Add("updateSql", updateSql);
                    retMessage.Add("deleteSql", deleteSql);
                }
                else
                {
                    retMessage.Add("error", "Invalid Db [" + dbName + "]");
                }

            }
            catch (Exception ex)
            {
                retMessage.Add("error", ex.Message);
            }


            return retMessage;
        }

        [JsonRpcMethod("DoSelect")]
        [JsonRpcHelp("Execute Select Sql Statement and Return Json[data(fieldName, fieldValue), error]")]
        public JsonObject DoSelect(string dbName, string sqlStatement)
        {
            JsonObject retMessage = new JsonObject();

            try
            {
                IDbConnection dbConn = GetDbConnection(dbName);
                if (dbConn != null)
                {
                    List<Dictionary<string, object>> recordList = new List<Dictionary<string, object>>();

                    using (IDbCommand queryCommand = dbConn.CreateCommand())
                    {
                        queryCommand.CommandText = sqlStatement.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\t", " ");

                        using (IDataReader reader = queryCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, object> columnList = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string fieldName = reader.GetName(i);
                                    Type fieldType = reader.GetFieldType(i);
                                    object fieldValue = null;
                                    if (fieldType.Name.Contains(typeof(DateTime).Name) == true)
                                    {
                                        try
                                        {
                                            string columnValue = reader.GetString(i);
                                            DateTime columnDateValue = DateTime.MinValue;
                                            if (DateTime.TryParse(columnValue, out columnDateValue) == true)
                                            {
                                                fieldValue = columnDateValue;
                                            }
                                        }
                                        catch
                                        {

                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            fieldValue = reader.GetValue(i);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    if (fieldValue == null) fieldValue = "NULL";
                                    columnList.Add(fieldName, fieldValue);
                                }
                                recordList.Add(columnList);
                            }
                        }
                    }
                    retMessage.Add("data", recordList);
                }
                else
                {
                    retMessage.Add("error", "Invalid Db [" + dbName + "]");
                }

            }
            catch (Exception ex)
            {
                retMessage.Add("error", ex.Message);
            }


            return retMessage;
        }

        [JsonRpcMethod("DoSelectScalar")]
        [JsonRpcHelp("Execute Select Sql Scalar Statement and Returns Json [data, error]")]
        public JsonObject DoSelectScalar(string dbName, string sqlStatement)
        {
            JsonObject retMessage = new JsonObject();

            try
            {
                IDbConnection dbConn = GetDbConnection(dbName);
                if (dbConn != null)
                {
                    using (IDbCommand queryCommand = dbConn.CreateCommand())
                    {
                        queryCommand.CommandText = sqlStatement.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\t", " ");

                        object result = queryCommand.ExecuteScalar();

                        retMessage.Add("data", result.ToString());
                    }
                }
                else
                {
                    retMessage.Add("error", "Invalid Db [" + dbName + "]");
                }

            }
            catch (Exception ex)
            {
                retMessage.Add("error", ex.Message);
            }


            return retMessage;
        }

        [JsonRpcMethod("DoInsert")]
        [JsonRpcHelp("Execute Insert Sql Statement and Returns Json[data(recordsAffected), error]")]
        public JsonObject DoInsert(string dbName, string sqlStatement)
        {
            JsonObject retMessage = new JsonObject();

            try
            {
                IDbConnection dbConn = GetDbConnection(dbName);
                if (dbConn != null)
                {
                    using (IDbCommand queryCommand = dbConn.CreateCommand())
                    {
                        queryCommand.CommandText = sqlStatement.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\t", " ");
                        long recordsAffected = queryCommand.ExecuteNonQuery();
                        retMessage.Add("data", recordsAffected);
                    }
                }
                else
                {
                    retMessage.Add("error", "Invalid Db [" + dbName + "]");
                }

            }
            catch (Exception ex)
            {
                retMessage.Add("error", ex.Message);
            }


            return retMessage;
        }

        [JsonRpcMethod("DoUpdate")]
        [JsonRpcHelp("Execute Update Sql Statement and Returns Json[data(recordsAffected), error]")]
        public JsonObject DoUpdate(string dbName, string sqlStatement)
        {
            JsonObject retMessage = new JsonObject();

            try
            {
                IDbConnection dbConn = GetDbConnection(dbName);
                if (dbConn != null)
                {
                    using (IDbCommand queryCommand = dbConn.CreateCommand())
                    {
                        queryCommand.CommandText = sqlStatement.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\t", " ");
                        long recordsAffected = queryCommand.ExecuteNonQuery();
                        retMessage.Add("data", recordsAffected);
                    }
                }
                else
                {
                    retMessage.Add("error", "Invalid Db [" + dbName + "]");
                }

            }
            catch (Exception ex)
            {
                retMessage.Add("error", ex.Message);
            }


            return retMessage;
        }

        [JsonRpcMethod("DoDelete")]
        [JsonRpcHelp("Execute Delete Sql Statement and Returns Json[data(recordsAffected), error]")]
        public JsonObject DoDelete(string dbName, string sqlStatement)
        {
            JsonObject retMessage = new JsonObject();

            try
            {
                IDbConnection dbConn = GetDbConnection(dbName);
                if (dbConn != null)
                {
                    using (IDbCommand queryCommand = dbConn.CreateCommand())
                    {
                        queryCommand.CommandText = sqlStatement.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\t", " ");
                        long recordsAffected = queryCommand.ExecuteNonQuery();
                        retMessage.Add("data", recordsAffected);
                    }
                }
                else
                {
                    retMessage.Add("error", "Invalid Db [" + dbName + "]");
                }

            }
            catch (Exception ex)
            {
                retMessage.Add("error", ex.Message);
            }


            return retMessage;
        }

        [JsonRpcMethod("OpenConnection")]
        [JsonRpcHelp("Open Connection using ProviderNameFactory and ConnectionString and Returns Json[message, error]")]
        public JsonObject OpenConnection(string dbName, string providerNameFactory, string connectionString)
        {
            JsonObject retMessage = new JsonObject();

            DbConnection dbConn = null;
            dbName = dbName.Trim();
            if (_dbList.ContainsKey(dbName) == false)
            {
                string message = "";
                string errorMessage = "";
                DbProviderFactory _factory = DataCommon.GetDBProviderFactory(providerNameFactory, out message, out errorMessage);
                if (_factory != null)
                {
                    dbConn = _factory.CreateConnection();

                    #region Open Connection

                    dbConn.ConnectionString = connectionString;
                    if (dbConn.State == ConnectionState.Closed)
                    {
                        dbConn.Open();
                    }

                    #endregion

                    #region Check Connection

                    if (dbConn.State == ConnectionState.Open)
                    {
                        _dbList.Add(dbName, dbConn);
                        retMessage.Add("message", "Connection Opened Successfully");
                    }
                    else
                    {
                        retMessage.Add("error", "Unable to Open Connection");
                    }

                    #endregion
                }
                else
                {
                    retMessage.Add("error", message);
                }
            }
            else
            {
                retMessage.Add("error", "Connection allready Open");
            }

            return retMessage;
        }

    }
}
