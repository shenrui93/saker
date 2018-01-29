/***************************************************************************
 * 
 * 创建时间：   2016/7/20 11:02:42
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   定义一个接口,该接口包含Http服务器的实现逻辑
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Saker.Net
{
    /// <summary>
    /// 定义一个接口,该接口包含Http服务器的实现逻辑
    /// </summary>
    public interface IWebServerListen : IDisposable
    {
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <returns></returns>
        bool Start();
        /// <summary>
        /// 停止服务器
        /// </summary>
        /// <returns></returns>
        bool Stop();
        /// <summary>
        /// 新请求的处理通知函数
        /// </summary>
        Saker.EventHandle<IWebServerRequestContext> NewRequest { get; set; }
    }
    /// <summary>
    ///  定义一个接口,该接口包含Http服务器处理请求的上下文信息
    /// </summary>
    public interface IWebServerRequestContext
    {
        /// <summary>
        /// 
        /// </summary>
        WebHeaderCollection Headers
        {
            get;
        }
        /// <summary>
        /// 
        /// </summary>
        string ContentType
        {
            get;
            set;
        }

        /// <summary>
        /// 当前请求的数入表单数据
        /// </summary>
        Stream InputStream { get; }


        /// <summary>
        /// 关闭并完成请求
        /// </summary>
        void Close();
        /// <summary>
        /// 请求上下文
        /// </summary>
        HttpListenerContext Context
        {
            get;
        }
        /// <summary>
        /// 表示请求的路径地址
        /// </summary>
        string Path { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        void Write(string value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        void WriteLine(string value);
        /// <summary>
        /// 将缓冲区内的数据写入基础流，并清空缓冲区
        /// </summary>
        void Flush();
        /// <summary>
        /// 获取请求中上传的POST提交数据
        /// </summary>
        /// <returns></returns>
        string GetPostData();
        /// <summary>
        /// 获取请求中上传的POST提交数据
        /// </summary>
        /// <returns></returns>
        Web.IWebParamData QueryString { get; }
        /// <summary>
        /// 获取请求中上传的POST提交数据
        /// </summary>
        /// <returns></returns>
        string GetPostData(System.Text.Encoding encoding);
        /// <summary>
        /// 获取玩家的请求参数
        /// </summary>
        /// <returns></returns>
        Saker.Web.IWebParamData GetWebDataParam();
        /// <summary>
        /// 写入json响应
        /// </summary>
        /// <param name="o"></param>
        void WriteJson(object o);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        void WriteRequestFail(string msg);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        void WriteRequestSuccess(string msg);
        /// <summary>
        /// 
        /// </summary>
        Web.IWebSessionState Session { get; }
    }

    /// <summary>
    /// 表示服务器的Session管理对象
    /// </summary>
    public interface IWebSessionContext
    {
        /// <summary>
        /// 获取或者设置Session的对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        object this[string name]
        {
            get;set;
        }
        /// <summary>
        /// 当前资源信息的最后读取或者写入时间
        /// </summary>
        DateTime LastWriterOrReadTime { get; }
        /// <summary>
        /// 当前Session的管理会话ID
        /// </summary>
        string SessionID { get; set; }


    }
}
