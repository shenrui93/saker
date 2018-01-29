/***************************************************************************
 * 
 * 创建时间：   2018/1/11 13:49:54
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   定义一个包含请求上下文需要实现的的接口
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saker.Api
{
    /// <summary>
    /// 定义一个包含请求上下文需要实现的的接口
    /// </summary>
    public interface IHttpContext
    {
        /// <summary>
        /// 请求的响应消息
        /// </summary>
        IHttpResponse Response { get; }
    }
    /// <summary>
    /// 
    /// </summary>
    public interface IHttpResponse
    {

    }
}
