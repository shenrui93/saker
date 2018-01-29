﻿/***************************************************************************
 * 
 * 创建时间：   2017/9/11 12:45:46
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供非对称验证加密支持
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;

namespace Saker.Web.RestfulApi
{
    /// <summary>
    /// 提供非对称验证加密支持
    /// </summary>
    public class RsaAuthorizeParam : TypeWebParamDataBase<RsaAuthorizeParam>
    {

        public override string MarkSign(bool addsign, params string[] secrets)
        {
            /*
            
            说明：关于签名我们约定使用 sign_type 字段来指示数据该使用何种签名方式，目前支持 SHA1,MD5,SHA256 三种签名方式
                  其中 sign_type 字段的值必须全部是大写，并且不包含空格等其他字符
                  默认使用SHA1加密方式对数据生成签名
            
            */
            var secret = secrets[0];

            var strA = this.ToUrlNoEncode(p => p != "sign");

            Security.RSA.RSACryptoService rsa = new Security.RSA.RSACryptoService(secret);

            var sign = Convert.ToBase64String(rsa.SignDataSHA1(Encoding.UTF8.GetBytes(strA)));

            if (addsign)
                this["sign"] = sign;

            return sign;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="secrets"></param>
        /// <returns></returns>
        public override bool VerifySign(params string[] secrets)
        {
            try
            {
                var secret = secrets[0];

                var re_sign = Convert.FromBase64String(this["sign"]);

                var strA = this.ToUrlNoEncode(p => p != "sign");

                Security.RSA.RSACryptoService rsa = new Security.RSA.RSACryptoService(null, secret);

                var result = rsa.VerifyDataSHA1(Encoding.UTF8.GetBytes(strA), re_sign);
                return result;
            }
            catch
            {
                return false;
            }
        }




    }
}
