/***************************************************************************
 * 
 * 创建时间：   2017/4/6 11:00:03
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   为 HttpContext 实现网站接口处理器基类
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using Saker.Extension;
using Saker.Tools;
using Saker.Web;

namespace Saker.Api.Web
{


    /// <summary>
    /// 为 HttpContext 实现网站接口处理器基类
    /// </summary>
    public abstract class SakerHttpContextHandleBase : WebApiBaseMethodBase
    {
        //[ThreadStatic]
        //private static HttpContext _context;
        private HttpContext _context;
        /// <summary>
        /// 
        /// </summary>
        public RequestAsyncResult AsyncResult { get; private set; }
        /// <summary>
        /// 指示当前的客户端请求是否是安全连接请求
        /// </summary>
        public virtual bool IsSsl
        {
            get; private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void ProcessRequest(HttpContext context, RequestAsyncResult iar)
        {
            try
            {
                this.AsyncResult = iar;
                HttpContext.Current = context;
                Initializer(context);
                SetCacheMaxAge(0);
            }

            catch (ThreadAbortException) { }
            catch (System.Exception ex)
            {
                WriteLog(ex.GetExceptionFormatString());
                Response.ClearContent();
                base.WriteFailJson(ex.Message);
                if (base.IsCacheError)
                    SetCacheMaxAge(base.CacheTime);
            }
        }

        /// <summary>
        /// 获取当前 HTTP 请求的 <see cref="System.Web.HttpRequest"/> 对象。
        /// </summary>
        public HttpRequest Request => _context.Request;
        /// <summary>
        /// 获取当前 HTTP 请求的 <see cref="System.Web.HttpResponse"/> 对象。
        /// </summary>
        public HttpResponse Response => _context.Response;
        /// <summary>
        /// 获取提供用于处理 Web 请求的方法的 System.Web.HttpServerUtility 对象。
        /// </summary>
        public HttpServerUtility Server => _context.Server;
        /// <summary>
        ///  为当前 HTTP 请求获取 System.Web.SessionState.HttpSessionState 对象。
        /// </summary>
        public HttpSessionState Session => _context.Session;
        /// <summary>
        /// 获取当前请求的上下文
        /// </summary>
        protected HttpContext Context => _context;
        /// <summary>
        /// 获取当前客户端的原始IP地址，会基于Http头获取代理的原始Ip
        /// </summary>
        public override string UserHostAddress
        {
            get
            {
                var ip = Request.Headers["X-Real-IP"];
                if (!string.IsNullOrEmpty(ip)) return ip;
                ip = Request.Headers["X-Forwarded-For"];
                if (!string.IsNullOrEmpty(ip)) return ip;
                return Request.UserHostAddress;
            }
        }
        /// <summary>
        /// 初始化当前的请求信息
        /// </summary> 
        protected virtual void Initializer(HttpContext context)
        {
            _context = context;
            var access = Request.Headers["Access-Control-Request-Headers"];
            if (!string.IsNullOrEmpty(access))
            {
                Response.Headers["Access-Control-Allow-Headers"] = access;
            }

            var isssl = Request.IsSecureConnection;
            if (isssl)
                IsSsl = true;
            var p = Request.Headers["X-Client-Proto"];
            IsSsl = p == "https";
        }

        /// <summary>
        /// 设置当前资源的客户端缓存时间
        /// </summary>
        /// <param name="maxAge"></param>
        /// <param name="nocheck"></param>
        public override void SetCacheMaxAge(int maxAge)
        {
            if (maxAge <= 0)
            {
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetMaxAge(TimeSpan.Zero);
                return;
            }
            Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.Cache.SetMaxAge(new TimeSpan(maxAge * TimeSpan.TicksPerSecond));
        }
        /// <summary>
        /// 终止当前请求，将当前的请求转换为get请求发起
        /// </summary>
        public virtual bool RedirectGet()
        {
            if (Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase)) return false;
            var param = Request.GetWebParam();
            RedirectUrl($"{Request.Path}?{param.ToUrl()}");
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        public virtual void RedirectUrl(string uri)
        {
            SetCacheMaxAge(0);
            Response.StatusCode = 303;
            Response.RedirectLocation = uri;
        }


        private void SetJsonContextType()
        {
            Response.ContentType = "application/json";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="result"></param>
        protected override void WriterString(ContextType type, string result)
        {
            switch (type)
            {
                case ContextType.Html: Response.ContentType = "text/html"; break;
                case ContextType.Json: Response.ContentType = "application/json"; break;
                case ContextType.JavaScript: Response.ContentType = "application/js"; break;
                case ContextType.Text: Response.ContentType = "text/plain"; break;
                default: break;
            }
            Response.Write(result);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void DoAsyncComplete()
        {
            this.AsyncResult?.RunComplete();
        }
        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="log"></param>
        protected virtual void WriteLog(string log)
        {

        }
    }





    /// <summary>
    /// 指定输出内容的类型
    /// </summary>
    public enum ContextType
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown,
        /// <summary>
        /// Json
        /// </summary>
        Json,
        /// <summary>
        /// 
        /// </summary>
        Html,
        /// <summary>
        /// 
        /// </summary>
        Text,
        /// <summary>
        /// 
        /// </summary>
        JavaScript,
    }
}
