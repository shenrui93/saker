
using System;
using System.Collections.Concurrent;
using Saker.Extension;
using System.Net;
using Saker.Tools;

namespace Saker.Web
{ 
    /// <summary>
    /// 
    /// </summary>
    public class WebSession : IWebSessionState
    {

        static Saker.Tools.IServerTimerBase _t;
        static WebSession()
        {
            _t = TimerService.CreateServerTimer(Callback, 15 * 60);
            _t.Restart();
        }

        private static bool Callback(IServerTimerBase timer)
        {
            try
            {
                var now = DateTime.Now;
                foreach(var r in _cacheList)
                {
                    if (r.Value.Exprie <= now)
                    {
                        r.Value.Dispose();
                    }
                }
            }
            catch
            {

            }
            return true;
        }

        [ThreadStatic]
        private static WebSession currentSession;

        private const string SessionIdKey = "WebSession_Id";
        static ConcurrentDictionary<string, WebSession> _cacheList = new ConcurrentDictionary<string, WebSession>();
        ConcurrentDictionary<string, object> _sessionList = new ConcurrentDictionary<string, object>();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IWebSessionState GetWebSessionStateByContext(System.Web.HttpContext context)
        {
            IWebSessionState session = null;
            var sessionId = context.Request.Cookies[SessionIdKey]?.Value;
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = context.Response.Cookies[SessionIdKey]?.Value;
                if (string.IsNullOrEmpty(sessionId))
                {
                    sessionId = Guid.NewGuid().ToSimpleString();
                    session = GetWebSessionById(sessionId);
                    CacheCookies(sessionId, context.Response.Cookies);
                    return session;
                }
            }
            session = GetWebSessionById(sessionId);
            return session;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IWebSessionState GetWebSessionStateByContext(System.Web.HttpContextBase context)
        {
            IWebSessionState session = null;
            var sessionId = context.Request.Cookies[SessionIdKey]?.Value;
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = context.Response.Cookies[SessionIdKey]?.Value;
                if (string.IsNullOrEmpty(sessionId))
                {
                    sessionId = Guid.NewGuid().ToSimpleString();
                    session = GetWebSessionById(sessionId);
                    CacheCookies(sessionId, context.Response.Cookies);
                    return session;
                }
            }
            session = GetWebSessionById(sessionId);
            return session;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IWebSessionState GetWebSessionStateByContext(System.Net.HttpListenerContext context)
        {
            IWebSessionState session = null;
            var sessionId = context.Request.Cookies[SessionIdKey]?.Value;
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = context.Response.Cookies[SessionIdKey]?.Value;
                if (string.IsNullOrEmpty(sessionId))
                {
                    sessionId = Guid.NewGuid().ToSimpleString();
                    session =  GetWebSessionById(sessionId);
                    CacheCookies(sessionId,context.Response.Cookies);
                    return session;
                }
            }
            session = GetWebSessionById(sessionId); 
            return session;
        }




        private static void CacheCookies(string sessionId,CookieCollection cookies)
        {
            var c = cookies?.AddOrSet(SessionIdKey, sessionId);
            if (c != null)
            {
                c.HttpOnly = true;
            };
        }
        private static void CacheCookies(string sessionId, System.Web.HttpCookieCollection cookies)
        {
            var c = cookies?.AddOrSet(SessionIdKey, sessionId);
            if (c != null)
            {
                c.HttpOnly = true;
            };
        }
        private static IWebSessionState GetWebSessionById(string sessionId)
        {
            if (currentSession != null && currentSession.sessionId == sessionId)
                return currentSession;

            var now = DateTime.Now;
            WebSession o;
            _cacheList.TryGetValue(sessionId, out o);
            if (o == null)
            {
                o = new WebSession(sessionId);
                _cacheList.AddOrUpdate(sessionId, o, (k, v) => o);
            }
            if (o.Exprie > now)
            {
                o.RefshExpire();
                //没有过期
                currentSession = o;
                return o;
            }
            //已经过期
            o.Init();
            currentSession = o;
            return o;

        } 




        private void Init()
        {
            _sessionList.Clear();
            RefshExpire();
        }
        private WebSession(string id)
        {
            this.sessionId = id;
            Exprie = DateTime.Now.AddMinutes(20);
        }
        object IWebSessionState.this[string key]
        {
            get
            {
                object o;
                _sessionList.TryGetValue(key, out o);
                return o;

            }

            set
            {
                if (value == null)
                {
                    object o;
                    _sessionList.TryRemove(key, out o);
                    return;
                }
                _sessionList.AddOrUpdate(key, value, (f, v) => value);

            }
        }
        void IWebSessionState.Unload()
        {
            WebSession o;
            _cacheList.TryRemove(sessionId, out o);
        }
        private string sessionId = "";
        private DateTime Exprie;
        private void RefshExpire()
        {
            Exprie = DateTime.Now.AddMinutes(20);
        }
        /// <summary>
        /// 
        /// </summary>
        void Dispose()
        {
            WebSession i;
            _cacheList.TryRemove(this.sessionId,out i);
            _sessionList.Clear();
        }
    }

    /// <summary>
    /// 表示一个会话状态
    /// </summary>
    public interface IWebSessionState
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object this[string key]
        {
            get; set;
        }
        /// <summary>
        /// 卸载当前会话
        /// </summary>
        void Unload();
    }
}
