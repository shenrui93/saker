using System;

namespace Saker.Net
{
    /// <summary>
    /// 通讯组件通讯异常的提供程序
    /// </summary>
    internal static class SystemRunErrorPorvider
    { 

        internal static void CatchException(Exception ex)
        {
            Saker.SystemErrorProvide.OnSystemErrorHandleEvent(null, ex);
        }
    }
}
