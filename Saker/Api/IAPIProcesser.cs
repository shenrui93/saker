/***************************************************************************
 * 
 * 创建时间：   2018/1/11 13:47:49
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   实现接口管理器定义的管理接口
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
    /// 实现接口管理器定义的管理接口
    /// </summary>
    public interface IAPIProcesser
    {
        /// <summary>
        /// 接口的备注描述信息
        /// </summary>
        string APIMark { get; }
        /// <summary>
        /// 接口的名称
        /// </summary>
        string HandleName { get; }
        /// <summary>
        /// 接口参数注释内容
        /// </summary>
        string ParamRemark { get; }
        /// <summary>
        /// 接口业务代码，如果接口需要控制接口权限，请返回系统唯一业务代码。系统会自动检查系统接口的业务代码唯一性
        /// </summary>
        string ApiCode { get; }

    }


}
