 
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;

namespace Saker.Api.Web
{
    /// <summary>
    /// 
    /// </summary>
    public class RequestAsyncResult : IAsyncResult
    {
        private AsyncCallback _cb;
        Tools.IServerTimerBase _timer;
        const int TimeOut = 60;
        private bool _completedSynchronously = true;

        /// <summary>
        /// 
        /// </summary>
        public object AsyncState { get; internal set; }
        WaitHandle IAsyncResult.AsyncWaitHandle => null;
        /// <summary>
        /// 
        /// </summary>
        public bool CompletedSynchronously
        {
            get { return _completedSynchronously; }
            internal set
            { 
                _completedSynchronously = value; 
                _timer?.Dispose();
                if (_completedSynchronously) return; 
                _timer = Saker.Tools.TimerService.WaitRunHandle(RunComplete, TimeOut);

            }
        }

        bool IAsyncResult.IsCompleted => true;



        internal AsyncCallback Cb
        {
            get
            {
                return _cb;
            }
            set
            {
                _cb = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public HttpContext Context { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public void RunComplete()
        {
            _timer?.Dispose();
            try
            {
                Cb?.Invoke(this);
            }
            catch
            {

            }
        }
    }

}
