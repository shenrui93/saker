/***************************************************************************
 * 
 * 创建时间：   2017/1/18 15:32:55
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供对网站的我配置数据的获取能力
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Saker.Web
{
    /// <summary>
    /// 提供对网站的我配置数据的获取能力
    /// </summary>
    public class WebConfigManager
    { 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetAppSettings(string name)
        {
            var str = System.Web.Configuration.WebConfigurationManager.AppSettings[name];
            if (string.IsNullOrEmpty(str))
            { 
                return "";
            } 
            return str;
        } 

    }
     
}
