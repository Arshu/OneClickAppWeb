
using System;
using System.Collections.Generic;
using PetaPoco;

namespace App.Secure.Entity
{
	


    	
	[TableName("SYD_User")]   
	[PrimaryKey("UserID")]   
	[ExplicitColumns]
    public partial class SYD_User  
    {
		public bool HaveColumn(string columnName, string columnValue, out bool retValueMatched)
		{
          bool ret = false;
          bool valueMatched = false;
          if (columnName == "UserID") 
          {
              ret = true;
              if (UserID.ToString() == columnValue) valueMatched = true;
          }
          if (columnName == "UserGUID") 
          {
              ret = true;
              if (UserGUID.ToString() == columnValue) valueMatched = true;
          }
          if (columnName == "UserAlias") 
          {
              ret = true;
              if (UserAlias.ToString() == columnValue) valueMatched = true;
          }
          if (columnName == "UserHash") 
          {
              ret = true;
              if (UserHash.ToString() == columnValue) valueMatched = true;
          }
          if (columnName == "IsAdmin") 
          {
              ret = true;
              if (IsAdmin.ToString() == columnValue) valueMatched = true;
          }
          retValueMatched = valueMatched;
          return ret;
		}
      
		long _userID;
		[Column] 
		public long UserID 
		{ 
			get
			{
				return _userID;
			}
			set
			{
				_userID = value;
			}
		}
      
		Guid _userGUID;
		[Column] 
		public Guid UserGUID 
		{ 
			get
			{
				return _userGUID;
			}
			set
			{
				_userGUID = value;
			}
		}
      
		string _userAlias;
		[Column] 
		public string UserAlias 
		{ 
			get
			{
				return _userAlias;
			}
			set
			{
				_userAlias = value;
			}
		}
      
		string _userHash;
		[Column] 
		public string UserHash 
		{ 
			get
			{
				return _userHash;
			}
			set
			{
				_userHash = value;
			}
		}
      
		bool _isAdmin;
		[Column] 
		public bool IsAdmin 
		{ 
			get
			{
				return _isAdmin;
			}
			set
			{
				_isAdmin = value;
			}
		}
	} //Class End
    	
	[TableName("SYS_Version")]   
	[PrimaryKey("VersionNo", AutoIncrement=false)]   
	[ExplicitColumns]
    public partial class SYS_Version  
    {
		public bool HaveColumn(string columnName, string columnValue, out bool retValueMatched)
		{
          bool ret = false;
          bool valueMatched = false;
          if (columnName == "VersionNo") 
          {
              ret = true;
              if (VersionNo.ToString() == columnValue) valueMatched = true;
          }
          if (columnName == "VersionNoGUID") 
          {
              ret = true;
              if (VersionNoGUID.ToString() == columnValue) valueMatched = true;
          }
          if (columnName == "VersionName") 
          {
              ret = true;
              if (VersionName.ToString() == columnValue) valueMatched = true;
          }
          retValueMatched = valueMatched;
          return ret;
		}
      
		double _versionNo;
		[Column] 
		public double VersionNo 
		{ 
			get
			{
				return _versionNo;
			}
			set
			{
				_versionNo = value;
			}
		}
      
		Guid _versionNoGUID;
		[Column] 
		public Guid VersionNoGUID 
		{ 
			get
			{
				return _versionNoGUID;
			}
			set
			{
				_versionNoGUID = value;
			}
		}
      
		string _versionName;
		[Column] 
		public string VersionName 
		{ 
			get
			{
				return _versionName;
			}
			set
			{
				_versionName = value;
			}
		}
	} //Class End

}
