
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using Saker.Web;

namespace Saker.Api.Web
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class WebApiBaseMethodBase : IAPIProcesser
    {

        /// <summary>
        /// 指示当前接口是否缓存错误请求
        /// </summary>
        public virtual bool IsCacheError => true;
        /// <summary>
        ///缓存错误请求的时间（单位秒）
        /// </summary>
        public virtual int CacheTime => 20;

        bool _isSetCache = false;

        /// <summary>
        /// 
        /// </summary>
        public abstract void ProcessRequest();
        /// <summary>
        /// 获取当前客户端的原始IP地址
        /// </summary>
        public abstract string UserHostAddress { get; }

        //以下提供一个基本的请求处理方法
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorcode"></param>
        /// <param name="message"></param>
        public virtual void WriteFailJson(int errorcode, string message)
        {
            CacheError();
            WriterString(ContextType.Json, $@"{{""status"":""fail"",""msg"":{GetJsonString(message)}}}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public virtual void WriteFailJson(string message)
        { 
            CacheError();
            WriterString(ContextType.Json, $@"{{""status"":""fail"",""msg"":{GetJsonString(message)}}}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        public virtual void WriteJson(object o)
        {
            WriterString(ContextType.Json, GetJsonString(o));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public virtual void WriteSuccessJson(string message)
        {
            WriterString(ContextType.Json, $@"{{""status"":""success"",""msg"":{GetJsonString(message)}}}");

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public virtual void WriteShowBoxJson(string message)
        { 
            WriterString(ContextType.Json, $@"{{""status"":""showbox"",""msg"":{GetJsonString(message)}}}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public virtual void WriteShowTipsJson(string message)
        { 
            WriterString(ContextType.Json, $@"{{""status"":""showtip"",""msg"":{GetJsonString(message)}}}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public virtual string GetJsonString(object o)
        {
            return Serialization.Json.JsonHelper.ToZipJson(o);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        public void WriterString(string result)
        {
            WriterString(ContextType.Unknown, result);
        }
        /// <summary>
        /// 
        /// </summary> 
        protected abstract void WriterString(ContextType type, string result);


        /// <summary>
        /// 获取当前方法调用堆栈信息
        /// </summary>
        public virtual string StackTraceInfo
        {
            get
            {
                var sb = new StringBuilder();

                var stackinfo = new System.Diagnostics.StackTrace(true).GetFrames();
                foreach (var st in stackinfo)
                {
                    var filename = st.GetFileName();
                    var fileLineNumber = st.GetFileLineNumber();
                    if (filename != null)
                        sb.AppendLine($@"在 {st.GetMethod()} 位置：{filename} 行号：{fileLineNumber}");
                    else
                    {
                        sb.AppendLine($@"在 {st.GetMethod()}");
                    }
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 接口注释
        /// </summary>
        public abstract string APIMark { get; }
        /// <summary>
        /// 接口名称
        /// </summary>
        public abstract string HandleName { get; }
        /// <summary>
        /// 获取接口参数信息
        /// </summary>
        public virtual string ParamRemark
        {
            get
            {
                var o = getParamMark();
                if (o.Count <= 0)
                {
                    return "该接口无参数";
                }
                StringBuilder strb = new StringBuilder();
                strb.Append("<div><table class=\"params_table\">");
                foreach (var r in o)
                {
                    strb.Append("<tr><td>");
                    strb.Append($@"<span style=""color:#45AF4E"">{r.Key}</span>:");
                    strb.Append("</td><td>");
                    strb.Append(r.Value);
                    strb.Append("</td></tr>");
                }
                strb.Append("</table></div>");
                return strb.ToString();

            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string ApiCode => null;


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual IWebParamData getParamMark()
        {
            return new WebParamData();
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsAsyncApi => false;
        /// <summary>
        /// 
        /// </summary>
        protected virtual void DoAsyncComplete()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxAge"></param> 
        public virtual void SetCacheMaxAge(int maxAge)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void CacheError()
        {
            if (IsCacheError)
                SetCacheMaxAge(CacheTime);
        }


    }
}
