/***************************************************************************
 * 
 * 创建时间：   2017/5/17 15:09:45
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   微信支付接口签名和验签
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
    /// 微信支付接口签名和验签
    /// </summary>
    public class WeiXinPayWebParamData:WebParamData
    {
        /// <summary>
        /// 类WeiXinPayWebParamData的默认构造函数
        /// </summary>
        public WeiXinPayWebParamData()
        {
            //在这里实现对象的初始化操作
        }

        #region 数据反转换

        //数据反转换
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public new  static WeiXinPayWebParamData FromXml(string xml)
        {
            var m_values = new WeiXinPayWebParamData();
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
        public new static WeiXinPayWebParamData FromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return new WeiXinPayWebParamData();
            try
            {
                var J = Newtonsoft.Json.Linq.JObject.Parse(json);
                return J.ToObject<WeiXinPayWebParamData>();
            }
            catch (System.Exception)
            {
                return new WeiXinPayWebParamData();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public new static WeiXinPayWebParamData FromUrl(string url)
        {
            var param = new WeiXinPayWebParamData();
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addsign"></param>
        /// <param name="secrets"></param>
        /// <returns></returns>
        public override string MarkSign(bool addsign, params string[] secrets)
        { 
            /*
            
            说明：关于签名我们约定使用 sign_type 字段来指示数据该使用何种签名方式，目前支持 SHA1,MD5,SHA256 三种签名方式
                  其中 sign_type 字段的值必须全部是大写，并且不包含空格等其他字符
                  默认使用SHA1加密方式对数据生成签名
            
            */
            var secret = secrets[0];
            string signKeyName = "sign";

            if (secrets.Length >= 2)
            {
                signKeyName = secrets[1];
            }

            if (string.IsNullOrEmpty(signKeyName))
            {
                signKeyName = "sign";
            }


            var strA = this.ToUrlNoEncode(p => p != signKeyName); 

            var strB = $"{strA}&key={secret}";

            var sign = Saker.Security.Encryption.MD5Encrypt(strB).ToUpper();

            if (addsign)
                this[signKeyName] = sign;

            return sign;
             
        }

        public override bool VerifySign(params string[] secrets)
        {
            return base.VerifySign(secrets);
        }
    }
}
