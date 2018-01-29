/***************************************************************************
 * 
 * 创建时间：   2017/6/6 10:01:38
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供Wcf服务的操作类
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Saker.WcfServices
{
    /// <summary>
    /// 提供Wcf服务的操作类（预留，暂未实现）
    /// </summary>
    public class WcfServer
    {
        private ServiceHost Host;

        /// <summary>
        /// 提供服务的主机
        /// </summary>
        public WcfServer(int port, string serviceName, object serverType, Type face)
        {
            Host = new ServiceHost(serverType);
            System.ServiceModel.Channels.Binding httpbinding = new BasicHttpBinding();

        }
        /// <summary>
        /// /
        /// </summary>
        public void Start()
        {
            //if (Host.State != CommunicationState.Opened)
            //    Host?.Open();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            Host?.Close();

        }
    }
}
