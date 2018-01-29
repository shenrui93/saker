/***************************************************************************
 * 
 * 创建时间：   2017/8/30 8:55:39
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
using static Saker.Security.Encryption;

namespace Saker.Web.RestfulApi
{
    /// <summary>
    /// 为网站的
    /// </summary>
    public class WebParamDataV2 : TypeWebParamDataBase<ShenPayWebParamData>
    {
        const string default_sg_key = "sign";


        /// <summary>
        /// 类WeiXinPayWebParamData的默认构造函数
        /// </summary>
        public WebParamDataV2()
        {
            //在这里实现对象的初始化操作
        }
          
        /// <summary>
        /// 
        /// </summary>
        /// <param name="addsign"></param>
        /// <param name="secrets"></param>
        /// <returns></returns>
        public override string MarkSign(bool addsign, params string[] secrets)
        { 
            /*
            
            说明：关于签名我们约定使用 sign_type 字段来指示数据该使用何种签名方式，目前支持 SHA1,MD5 三种签名方式
                  其中 sign_type 字段的值必须全部是大写，并且不包含空格等其他字符
                  默认使用SHA1加密方式对数据生成签名
            
            */
            var url = secrets[0];
            var secret = secrets[1];
            var sign = ""; 
            string unsignStr = $"{url}?{this.ToUrlNoEncode(p => p != default_sg_key)}&{secret}"; 
            switch (WebSignType)
            {
                case SignType.MD5:
                    sign = MD5Encrypt(unsignStr, Encoding.UTF8); break; 
                default:
                    sign = Sha1Encrypt(unsignStr); break;
            }
            if (addsign)
                this[default_sg_key] = sign;
            return sign;

        } 

    }
}
