/***************************************************************************
 * 
 * 创建时间：   2017/2/8 17:44:32
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   IIS 操作方法集合
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;

namespace Saker.Web
{

    /// <summary>
    /// IIS 操作方法集合
    /// http://blog.csdn.net/ts1030746080/article/details/8741399
    /// </summary>
    public class IisWorker
    {
        private static string HostName = "localhost";

        /// <summary>
        /// 获取本地IIS版本
        /// </summary>
        /// <returns></returns>
        public static string GetIIsVersion()
        {
            try
            {
                DirectoryEntry entry = new DirectoryEntry("IIS://" + HostName + "/W3SVC/INFO");
                string version = entry.Properties["MajorIISVersionNumber"].Value.ToString();
                return version;
            }
            catch //(Exception se)
            {
                //说明一点:IIS5.0中没有(int)entry.Properties["MajorIISVersionNumber"].Value;属性，将抛出异常 证明版本为 5.0
                return string.Empty;
            }
        }
        /// <summary>
        /// 创建虚拟目录网站
        /// </summary>
        /// <param name="webSiteName">网站名称</param>
        /// <param name="physicalPath">物理路径</param>
        /// <param name="domainPort">站点+端口，如192.168.1.23:90</param>
        /// <param name="isCreateAppPool">是否创建新的应用程序池</param>
        /// <returns></returns>
        public static void CreateWebSite(string webSiteName, string physicalPath, string domainPort, bool isCreateAppPool)
        {
            var siteInfo = new WebSiteInfo()
            {
                BindingHost = new string[] { domainPort },
                Root = new SiteVirPathInfo()
                {
                    IsWebServer = true,
                    SchemaClassName = "IISWebVirtualDir",
                    SiteName = webSiteName,
                    SitePath = physicalPath
                },
                SiteName = webSiteName,
                SitePath = physicalPath,

            };

            CreateWebSite(siteInfo);

        }
        /// <summary>
        /// 使用站点配置信息创建新的站点
        /// </summary>
        /// <param name="o"></param>
        public static DirectoryEntry CreateWebSite(WebSiteInfo o)
        {
            return CreateWebSiteConsole(o, true);
        }
        /// <summary>
        /// 使用站点配置信息创建新的站点
        /// </summary>
        /// <param name="o"></param>
        /// <param name="addunc"></param>
        public static DirectoryEntry CreateWebSiteConsole(WebSiteInfo o, bool addunc)
        {

            string webSiteName = o.SiteName;
            var domainPort = o.BindingHost;
            var root_config = o.Root;
            if (root_config == null)
            {
                throw new System.Exception("网站配置root不完整");
            }
            var physicalPath = System.IO.Path.GetFullPath(root_config.SitePath);

            if (IsExistsWebSite(webSiteName))
            {
                throw new System.Exception($"指定的站点名称“{webSiteName}”已经存在");
            }

            DirectoryEntry root = new DirectoryEntry("IIS://" + HostName + "/W3SVC");
            //为新WEB站点查找一个未使用的ID
            int siteId = 1;
            foreach (DirectoryEntry e in root.Children)
            {
                if (e.SchemaClassName == "IIsWebServer")
                {
                    int id = Convert.ToInt32(e.Name);
                    if (id >= siteId) { siteId = id + 1; }
                }
            }
            // 创建WEB站点
            DirectoryEntry site = (DirectoryEntry)root.Invoke("Create", "IIsWebServer", siteId);
            Console.WriteLine($"开始创建站点  {webSiteName}");
            site.Invoke("Put", "ServerComment", webSiteName);
            site.Invoke("Put", "KeyType", "IIsWebServer");
            site.Invoke("Put", "ServerState", 2);
            site.Invoke("Put", "FrontPageWeb", 1);
            site.Invoke("Put", "ServerAutoStart", 1);
            site.Invoke("Put", "ServerSize", 1);
            site.Invoke("SetInfo");
            //设置绑定域名
            site.Properties["ServerBindings"].Value = domainPort;
            // 创建应用程序虚拟目录 


            DirectoryEntry siteVDir = site.Children.Add("Root", "IISWebVirtualDir");
            siteVDir.Properties["AppIsolated"][0] = 2;
            siteVDir.Properties["Path"][0] = physicalPath;
            siteVDir.Properties["AccessFlags"][0] = 513;
            siteVDir.Properties["FrontPageWeb"][0] = 1;
            siteVDir.Properties["AppRoot"][0] = "LM/W3SVC/" + siteId + "/Root";
            siteVDir.Properties["AppFriendlyName"][0] = "Root";

            if (!string.IsNullOrEmpty(root_config.UncUserName) && addunc)
            {
                siteVDir.Properties["UNCUserName"][0] = root_config.UncUserName;
                siteVDir.Properties["UNCPassword"][0] = root_config.UncPassword + "";
            }


            try
            {
                Console.WriteLine($"开始绑定应用程序池  {webSiteName}");

                DirectoryEntry apppools = new DirectoryEntry("IIS://" + HostName + "/W3SVC/AppPools");
                DirectoryEntry newpool = apppools.Children.Add(webSiteName, "IIsApplicationPool");
                newpool.Properties["ManagedPipelineMode"][0] = "0"; //0:集成模式 1:经典模式
                newpool.CommitChanges();
                siteVDir.Properties["AppPoolId"][0] = webSiteName;
            }
            catch (System.Exception)
            {
                SetAppToPool(webSiteName, webSiteName);
            }

            Console.WriteLine($"绑定应用程序池  {webSiteName} 完成");


            siteVDir.CommitChanges();
            site.CommitChanges();


            BindSiteVirPath(siteVDir, o.VirPaths, webSiteName, addunc);

            siteVDir.CommitChanges();
            return site;
        }

        private static void BindSiteVirPath(DirectoryEntry siteVDir, SiteVirPathInfo[] virPaths, string webSiteName, bool addunc)
        {

            if (virPaths == null || virPaths.Length <= 0) return;


            //追加虚拟目录

            foreach (var c in virPaths)
            {
                if (c == null) continue;

                Console.WriteLine($"追加虚拟目录  {webSiteName}-{c.SiteName}");
                DirectoryEntry vdir;

                
                //if (vdir == null)
                    vdir = siteVDir.Children.Add(c.SiteName, c.SchemaClassName);

                vdir.Properties["Path"][0] = System.IO.Path.GetFullPath(c.SitePath);
                if (c.IsWebServer)
                {
                    vdir.Properties["AppIsolated"][0] = 2;
                    try
                    {
                        Console.WriteLine($"该虚拟目录是一个应用程序！开始绑定应用程序池  {siteVDir.Name}-{c.SiteName}");

                        DirectoryEntry apppools = new DirectoryEntry("IIS://" + HostName + "/W3SVC/AppPools");
                        DirectoryEntry newpool = apppools.Children.Add($"{webSiteName}-{c.SiteName}", "IIsApplicationPool");
                        newpool.Properties["ManagedPipelineMode"][0] = "0"; //0:集成模式 1:经典模式
                        newpool.CommitChanges();
                        vdir.Properties["AppPoolId"][0] = $"{webSiteName}-{c.SiteName}";
                    }
                    catch (System.Exception)
                    {
                        vdir.Properties["AppPoolId"][0] = $"{webSiteName}-{c.SiteName}";
                    }
                }
                if (!string.IsNullOrEmpty(c.UncUserName) && addunc)
                {
                    vdir.Properties["UNCUserName"][0] = c.UncUserName;
                    vdir.Properties["UNCPassword"][0] = c.UncPassword + "";
                }
                vdir.CommitChanges();

                BindSiteVirPath(vdir, c.Childrens, $"{webSiteName}-{c.SiteName}", addunc);


                vdir.CommitChanges();

                Console.WriteLine($"追加虚拟目录  {c.SiteName} 完成");

            }
        }
        private static SiteVirPathInfo[] GetSiteVirPathInfo(DirectoryEntry dirEntity,string baseDir)
        {
            if (dirEntity == null) return new SiteVirPathInfo[0];
            return dirEntity.Children
                .Cast<DirectoryEntry>()
                .Where(p => !p.Name.Equals("root", StringComparison.OrdinalIgnoreCase) && p.SchemaClassName == "IIsWebVirtualDir")
                .Select(p =>
                {
                    var pros = p.Properties;
                    return new SiteVirPathInfo()
                    {
                        SiteName = p.Name,
                        SitePath = Saker.IO.FileHelper.GetRelativePath(baseDir, pros["Path"][0].ToString()),
                        UncPassword = pros["UNCPassword"]?.Value + "",
                        UncUserName = pros["UNCUserName"]?.Value + "",
                        SchemaClassName = p.SchemaClassName,
                        IsWebServer = p.Properties["AppIsolated"]?.Value + "" == "0",
                        Childrens = GetSiteVirPathInfo(p,baseDir)
                    };
                }).ToArray();
        } 
        /// <summary>
        /// 获取一个列表表示站点列表信息
        /// </summary>
        /// <returns></returns>
        public static WebSiteInfo[] GetWebSites(string baseDir = null)
        {
            DirectoryEntry root = new DirectoryEntry("IIS://" + HostName + "/W3SVC");

            List<WebSiteInfo> webSiteList = new List<WebSiteInfo>();

            foreach (DirectoryEntry r in root.Children)
            {
                if (r == null) continue;
                if (r.SchemaClassName != "IIsWebServer") continue;
                {
                    var properties = r.Properties;
                    var siteroot = r.GetWebsiteRoot();
                    var o = new WebSiteInfo();
                    {
                        //ServerBindings
                        o.SiteId = int.Parse(r.Name);
                        o.SiteName = properties["ServerComment"].Value.ToString();
                        o.SitePath = Saker.IO.FileHelper.GetRelativePath(baseDir, GetWebsitePhysicalPath(r));
                        var objectArr = properties["ServerBindings"]?.Value;

                        if (IsArray(objectArr))//如果有多个绑定站点时
                        {
                            var objectToArr = ((object[])objectArr).Select(p => p + "").ToArray();
                            o.BindingHost = objectToArr;
                        }
                        else//只有一个绑定站点
                        {
                            o.BindingHost = new string[] { (objectArr + "") };
                        }


                        properties = siteroot.Properties;
                        o.Root = new SiteVirPathInfo()
                        {
                            SiteName = siteroot.Name,
                            SitePath = o.SitePath,
                            UncPassword = properties["UNCPassword"]?.Value + "",
                            UncUserName = properties["UNCUserName"]?.Value + "",
                            SchemaClassName = siteroot.SchemaClassName,
                            IsWebServer = true,
                        };
                        o.VirPaths = GetSiteVirPathInfo(siteroot,baseDir);
                    };

                    webSiteList.Add(o);
                }
            }

            return webSiteList.ToArray();
        }



        /// <summary>
        /// 检查指定的站点名称是否已经存在
        /// </summary>
        /// <param name="siteName">需要检查的站点名称</param>
        /// <returns></returns>
        public static bool IsExistsWebSite(string siteName)
        {
            foreach (var r in GetWebSites())
            {
                if (string.Equals(r.SiteName, siteName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 得到网站的物理路径
        /// </summary>
        /// <param name="rootEntry">网站节点</param>
        /// <returns></returns>
        public static string GetWebsitePhysicalPath(DirectoryEntry rootEntry)
        {
            foreach (DirectoryEntry childEntry in rootEntry.Children)
            {
                if ((childEntry.SchemaClassName != "IIsWebVirtualDir") || (childEntry.Name.ToLower() != "root"))
                    continue;
                if (childEntry.Properties["Path"].Value != null)
                {
                    return childEntry.Properties["Path"].Value.ToString();
                }
            }
            return "";
        }
        /// <summary>
        /// 获取站点名
        /// </summary>
        public static List<IisInfo> GetServerBindings()
        {
            List<IisInfo> iisList = new List<IisInfo>();
            string entPath = String.Format("IIS://{0}/w3svc", HostName);
            DirectoryEntry ent = new DirectoryEntry(entPath);
            foreach (DirectoryEntry child in ent.Children)
            {
                if (child.SchemaClassName.Equals("IIsWebServer", StringComparison.OrdinalIgnoreCase))
                {
                    if (child.Properties["ServerBindings"].Value != null)
                    {
                        object objectArr = child.Properties["ServerBindings"].Value;
                        string serverBindingStr = string.Empty;
                        if (IsArray(objectArr))//如果有多个绑定站点时
                        {
                            object[] objectToArr = (object[])objectArr;
                            serverBindingStr = objectToArr[0].ToString();
                        }
                        else//只有一个绑定站点
                        {
                            serverBindingStr = child.Properties["ServerBindings"].Value.ToString();
                        }
                        IisInfo iisInfo = new IisInfo();
                        iisInfo.DomainPort = serverBindingStr;
                        iisInfo.AppPool = child.Properties["AppPoolId"].Value.ToString();//应用程序池
                        iisList.Add(iisInfo);
                    }
                }
            }
            return iisList;
        }
        /// <summary>
        /// 创建应用池名称
        /// </summary>
        /// <param name="appPoolName">应用池名称</param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public static bool CreateAppPool(string appPoolName, string username, string password)
        {
            try
            {
                //创建一个新程序池
                DirectoryEntry apppools = new DirectoryEntry("IIS://" + HostName + "/W3SVC/AppPools");
                var newpool = apppools.Children.Add(appPoolName, "IIsApplicationPool");

                //设置属性 访问用户名和密码 一般采取默认方式
                newpool.Properties["WAMUserName"][0] = username;
                newpool.Properties["WAMUserPass"][0] = password;
                newpool.Properties["AppPoolIdentityType"][0] = "3";
                newpool.CommitChanges();
                return true;
            }
            catch // (Exception ex) 
            {
                return false;
            }
        }
        /// <summary>
        /// 建立程序池后关联相应应用程序及虚拟目录
        /// </summary>
        public static void SetAppToPool(string appname, string poolName)
        {
            DirectoryEntry getsite;
            GetWebSiteId(appname, out getsite);
            getsite.Properties["AppPoolId"][0] = poolName;
            getsite.CommitChanges();
        }
        /// <summary>
        /// 判断object对象是否为数组
        /// </summary>
        public static bool IsArray(object o)
        {
            return o is Array;
        }

        /// <summary>
        /// 获取指定站点的站点id
        /// </summary>
        /// <param name="webSiteName">站点名称</param>
        /// <param name="siteroot"></param>
        /// <returns></returns>
        public static string GetWebSiteId(string webSiteName, out DirectoryEntry siteroot)
        {

            DirectoryEntry root = new DirectoryEntry("IIS://" + HostName + "/W3SVC");

            List<WebSiteInfo> webSiteList = new List<WebSiteInfo>();

            foreach (DirectoryEntry r in root.Children)
            {
                if (r == null) continue;
                if (r.SchemaClassName != "IIsWebServer") continue;
                {
                    var properties = r.Properties;
                    var siteId = r.Name;
                    var siteName = properties["ServerComment"].Value.ToString();
                    if (siteName.Equals(webSiteName, StringComparison.OrdinalIgnoreCase))
                    {
                        siteroot = r;
                        return siteId;
                    }
                }
            }
            siteroot = null;
            return "";
        }
        /// <summary>
        /// 获取指定应用程序池的id
        /// </summary> 
        /// <param name="poolname"></param> 
        /// <returns></returns>
        public static DirectoryEntry GetAppPoolInfo(string poolname)
        {
            //创建一个新程序池
            DirectoryEntry apppools = new DirectoryEntry("IIS://" + HostName + "/W3SVC/AppPools");
            foreach (DirectoryEntry e in apppools.Children)
            {
                if (e.Name.Equals(poolname))
                {
                    return e;
                }
            }

            return null;
        }
        /// <summary>
        /// 删除指定服务器站点信息
        /// </summary>
        /// <param name="webSiteName"></param>
        /// <returns></returns>
        public static bool RemoveWebSite(string webSiteName)
        {
            try
            {
                DirectoryEntry root = new DirectoryEntry("IIS://" + HostName + "/W3SVC");
                DirectoryEntry siteInfo;
                GetWebSiteId(webSiteName, out siteInfo);
                if (siteInfo == null) return false;
                root.Children.Remove(siteInfo);
                root.CommitChanges();
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="poolname"></param>
        /// <returns></returns>
        public static bool RemoveAppPoolInfo(string poolname)
        {
            try
            {
                var apppools = new DirectoryEntry("IIS://" + HostName + "/W3SVC/AppPools");
                var pool = apppools.Children.Cast<DirectoryEntry>().FirstOrDefault(e => e.Name.Equals(poolname));
                if (pool == null) return false;
                apppools.Children.Remove(pool);
                apppools.CommitChanges();
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }



    #region 结构定义

    /// <summary>
    /// 表示IIS信息
    /// </summary>
    public class IisInfo
    {
        /// <summary>
        /// 获取应用池名称
        /// </summary>
        public string AppPool { get; internal set; }
        /// <summary>
        /// 获取域名端口
        /// </summary>
        public string DomainPort { get; internal set; }
    }
    /// <summary>
    /// 表示网站站点信息
    /// </summary>
    public class WebSiteInfo
    {
        /// <summary>
        /// 站点编号
        /// </summary>
        public int SiteId;
        /// <summary>
        /// 网站根目录信息
        /// </summary>
        public SiteVirPathInfo Root;
        /// <summary>
        /// 指示站点的虚拟目录结构数组
        /// </summary>
        public SiteVirPathInfo[] VirPaths;
        /// <summary>
        /// 站点名称
        /// </summary>
        public string SiteName;
        /// <summary>
        /// 站点根目录
        /// </summary>
        public string SitePath;
        /// <summary>
        /// 绑定地址
        /// </summary>
        public string[] BindingHost;
    }
    /// <summary>
    /// 网站虚拟目录结构信息
    /// </summary>
    public class SiteVirPathInfo
    {
        /// <summary>
        /// 站点名称
        /// </summary>
        public string SiteName;
        /// <summary>
        /// 站点路径
        /// </summary>
        public string SitePath;
        /// <summary>
        /// 文件路径连接使用用户名
        /// </summary>
        public string UncUserName;
        /// <summary>
        /// 文件路径连接使用的用户密码
        /// </summary>
        public string UncPassword;
        /// <summary>
        /// 虚拟目录架构信息
        /// </summary>
        public string SchemaClassName;
        /// <summary>
        /// 
        /// </summary>
        public bool IsWebServer;
        /// <summary>
        /// 
        /// </summary>
        public SiteVirPathInfo[] Childrens;
    }
    /// <summary>
    /// 扩展服务类
    /// </summary>
    public static class __
    {
        /// <summary>
        /// 查找Iis目录实体
        /// </summary>
        /// <param name="pros"></param>
        /// <param name="ep"></param>
        /// <returns></returns>
        public static DirectoryEntry FindDirectoryEntry(this DirectoryEntries pros, Func<DirectoryEntry, bool> ep)
        {
            return pros.Cast<DirectoryEntry>().FirstOrDefault(ep);
        }
        /// <summary>
        /// 获取目录属性信息
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static string GetPropertyInfo(this PropertyCollection properties)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string proName in properties.PropertyNames)
            {
                sb.AppendLine($"{proName}: {properties[proName].Value}");
            }

            return sb.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootEntry"></param>
        /// <returns></returns>
        public static DirectoryEntry GetWebsiteRoot(this DirectoryEntry rootEntry)
        {
            return rootEntry.Children.Cast<DirectoryEntry>().FirstOrDefault(childEntry => (childEntry.SchemaClassName == "IIsWebVirtualDir") && (childEntry.Name.Equals("root", StringComparison.OrdinalIgnoreCase)));
        }
    }

    #endregion


}
