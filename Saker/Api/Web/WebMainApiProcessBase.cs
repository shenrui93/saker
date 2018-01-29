/***************************************************************************
 * 
 * 创建时间：   2017/8/29 16:07:08
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   为网站接口入口点提供基类
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading;
using Saker.Tools;
using Saker.Threading;
using Saker.Web;
using Saker.Extension;

namespace Saker.Api.Web
{

    /// <summary>
    /// 为网站接口入口点提供基类
    /// </summary>
    /// <typeparam name="TBase"></typeparam>
    public abstract class WebMainApiProcessBase<TBase> : APIProcesserManager<TBase>, IHttpAsyncHandler
        where TBase : SakerHttpContextHandleBase
    { 
        /// <summary>
        /// 输出动态接口文档
        /// </summary>
        /// <param name="context">请求上下文</param>
        /// <returns></returns>
        protected virtual bool FlushApiDoc(HttpContext context)
        {
            context.Response.StatusCode = 404;
            context.Response.Write("404");
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public static string ApiPath { get; set; }



        /// <summary>
        /// 指示接口是否允许重用
        /// </summary>
        public virtual bool IsReusable => false;
        /// <summary>
        /// 
        /// </summary>
        public abstract int PathLength { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiname"></param>
        /// <param name="context"></param>
        /// <param name="iar"></param>
        /// <returns>返回当前的请求是否是同步完成的如果是则为true,否则为false</returns>
        public virtual bool ProcessRequest(string apiname, HttpContext context, RequestAsyncResult iar)
        { 
            var _process = GetAPIProcessor(apiname);
            if (_process == null)
            {
                return FlushApiDoc(context);
            }
            try
            {
                _process.ProcessRequest(context, iar);
                return !_process.IsAsyncApi;
            }
            catch (ThreadAbortException)
            {
                return true;
            }
            catch (System.Exception ex)
            {
                WriteError(context, ex);
                return true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ex"></param>
        public static void WriteError(HttpContext context, System.Exception ex)
        {
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            WriteError(context, ex.Message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ex"></param>
        public static void WriteError(HttpContext context, string ex)
        {
            context?.Response?.Write(ex);
        }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
        }

        IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            var iar = new RequestAsyncResult();

            iar.Cb = cb;
            iar.AsyncState = extraData;
            iar.Context = context;
            iar.CompletedSynchronously = true;

            try
            {
                var path = context.Request.Path + "";

                if (path.StartsWith(ApiPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    path = path.Substring(PathLength);
                } 
                var isSync = ProcessRequest(path, context, iar);
  
                iar.CompletedSynchronously = isSync;
            }
            catch (System.Exception ex)
            {
                iar.CompletedSynchronously = true;
            } 
            return iar;

        }

        void IHttpAsyncHandler.EndProcessRequest(IAsyncResult result)
        {

        }
    }

}
