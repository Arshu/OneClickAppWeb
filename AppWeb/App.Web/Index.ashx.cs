using System;
using System.Web;
using Arshu.Web.Http;

namespace App.Web
{
    /// <summary>
    /// Summary description for Index
    /// </summary>
    public class Index : HttpGenericHandler 
    {
        public override string GetResponse(bool reload, string postFilePath, bool isGetRequest, string rawUrl, string requestJson, System.Collections.Generic.Dictionary<string, string> queryString, DateTime startTime, out string retContentType)
        {
            //HttpBaseHandler.DevelopmentTestMode = true;

            return base.GetResponse(reload, postFilePath, isGetRequest, rawUrl, requestJson, queryString, startTime, out retContentType);
        }
    }
}