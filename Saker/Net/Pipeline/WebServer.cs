/***************************************************************************
 * 
 * 创建时间：   2016/7/20 11:07:42
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供一个基础Http服务器的简单实现
 * 
 * *************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Saker.Web;

namespace Saker.Net.Pipeline
{

    #region WebServer

    /// <summary>
    /// 提供一个基础Http服务器的简单实现
    /// </summary>
    public class WebServer : IWebServerListen
    {
        long _isStarted = 0;
        long _isRecvice = 0;
        object root = new object();


        HttpListener _httpListen;
        private string[] prefixes;

        /// <summary>
        /// 初始化一个默认包含所有IP的指定端口的HTTP请求监听器
        /// </summary>
        public WebServer(int port) : this(new string[] { "http://+:" + port + "/" })
        {
        }
        /// <summary>
        /// 初始化一个默认包含所有IP的指定端口的HTTP请求监听器
        /// </summary>
        public WebServer(string host, int port) : this(new string[] { $"http://{host}:{ port }/" })
        {
        }
        /// <summary>
        /// 初始化一个默认包含所有IP的指定端口的HTTP请求监听器
        /// </summary>
        public WebServer(string[] prefixes)
        {
            this.prefixes = prefixes;
        }
        /// <summary>
        /// 包含一个新请求到达的处理函数
        /// </summary>
        public virtual EventHandle<IWebServerRequestContext> NewRequest { get; set; } = (s, e) => { e.Close(); };
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <returns></returns>
        public virtual bool Start()
        {
            Initializer();
            if (Interlocked.CompareExchange(ref _isStarted, 1, 0) != 0) return true;
            _httpListen.Start();
            Proncess_BeginGetContext(_httpListen);
            return true;
        }

        private void Initializer()
        {
            lock (root)
            {
                if (_httpListen == null)
                {
                    _httpListen = new HttpListener();
                    _httpListen.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
                    foreach (var r in prefixes)
                    {
                        _httpListen.Prefixes.Add(r);
                    }
                }
            }
        }

        private void Proncess_BeginGetContext(HttpListener httpListen)
        {
            if (Interlocked.Read(ref _isStarted) != 1) return;
            if (Interlocked.CompareExchange(ref _isRecvice, 1, 0) != 0) return;

            try
            {
                httpListen.BeginGetContext(iar =>
                {
                    HttpListenerContext context = null;
                    try
                    {
                        context = httpListen.EndGetContext(iar);
                    }
                    catch (System.Exception)
                    {
                        //this.Dispose();
                    }
                    if (Interlocked.CompareExchange(ref _isRecvice, 0, 1) == 1)
                    {
                        Proncess_BeginGetContext(httpListen);
                    }
                    if (context != null)
                    {
                        ThreadPool.UnsafeQueueUserWorkItem(r =>
                        {
                            try
                            {
                                var c = new WebServerRequestContext(context);
                                try
                                {
                                    Proncess_EndGetContext(c);
                                }
                                catch (System.Exception ex)
                                {
                                    c.WriteLine(GetErrorFormatString(ex));
                                    c.Close();
                                }
                            }
                            catch //(Exception ex)
                            {
                            }
                        }, null);
                    }
                }, null);
            }
            catch (System.Exception)
            {
                this.Dispose();
            }
        }
        private void Proncess_EndGetContext(WebServerRequestContext context)
        {
            context.Context.Response.ContentEncoding = Encoding.UTF8;
            context.Context.Response.Headers["charset"] = "utf-8";
            context.ContentType = "application/json";
            NewRequest(this, context);
            context.Flush();
            //结束处理，发送响应
            context.Close();
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        /// <returns></returns>
        public virtual bool Stop()
        {
            if (Interlocked.CompareExchange(ref _isStarted, 0, 1) != 1) return true;
            _httpListen?.Stop();
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            lock (root)
            {
                _httpListen?.Abort();
                _httpListen = null;
            }
        }

        static string GetErrorFormatString(System.Exception ex)
        {

            if (ex == null)
                return "";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("*********************异常文本*********************");
            sb.AppendLine("【出现时间】：" + DateTime.Now.ToString());

            sb.AppendLine("【异常类型】：" + ex.GetType().Name);
            sb.AppendLine("【异常信息】：" + ex.Message);
            sb.AppendLine("【堆栈调用】：" + ex.StackTrace);

            sb.AppendLine("******************************************************************");

            if (ex.InnerException != null)
            {
                sb.AppendLine(GetErrorFormatString(ex.InnerException));
            }

            return sb.ToString();
        }
    }


    #endregion

    #region WebServerRequestContext


    class WebServerRequestContext : IWebServerRequestContext
    {
        private HttpListenerContext context;
        private IO.StreamWriter writer;
        private WebHeaderCollection _header;
        private System.IO.MemoryStream ms = new MemoryStream();

        public WebHeaderCollection Headers { get { return _header; } }
        public string ContentType
        {
            get
            {
                return context.Response.Headers["Content-Type"];
            }
            set
            {
                context.Response.Headers["Content-Type"] = $"{value}; charset=utf-8";
            }
        }

        public IWebParamData QueryString
        {
            get
            {
                var q = context.Request.Url.Query.Trim('?');
                
                return WebParamData.FromUrl(q);
            }
        }

        public WebServerRequestContext(HttpListenerContext context)
        {
            this.context = context;
            this._header = context.Response.Headers;
            this.writer = new IO.StreamWriter(context.Response.OutputStream);
            InitializerInputStream(context);
        }

        private void InitializerInputStream(HttpListenerContext context)
        {
            var reqstream = context.Request.InputStream;
            if (reqstream == null) return;
            if (!reqstream.CanRead) return;
            byte[] data = new byte[1024];
            int read_count = 0;
            while ((read_count = reqstream.Read(data, 0, 1024)) > 0)
            {
                ms.Write(data, 0, read_count);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Path
        {
            get
            {
                var qs = this.context.Request.RawUrl;
                var f = qs.IndexOf('?');
                if (f > 0)
                {
                    return qs.Substring(0, f);
                }
                return qs;
            }
        }
        public virtual HttpListenerContext Context
        {
            get
            {
                return context;
            }
        }

        public Stream InputStream
        {
            get
            {
                return ms;
            }
        }

        public virtual IWebSessionState Session=> WebSession.GetWebSessionStateByContext(this.Context);

        public virtual void Close()
        {
            this.Flush();
            this.context?.Response?.Close();
        }
        public virtual void Write(string value)
        {
            this.writer.Write(value);
        }
        public virtual void WriteLine(string value)
        {
            this.Write(value + System.Environment.NewLine);
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void Flush()
        {
            this.writer.Flush();
        }
        /// <summary>
        /// 获取请求中上传的POST提交数据
        /// </summary>
        /// <returns></returns>
        public string GetPostData()
        {
            if (ms == null) return "";
            if (!ms.CanSeek) return "";
            ms.Position = 0;
            return (context.Request.ContentEncoding ?? Encoding.UTF8).GetString(ms.ToArray());
        }
        /// <summary>
        /// 获取请求中上传的POST提交数据
        /// </summary>
        /// <returns></returns>

        public string GetPostData(Encoding encoding)
        {
            if (ms == null) return "";
            if (!ms.CanSeek) return "";
            ms.Position = 0;
            return (encoding).GetString(ms.ToArray());
        }

        public IWebParamData GetWebDataParam()
        {
            string url;
            url = this.GetPostData();
            return Saker.Web.WebParamData.FromUrl(url);
        }

        /// <summary>
        /// 写入json响应
        /// </summary>
        /// <param name="o"></param>
        public void WriteJson(object o)
        {
            this.Write(Saker.Serialization.Json.JsonHelper.ToZipJson(o));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public void WriteRequestFail(string msg)
        {
            Write($"{{\"status\":\"fail\",\"msg\":\"{msg}\"}}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public void WriteRequestSuccess(string msg)
        {
            Write($"{{\"status\":\"success\",\"msg\":\"{msg}\"}}");
        }
    }


    #endregion


    #region 会话相关


    /// <summary>
    /// 会话相关
    /// </summary>
    public class WebSessionContextItem : IWebSessionContext, IDisposable
    {
        Dictionary<string, object> _list = new Dictionary<string, object>();

        /// <summary>
        /// 向Session管理器写入Session数据
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object this[string name]
        {
            get
            {
                object val;
                if (_list.TryGetValue(name, out val))
                {
                    LastWriterOrReadTime = DateTime.Now;
                    return val;
                }
                return null;
            }
            set
            {
                _list[name] = value;
                LastWriterOrReadTime = DateTime.Now;
            }
        }
        /// <summary>
        /// 表示当前Session对象的初始化
        /// </summary>
        public DateTime LastWriterOrReadTime { get; private set; } = DateTime.Now;
        /// <summary>
        ///  Session 会话ID
        /// </summary>
        public string SessionID
        {
            get;
            set;
        } = GetNewID();
        /// <summary>
        /// 获取一个新的 Session 会话ID
        /// </summary>
        /// <returns></returns>
        private static string GetNewID()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
        /// <summary>
        /// 释放当前会话对象
        /// </summary>
        public void Dispose()
        {
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class WebSessionContextCollections : ICollection<IWebSessionContext>
    {

        Dictionary<string, IWebSessionContext> _list = new Dictionary<string, IWebSessionContext>();
        /// <summary>
        /// 
        /// </summary>
        public int Count
        {
            get
            {
                return _list.Count;
            }
        }
        /// <summary>
        /// 指示当前的对象是否是只读的
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="item"></param>
        public void Add(IWebSessionContext item)
        {
            _list[item.SessionID] = item;
        }
        /// <summary>
        /// 清理数据
        /// </summary>
        public void Clear()
        {
            _list.Clear();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(IWebSessionContext item)
        {
            return _list.ContainsKey(item.SessionID);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(IWebSessionContext[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IWebSessionContext> GetEnumerator()
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(IWebSessionContext item)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

    }


    #endregion


}
