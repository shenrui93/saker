/***************************************************************************
 * 
 * 创建时间：   2016/10/10 13:08:19
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   一个关于Web操作的相关扩展
 * 
 * *************************************************************************/

using System;
using System.Text;
using System.Web;
using Saker.Web;

namespace Saker.Extension
{
    /// <summary>
    /// 一个关于Web操作的相关扩展
    /// </summary>
    public static class _WebExternBase
    {
        /// <summary>
        /// 获取一个参数的集合列表，该参数集合包含有Get请求参数也包含有POST请求的参数
        /// </summary>
        /// <param name="request">请求页面</param>
        /// <param name="call">一个检查回调</param>
        /// <returns></returns>
        public static IWebParamData GetAllWebParam(this HttpRequestBase request, Func<string, bool> call)
        {
            WebParamData _query_dic = new Saker.Web.WebParamData(true);
            var method = request.HttpMethod;

            foreach (var r in request.QueryString.AllKeys)
            {
                if (string.IsNullOrEmpty(r)) continue;
                var key = r.Trim();
                if (!call(key)) continue;
                _query_dic[key] = request.QueryString[key];

            }
            foreach (var r in request.Form.AllKeys)
            {
                if (string.IsNullOrEmpty(r)) continue;
                var key = r.Trim();
                if (!call(key)) continue;
                _query_dic[key] = request.Form[key];
            }

            return _query_dic;

        }
        /// <summary>
        /// 获取一个参数的集合列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="call"></param>
        /// <returns></returns>
        public static IWebParamData GetWebParam(this HttpRequestBase request, Func<string, bool> call)
        {
            Saker.Web.WebParamData _query_dic = new WebParamData(true);
            var method = request.HttpMethod;
            if (method == "GET")
            {
                foreach (var r in request.QueryString.AllKeys)
                {
                    if (string.IsNullOrEmpty(r)) continue;
                    var key = r.Trim();
                    if (!call(key)) continue;
                    _query_dic[key] = request.QueryString[key];

                }
            }
            else
            {
                foreach (var r in request.Form.AllKeys)
                {
                    if (string.IsNullOrEmpty(r)) continue;
                    var key = r.Trim();
                    if (!call(key)) continue;
                    _query_dic[key] = request.Form[key];
                }
            }

            return _query_dic;

        }
        /// <summary>
        ///  获取一个参数的集合列表，该参数集合包含有Get请求参数也包含有POST请求的参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IWebParamData GetWebParam(this HttpRequestBase request)
        {
            return GetWebParam(request, r => true);
        }
        /// <summary>
        /// 获取一个参数的集合列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IWebParamData GetAllWebParam(this HttpRequestBase request)
        {
            return GetAllWebParam(request, r => true);
        }
        /// <summary>
        /// 停止当前页面的请求。使用 POST 提交方式 将请求重定向到新 URL 并指定该新 URL。
        /// </summary>
        public static void RedirectPostUrl(this HttpResponseBase response, string url, IWebParamData param, CheckUrlKeyCall func)
        {
            response.Write(param.ToPostHtml(url, func));
            response.Flush();
            response.End();
        }
        /// <summary>
        /// 停止当前页面的请求。使用 POST 提交方式 将请求重定向到新 URL 并指定该新 URL。
        /// </summary>
        public static void RedirectPostUrl(this HttpResponseBase response, string url, IWebParamData param)
        {
            response.RedirectPostUrl(url, param, r => true);
        }
        /// <summary>
        /// 获取请求的响应文本
        /// </summary>
        /// <param name="Req"></param>
        /// <returns></returns>
        public static string GetPostData(this HttpRequestBase Req)
        {
            if (Req == null) return "";
            var stream = Req.InputStream;
            if (stream == null) return "";
            if (!stream.CanSeek) return "";
            stream.Position = 0;
            using (var ms = new System.IO.MemoryStream())
            {
                stream.CopyTo(ms);
                return (Req.ContentEncoding ?? Encoding.UTF8).GetString(ms.ToArray());
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IWebParamData GetQueryParamData(this HttpRequestBase request)
        {
            WebParamData _query_dic = new WebParamData(true);
            var method = request.HttpMethod;

            foreach (var r in request.QueryString.AllKeys)
            {
                if (string.IsNullOrEmpty(r)) continue;
                var key = r.Trim();
                _query_dic[key] = request.QueryString[key];
            }

            return _query_dic;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IWebParamData GetFormParamData(this HttpRequestBase request)
        { 
            Saker.Web.WebParamData _query_dic = new WebParamData(true);
            var method = request.HttpMethod;

            foreach (var r in request.Form.AllKeys)
            {
                if (string.IsNullOrEmpty(r)) continue;
                var key = r.Trim();
                _query_dic[key] = request.Form[key];
            }

            return _query_dic;

        }

        /// <summary>
        /// 获取一个请求上下文的调试测试信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="call">获取信息的回调</param>
        /// <returns></returns>
        //[System.Diagnostics.Conditional("WEB_DEBUG")]
        public static void GetRequestDebugMessage(this HttpContextBase context,Action<string> call)
        {
            var request = context.Request;
           string SS =$@"
    请求地址：   {request.RawUrl}
    请求标头：
{GetHeaderInfo(request)}
    请求参数：   {request.QueryString}
    表单数据：   {request.GetPostData()}
    请求方式：   {request.HttpMethod}
    代理信息：   {request.UserAgent} 
    主机地址：   {request.UserHostAddress}";
            call?.Invoke(SS);
        } 
        private static string GetHeaderInfo(HttpRequestBase req)
        { 
            StringBuilder sb = new StringBuilder();
            foreach (var key in req.Headers.AllKeys)
            {
                sb.Append($@"             {key}: {req.Headers[key]}
");
            }
            return sb.ToString();


        }  
    }
}
