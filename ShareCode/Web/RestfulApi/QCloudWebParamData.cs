/***************************************************************************
 * 
 * 创建时间：   2017/6/8 14:36:23
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   腾讯云接口签名支持类
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
    /// 腾讯云接口签名支持类
    /// </summary>
    public class QCloudWebParamData : TypeWebParamDataBase<QCloudWebParamData>
    {
        /// <summary>
        /// 
        /// </summary>
        public QCloudWebParamData()
            : base(false)
        {

            this.Region = "sh";
            this.Nonce = new Random().Next(1, int.MaxValue).ToString();
        }


        /*

          Action 	            方法名 	DescribeInstances
          SecretId 	            密钥Id 	AKIDz8krbsJ5yKBZQpn74WFkmLPx3gnPhESA
          Timestamp 	        当前时间戳 	1465185768
          Nonce 	            随机正整数 	11886
          Region 	            实例所在区域 	gz 

       */


        /// <summary>
        /// 方法名
        /// </summary>
        public string Action { get { return this[nameof(Action)]; } set { this[nameof(Action)] = value; } }
        /// <summary>
        /// 密钥Id
        /// </summary>
        public string SecretId { get { return this[nameof(SecretId)]; } set { this[nameof(SecretId)] = value; } }
        /// <summary>
        /// 时间戳
        /// </summary>
        public string Timestamp { get { return this[nameof(Timestamp)]; } set { this[nameof(Timestamp)] = value; } }
        /// <summary>
        /// 随机串
        /// </summary>
        public string Nonce { get { return this[nameof(Nonce)]; } set { this[nameof(Nonce)] = value; } }
        /// <summary>
        /// 接口区域
        /// </summary>
        public string Region { get { return this[nameof(Region)]; } set { this[nameof(Region)] = value; } }
        /// <summary>
        /// 请求签名
        /// </summary>
        public string Signature { get { return this[nameof(Signature)]; } private set { this[nameof(Signature)] = value; } }

        /// <summary>
        /// 
        /// </summary>
        public override IWebParamData MarkTimestamp()
        {
            this.Timestamp = ((long)(DateTime.Now - LocalUTCBegin).TotalSeconds).ToString();
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addsign"></param>
        /// <param name="secrets"></param>
        /// <returns></returns>
        public override string MarkSign(bool addsign, params string[] secrets)
        {

            var method = secrets[0];
            var url = secrets[1];
            var key = secrets[2];
            var signStr = $"{method}{url}?{this.ToUrlNoEncode(p => p != nameof(Signature))}";

            var s = System.Security.Cryptography.HMAC.Create();

            s.Key = System.Text.Encoding.UTF8.GetBytes(key);
            s.HashName = "SHA1";
             
            var sign = Convert.ToBase64String(s.ComputeHash(Encoding.UTF8.GetBytes(signStr)));

            if (addsign)
            {
                Signature = sign;
            }




            return sign;


        }



    }
}
