/***************************************************************************
 * 
 * 创建时间：   2017/5/27 9:38:48
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   未填写备注信息
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Saker.Web.RestfulApi
{
    /// <summary>
    ///  
    /// </summary>
    public abstract class TypeWebParamDataBase<T> : WebParamData
        where T :WebParamData,new()
    {

        /// <summary>
        /// 初始化一个默认实例
        /// </summary>
        public TypeWebParamDataBase() : base()
        {

        }
        /// <summary>
        /// 初始化一个新实例
        /// </summary>
        /// <param name="ignoreCase">表示当前是否忽略大小写</param>
        public TypeWebParamDataBase(bool ignoreCase) : base(ignoreCase) { }


        #region 数据反转换

        //数据反转换
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public new static T FromXml(string xml)
        {
            var m_values = new T();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);
                XmlNode xmlNode = xmlDoc.FirstChild;//获取到根节点<xml>
                XmlNodeList nodes = xmlNode.ChildNodes;
                foreach (XmlNode xn in nodes)
                {
                    XmlElement xe = (XmlElement)xn;
                    m_values[xe.Name] = xe.InnerText;
                }
                return m_values;
            }
            catch //(Exception ex)
            {
                return m_values;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public new static T FromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return new T();
            try
            {
                var J = Newtonsoft.Json.Linq.JObject.Parse(json);
                return J.ToObject<T>();
            }
            catch (System.Exception)
            {
                return new T();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public new static T FromUrl(string url)
        {
            var param = new T();
            try
            {
                int l = string.IsNullOrEmpty(url) ? 0 : url.Length;
                int i = 0;

                unsafe
                {
                    fixed (char* s = url)
                    {
                        while (i < l)
                        {
                            int si = i;
                            int ti = -1;

                            while (i < l)
                            {
                                char ch = s[i];

                                if (ch == '=')
                                {
                                    if (ti < 0)
                                        ti = i;
                                }
                                else if (ch == '&')
                                {
                                    break;
                                }
                                i++;
                            }
                            string name = null;
                            string value = null;

                            if (ti >= 0)
                            {
                                name = url.Substring(si, ti - si);
                                value = url.Substring(ti + 1, i - ti - 1);
                            }
                            else
                            {
                                value = url.Substring(si, i - si);
                                i++;
                                continue;
                            }
                            if (!string.IsNullOrEmpty(name) && !(string.IsNullOrEmpty(value)))
                                param.Add(name, WebHelper.UrlDecode(value));
                            i++;
                        }
                    }
                }
                return param;
            }
            catch (System.Exception)
            {

                return param;
            }
        }
        #endregion
 
    }
}
