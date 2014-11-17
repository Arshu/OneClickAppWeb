using System;
using System.Web;
using Arshu.Web.Http;
using App.Web.Views;

namespace App.Web
{
    /// <summary>
    /// Summary description for Index
    /// </summary>
    public class Index : HttpGenericHandler 
    {
        public override string GetResponse(bool reload, string postFilePath, bool isGetRequest, string rawUrl, string requestJson, System.Collections.Generic.Dictionary<string, string> queryString, DateTime startTime, out string retContentType)
        {
            //HttpBaseHandler.ProcessResponseUrlList.Add(Resource.CssJqueryMobileInlinePng144.ToString());
            //HttpBaseHandler.ProcessResponseUrlList.Add(Resource.CssJqueryMobileTheme144.ToString());
            //HttpBaseHandler.ProcessResponseUrlList.Add(Resource.JsJqueryMobile144.ToString());

            ////JQuery
            //RegisterResource(ResourceGroup.G1, ResourceGroup.G1, Resource.JsJquery211, null);
            ////JQM                
            //RegisterResource(ResourceGroup.G1, ResourceGroup.G1, Resource.JsJqueryMobile144, null);
           

            ////JQM Theme
            //RegisterResource(ResourceGroup.G2, ResourceGroup.G2, Resource.CssJqueryMobile144, null);
            ////JQM Default Theme
            //RegisterResource(ResourceGroup.G2, ResourceGroup.G2, Resource.CssJqueryMobileInlinePng144, null);
            ////Main.css
            //RegisterResource(ResourceGroup.G2, ResourceGroup.G2, Resource.CssMain , null);

            ////JQM Auto Complete
            //RegisterResource(ResourceGroup.G3, ResourceGroup.G3, Resource.JsJqmAutoComplete152, null);
            ////Main.js
            //RegisterResource(ResourceGroup.G3, ResourceGroup.G3, Resource.JsMain, null);

            //SaveResourceMap();
            ////LoadResourceMap(true);

            //HttpBaseHandler.DevelopmentTestMode = true;

            return base.GetResponse(reload, postFilePath, isGetRequest, rawUrl, requestJson, queryString, startTime, out retContentType);
        }
    }
}