

using System;
using System.Text;
using System.Collections.Generic;

using Arshu.Web.Basic.Compress;
using Arshu.Web.Common;
using Arshu.Web.Http;
using Arshu.Web.IO;

namespace App.Secure.Views
{


	#region Html TemplateIndex Class	

    public class TemplateIndex	: ITemplate 
    {
		#region Constants
			
		public string RelativeFilePath
        {
            get
            {
                return "Index.html";
            }
        }

		public string ScriptRelativeFilePath
        {
            get
            {
                return "";
            }
        }

		public string MinScriptRelativeFilePath
        {
            get
            {
                return "";
            }
        }

		public string StyleRelativeFilePath
        {
            get
            {
                return "";
            }
        }

		public string MinStyleRelativeFilePath
        {
            get
            {
                return "";
            }
        }

		private const string TemplateSource_600="EDwhZG9jdHlwZSBodG1sPgo8wAYFZWFkPgogIAAUPHRpdGxlPlNlY3VyZSBBcHA8L3RpQBEXCgk8bWV0YSBuYW1lPSJhcHBsZS1tb2JpIAYJd2ViLWFwcC10aSAoCyIgY29udGV4dD0iU+AARAAigFwBICCAY2BIBWNoYXJzZSAmCFVURi04IiAvPiAfQAACCgk8YCAPbmFtZT0idmlld3BvcnQiIGBXEW50PSJ3aWR0aD1kZXZpY2Utd0AMFCwgaW5pdGlhbC1zY2FsZT0xIj4KCYBrCiEtLSBLaWxscyB0IJQEIHpvb20gLAYgaU9TLi0tgJYAPOACbuAM2ARjYXBhYuAA2kCCA3llcyLgHjwJc3RhdHVzLWJhciAKAHngBEUFYmxhY2sigPcACmCuFHt7Q29tbW9uSGVhZGVyfX0KCjwvaGGcBTxib2R5IGBBHz0icGFkZGluZzogMHB4O2JhY2tncm91bmQtaW1hZ2U6E3VybCgnL0FwcF9SZXNvdXJjZS9i4AAjCC5wbmcnKTsiPsBwCU1haW5QYWdlfX3gBIEFRm9vdGVyIIFBkkAAAwo8L2IgggE+CiCRA3RtbD4=";

		#endregion		


		
		
		#region PlaceHolder Properties

			private string _commonHeader;
			public string CommonHeader
			{
				get { return _commonHeader; }
				set { _commonHeader = value; }
			}
			private string _mainPage;
			public string MainPage
			{
				get { return _mainPage; }
				set { _mainPage = value; }
			}
			private string _commonFooter;
			public string CommonFooter
			{
				get { return _commonFooter; }
				set { _commonFooter = value; }
			}

		#endregion

		#region Validation Method

		
		public virtual bool IsScriptValid(bool throwException, out string retMessage)
		{

			bool ret =true;
			StringBuilder message = new StringBuilder();
			retMessage = message.ToString();
			return ret;

		}
		
		public virtual bool IsStyleValid(bool throwException, out string retMessage)
		{

			bool ret =true;
			StringBuilder message = new StringBuilder();
			retMessage = message.ToString();
			return ret;

		}
		
		public virtual bool IsValid(bool throwException, out string retMessage)
		{

			bool ret =true;
			StringBuilder message = new StringBuilder();
			retMessage = message.ToString();
			return ret;

		}

		#endregion 

		#region Script Fill Methods

		public virtual string GetScript(bool validate, bool throwException, out string retMessage)
        {
			throw new NotImplementedException();
        }

		public virtual string GetScriptFilled(bool includeScriptTag,  bool validate, bool throwException, out string retMessage)
        {
			throw new NotImplementedException();
        }

		public virtual string GetScriptFilled(string scriptTemplate, bool includeScriptTag,  bool validate, bool throwException, out string retMessage)
        {
			throw new NotImplementedException();
        }

		#endregion

		#region Style Fill Methods

		public virtual string GetStyle(bool validate, bool throwException, out string retMessage)
        {
			throw new NotImplementedException();
        }

		public virtual string GetStyleFilled(bool includeStyleTag,  bool validate, bool throwException, out string retMessage)
        {
			throw new NotImplementedException();
        }

		public virtual string GetStyleFilled(string styleTemplate, bool includeStyleTag,  bool validate, bool throwException, out string retMessage)
        {
			throw new NotImplementedException();
        }

		#endregion

        #region Fill Method

		public virtual string GetTemplate(string templateSuffix, bool validate, bool throwException, out string retMessage)
        {
            string message ="";
            string template = "";
			
            if ((HttpBaseHandler.DevelopmentTestMode == false) && (HttpBaseHandler.ProductionTestMode == false))
            {
				if (string.IsNullOrEmpty(template) ==true)
                {
					template = LZF.DecompressFromBase64(TemplateSource_600);
				}
            }
            else
            {
				if (string.IsNullOrEmpty(templateSuffix) ==false)
				{
                    string fileExtension = IOManager.GetExtension(RelativeFilePath);
					string suffixRelativeFilePath = RelativeFilePath.Replace(fileExtension, "." + templateSuffix + fileExtension);
                    template = ResourceUtil.GetTextFromFile(suffixRelativeFilePath, HttpBaseHandler.ResourceCache);
				}
				if (string.IsNullOrEmpty(template) ==true)
				{
					template = ResourceUtil.GetTextFromFile(RelativeFilePath, HttpBaseHandler.ResourceCache);
				}
                if (string.IsNullOrEmpty(template) == true)
                {
					if (string.IsNullOrEmpty(template) == true)
					{
						template = LZF.DecompressFromBase64(TemplateSource_600);
					}
                }
            }
			
            retMessage = message;
			return template;
		}

		public virtual string GetFilled(string templateSuffix, bool validate, bool throwException, out string retMessage)
		{
			string message = "";

			string template = GetTemplate(templateSuffix, validate, throwException, out message);

			string filledTemplate = GetFilled(template, templateSuffix, validate, throwException, out message);

			retMessage = message ;
			return filledTemplate;
		}


		public virtual string GetFilled(string template, string templateSuffix, bool validate, bool throwException, out string retMessage)
		{
            string message = "";

			if ((string.IsNullOrEmpty(template)==false) && ((validate ==false) || IsValid(throwException, out message)))
            {
				template = ProcessListSection(this, template);

				template = ProcessBoolSection(this, template);
			
				template = ProcessPlaceHolder(this, template);
			}
			retMessage = message;
			return template;
		}

        #endregion

		#region Protected Methods
		
		protected static string ProcessListSection(TemplateIndex templateindex, string template)
		{

			return template;
		}
		
		protected static string ProcessBoolSection(TemplateIndex templateindex, string template)
		{

			return template;
		}
		
		protected static string ProcessPlaceHolder(TemplateIndex templateindex, string template)
		{
			template = template.Replace("{{CommonHeader}}", string.IsNullOrEmpty(templateindex.CommonHeader)==false ? templateindex.CommonHeader : "");
			template = template.Replace("{{MainPage}}", string.IsNullOrEmpty(templateindex.MainPage)==false ? templateindex.MainPage : "");
			template = template.Replace("{{CommonFooter}}", string.IsNullOrEmpty(templateindex.CommonFooter)==false ? templateindex.CommonFooter : "");

			return template;
		}
		
		
		#endregion

    }

	#endregion
	


	#region Html TemplateLiTwoColumn Class	

    public class TemplateLiTwoColumn	: ITemplate 
    {
		#region Constants
			
		public string RelativeFilePath
        {
            get
            {
                return "App_Service/App.Secure/View/Common/liTwoColumn.html";
            }
        }

		public string ScriptRelativeFilePath
        {
            get
            {
                return "";
            }
        }

		public string MinScriptRelativeFilePath
        {
            get
            {
                return "";
            }
        }

		public string StyleRelativeFilePath
        {
            get
            {
                return "";
            }
        }

		public string MinStyleRelativeFilePath
        {
            get
            {
                return "";
            }
        }

		private const string TemplateSource_260="HTxsaSBkYXRhLXJvbGU9Imxpc3QtZGl2aWRlciI+ICAAFDxkaXYgY2xhc3M9InVpLWdyaWQtYYAaQADgBh4EYmxvY2sgHxogc3R5bGU9InBhZGRpbmctcmlnaHQ6IDVweDvgATtAAA97e0NvbHVtbjFWYWx1ZX19QBNAAAQ8L2RpdmCAQAAAPOAFhAFibEBlAWIi4AdlAmxlZqBkEiB3aGl0ZS1zcGFjZTpub3JtYWzgDXcBMlbgD3eAgQQ8L2xpPg==";

		#endregion		


		
		
		#region PlaceHolder Properties

			private string _column1Value;
			public string Column1Value
			{
				get { return _column1Value; }
				set { _column1Value = value; }
			}
			private string _column2Value;
			public string Column2Value
			{
				get { return _column2Value; }
				set { _column2Value = value; }
			}

		#endregion

		#region Validation Method

		
		public virtual bool IsScriptValid(bool throwException, out string retMessage)
		{

			bool ret =true;
			StringBuilder message = new StringBuilder();
			retMessage = message.ToString();
			return ret;

		}
		
		public virtual bool IsStyleValid(bool throwException, out string retMessage)
		{

			bool ret =true;
			StringBuilder message = new StringBuilder();
			retMessage = message.ToString();
			return ret;

		}
		
		public virtual bool IsValid(bool throwException, out string retMessage)
		{

			bool ret =true;
			StringBuilder message = new StringBuilder();
			retMessage = message.ToString();
			return ret;

		}

		#endregion 

		#region Script Fill Methods

		public virtual string GetScript(bool validate, bool throwException, out string retMessage)
        {
			throw new NotImplementedException();
        }

		public virtual string GetScriptFilled(bool includeScriptTag,  bool validate, bool throwException, out string retMessage)
        {
			throw new NotImplementedException();
        }

		public virtual string GetScriptFilled(string scriptTemplate, bool includeScriptTag,  bool validate, bool throwException, out string retMessage)
        {
			throw new NotImplementedException();
        }

		#endregion

		#region Style Fill Methods

		public virtual string GetStyle(bool validate, bool throwException, out string retMessage)
        {
			throw new NotImplementedException();
        }

		public virtual string GetStyleFilled(bool includeStyleTag,  bool validate, bool throwException, out string retMessage)
        {
			throw new NotImplementedException();
        }

		public virtual string GetStyleFilled(string styleTemplate, bool includeStyleTag,  bool validate, bool throwException, out string retMessage)
        {
			throw new NotImplementedException();
        }

		#endregion

        #region Fill Method

		public virtual string GetTemplate(string templateSuffix, bool validate, bool throwException, out string retMessage)
        {
            string message ="";
            string template = "";
			
            if ((HttpBaseHandler.DevelopmentTestMode == false) && (HttpBaseHandler.ProductionTestMode == false))
            {
				if (string.IsNullOrEmpty(template) ==true)
                {
					template = LZF.DecompressFromBase64(TemplateSource_260);
				}
            }
            else
            {
				if (string.IsNullOrEmpty(templateSuffix) ==false)
				{
                    string fileExtension = IOManager.GetExtension(RelativeFilePath);
					string suffixRelativeFilePath = RelativeFilePath.Replace(fileExtension, "." + templateSuffix + fileExtension);
                    template = ResourceUtil.GetTextFromFile(suffixRelativeFilePath, HttpBaseHandler.ResourceCache);
				}
				if (string.IsNullOrEmpty(template) ==true)
				{
					template = ResourceUtil.GetTextFromFile(RelativeFilePath, HttpBaseHandler.ResourceCache);
				}
                if (string.IsNullOrEmpty(template) == true)
                {
					if (string.IsNullOrEmpty(template) == true)
					{
						template = LZF.DecompressFromBase64(TemplateSource_260);
					}
                }
            }
			
            retMessage = message;
			return template;
		}

		public virtual string GetFilled(string templateSuffix, bool validate, bool throwException, out string retMessage)
		{
			string message = "";

			string template = GetTemplate(templateSuffix, validate, throwException, out message);

			string filledTemplate = GetFilled(template, templateSuffix, validate, throwException, out message);

			retMessage = message ;
			return filledTemplate;
		}


		public virtual string GetFilled(string template, string templateSuffix, bool validate, bool throwException, out string retMessage)
		{
            string message = "";

			if ((string.IsNullOrEmpty(template)==false) && ((validate ==false) || IsValid(throwException, out message)))
            {
				template = ProcessListSection(this, template);

				template = ProcessBoolSection(this, template);
			
				template = ProcessPlaceHolder(this, template);
			}
			retMessage = message;
			return template;
		}

        #endregion

		#region Protected Methods
		
		protected static string ProcessListSection(TemplateLiTwoColumn templatelitwocolumn, string template)
		{

			return template;
		}
		
		protected static string ProcessBoolSection(TemplateLiTwoColumn templatelitwocolumn, string template)
		{

			return template;
		}
		
		protected static string ProcessPlaceHolder(TemplateLiTwoColumn templatelitwocolumn, string template)
		{
			template = template.Replace("{{Column1Value}}", string.IsNullOrEmpty(templatelitwocolumn.Column1Value)==false ? templatelitwocolumn.Column1Value : "");
			template = template.Replace("{{Column2Value}}", string.IsNullOrEmpty(templatelitwocolumn.Column2Value)==false ? templatelitwocolumn.Column2Value : "");

			return template;
		}
		
		
		#endregion

    }

	#endregion
	


	#region Html TemplateSelectOption Class	

    public class TemplateSelectOption	: ITemplate 
    {
		#region Constants
			
		public string RelativeFilePath
        {
            get
            {
                return "App_Service/App.Secure/View/Common/selectOption.html";
            }
        }

		public string ScriptRelativeFilePath
        {
            get
            {
                return "";
            }
        }

		public string MinScriptRelativeFilePath
        {
            get
            {
                return "";
            }
        }

		public string StyleRelativeFilePath
        {
            get
            {
                return "";
            }
        }

		public string MinStyleRelativeFilePath
        {
            get
            {
                return "";
            }
        }

		private const string TemplateSource_68="EzxvcHRpb24gdmFsdWU9Int7T3B0IA8AVkAOBX19Ij57e4AQCFRleHR9fTwvb0AuAW4+";

		#endregion		


		
		
		#region PlaceHolder Properties

			private string _optionValue;
			public string OptionValue
			{
				get { return _optionValue; }
				set { _optionValue = value; }
			}
			private string _optionText;
			public string OptionText
			{
				get { return _optionText; }
				set { _optionText = value; }
			}

		#endregion

		#region Validation Method

		
		public virtual bool IsScriptValid(bool throwException, out string retMessage)
		{

			bool ret =true;
			StringBuilder message = new StringBuilder();
			retMessage = message.ToString();
			return ret;

		}
		
		public virtual bool IsStyleValid(bool throwException, out string retMessage)
		{

			bool ret =true;
			StringBuilder message = new StringBuilder();
			retMessage = message.ToString();
			return ret;

		}
		
		public virtual bool IsValid(bool throwException, out string retMessage)
		{

			bool ret =true;
			StringBuilder message = new StringBuilder();
			retMessage = message.ToString();
			return ret;

		}

		#endregion 

		#region Script Fill Methods

		public virtual string GetScript(bool validate, bool throwException, out string retMessage)
        {
			throw new NotImplementedException();
        }

		public virtual string GetScriptFilled(bool includeScriptTag,  bool validate, bool throwException, out string retMessage)
        {
			throw new NotImplementedException();
        }

		public virtual string GetScriptFilled(string scriptTemplate, bool includeScriptTag,  bool validate, bool throwException, out string retMessage)
        {
			throw new NotImplementedException();
        }

		#endregion

		#region Style Fill Methods

		public virtual string GetStyle(bool validate, bool throwException, out string retMessage)
        {
			throw new NotImplementedException();
        }

		public virtual string GetStyleFilled(bool includeStyleTag,  bool validate, bool throwException, out string retMessage)
        {
			throw new NotImplementedException();
        }

		public virtual string GetStyleFilled(string styleTemplate, bool includeStyleTag,  bool validate, bool throwException, out string retMessage)
        {
			throw new NotImplementedException();
        }

		#endregion

        #region Fill Method

		public virtual string GetTemplate(string templateSuffix, bool validate, bool throwException, out string retMessage)
        {
            string message ="";
            string template = "";
			
            if ((HttpBaseHandler.DevelopmentTestMode == false) && (HttpBaseHandler.ProductionTestMode == false))
            {
				if (string.IsNullOrEmpty(template) ==true)
                {
					template = LZF.DecompressFromBase64(TemplateSource_68);
				}
            }
            else
            {
				if (string.IsNullOrEmpty(templateSuffix) ==false)
				{
                    string fileExtension = IOManager.GetExtension(RelativeFilePath);
					string suffixRelativeFilePath = RelativeFilePath.Replace(fileExtension, "." + templateSuffix + fileExtension);
                    template = ResourceUtil.GetTextFromFile(suffixRelativeFilePath, HttpBaseHandler.ResourceCache);
				}
				if (string.IsNullOrEmpty(template) ==true)
				{
					template = ResourceUtil.GetTextFromFile(RelativeFilePath, HttpBaseHandler.ResourceCache);
				}
                if (string.IsNullOrEmpty(template) == true)
                {
					if (string.IsNullOrEmpty(template) == true)
					{
						template = LZF.DecompressFromBase64(TemplateSource_68);
					}
                }
            }
			
            retMessage = message;
			return template;
		}

		public virtual string GetFilled(string templateSuffix, bool validate, bool throwException, out string retMessage)
		{
			string message = "";

			string template = GetTemplate(templateSuffix, validate, throwException, out message);

			string filledTemplate = GetFilled(template, templateSuffix, validate, throwException, out message);

			retMessage = message ;
			return filledTemplate;
		}


		public virtual string GetFilled(string template, string templateSuffix, bool validate, bool throwException, out string retMessage)
		{
            string message = "";

			if ((string.IsNullOrEmpty(template)==false) && ((validate ==false) || IsValid(throwException, out message)))
            {
				template = ProcessListSection(this, template);

				template = ProcessBoolSection(this, template);
			
				template = ProcessPlaceHolder(this, template);
			}
			retMessage = message;
			return template;
		}

        #endregion

		#region Protected Methods
		
		protected static string ProcessListSection(TemplateSelectOption templateselectoption, string template)
		{

			return template;
		}
		
		protected static string ProcessBoolSection(TemplateSelectOption templateselectoption, string template)
		{

			return template;
		}
		
		protected static string ProcessPlaceHolder(TemplateSelectOption templateselectoption, string template)
		{
			template = template.Replace("{{OptionValue}}", string.IsNullOrEmpty(templateselectoption.OptionValue)==false ? templateselectoption.OptionValue : "");
			template = template.Replace("{{OptionText}}", string.IsNullOrEmpty(templateselectoption.OptionText)==false ? templateselectoption.OptionText : "");

			return template;
		}
		
		
		#endregion

    }

	#endregion
	


}

