/***************************************************************************
 * 
 * 创建时间：   2016/11/25 11:00:21
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   为API的动态生成处理提供处理基类
 * 
 * *************************************************************************/

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Linq;
using Saker.Extension;
using System.Data;
using System.Text;
using Saker.Web;
using System.Threading;

 
namespace Saker.Api
{ 

    /// <summary>
    /// 接口处理器管理类，提供接口初始化，创建等管理服务
    /// </summary>
    /// <typeparam name="TBase"></typeparam>
    public class APIProcesserManager<TBase> where TBase : class, IAPIProcesser
    { 
        /// <summary>
        /// 验证秘钥
        /// </summary>
        public virtual string PublicRsaKey =>"";
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckAuth(IWebParamData param, string url)
        {
            if (param == null) return false;

            if (param.CheckTimeOut(5 * 60))
            {
                return false;
            }
            if (!param.VerifySign(PublicRsaKey, url))
            {
                return false;
            }


            return true;

        } 
        static object root = new object();
        static bool isInit = false;


        #region SecrechKey

        const string SecrechKey = @"<div style=""text-align:center;margin-bottom:20px"">
        关键词: <input type=""text"" id=""keywords"" placeholder=""请输入搜索关键字"" style=""width:250px;height:20px; font-size:14px;border:1px solid #000"">
        <input type=""button"" value=""搜索"" onclick=""OnSecrchProcess()"" />
        <script type=""text/javascript"">
            function OnSecrchProcess() {
                var input = document.getElementById(""keywords"");
                if (!input) return;
                var kw = input.value;
                if (!kw) {
                    window.location = window.location.pathname;
                    return;
                }
                if (kw.length <= 0) {
                    window.location = window.location.pathname;
                    return;
                }
                var ref = window.location.pathname + ""?kw="" + encodeURIComponent(kw);
                window.location = ref;
            }
            var input = document.getElementById(""keywords"");

            input.onkeydown = function (event) {
                var e = event || window.event || arguments.callee.caller.arguments[0];

                //if (e && e.keyCode == 27) { // 按 Esc
                //    //要做的事情
                //}
                //if (e && e.keyCode == 113) { // 按 F2
                //    //要做的事情
                //}
                if (e && e.keyCode == 13) { // enter 键
                    //要做的事情
                    OnSecrchProcess();
                }
            };
            input.focus();
        </script>
    </div>";

        #endregion

        #region 接口文档


        /// <summary>
        /// 
        /// </summary> 
        public static string GetWriteDoc(string ApiPath, string kw)
        {
            var allapi = GetAllApiArray();


            if (!string.IsNullOrEmpty(kw))
            {
                kw = kw.ToLower();
                allapi = allapi.Where(p =>
                {
                    if (p.HandleName.IndexOf(kw) >= 0) return true;
                    if (p.APIMark.IndexOf(kw) >= 0) return true;
                    return false;
                }
                ).ToArray();
            }

            DataTable tb = new DataTable();

            tb.Columns.Add("接口地址");
            tb.Columns.Add("业务代码");
            tb.Columns.Add("接口说明");
            tb.Columns.Add("接口参数");

            foreach (var r in allapi.OrderBy(p => p.HandleName))
            {
                var newrow = tb.NewRow();
                var handlename = ApiPath + r.HandleName;
                var mark = r.APIMark;

                if (!string.IsNullOrEmpty(kw))
                {
                    newrow["接口地址"] = $@"<a target=""_blank"" style=""color:#00f"" href=""{handlename}"">{handlename.Replace(kw, ($"<span style=\"color:#f00\">{kw}</span>"))}</a>";
                    newrow["接口说明"] = mark.Replace(kw, ($"<span style=\"color:#f00\">{kw}</span>"));
                }
                else
                {
                    newrow["接口地址"] = $@"<a target=""_blank"" style=""color:#00f"" href=""{handlename}"">{handlename}</a>";
                    newrow["接口说明"] = r.APIMark;
                }
                newrow["接口参数"] = r.ParamRemark;
                newrow["业务代码"] = r.ApiCode;
                tb.Rows.Add(newrow);
            }

            return WriterTableInfo(tb, $"接口地址说明信息({kw})", new int[] { 400, 65, });

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="title"></param>
        /// <param name="size"></param>
        /// <param name="maxszie"></param>
        /// <param name="minsize"></param>
        /// <param name="getvalue">获取数据的方法</param>
        /// <returns></returns>
        static string WriterTableInfo(DataTable dt, string title, int[] size, int maxszie = 0, int minsize = 0, GetValueMethod getvalue = null)
        {
            getvalue = getvalue ?? GetValue;
            StringBuilder strb = new StringBuilder();

            size = size ?? new int[0];

            //写入网页标题
            strb.Append($@"<!DOCTYPE html><html><head>
<title>{title}</title>
<style>
div{{text-align:center}}
table{{border-collapse:collapse;border-spacing:0;border-left:1px solid #888;border-top:1px solid #888;background:#efefef;width:100%;margin:0 auto;
{(minsize <= 0 ? "" : $"min-width:{minsize}px;")}{(maxszie <= 0 ? "" : $"max-width:{maxszie}px;")}
}}
table tr:first-child{{text-align:center}}
th,td{{border-right:1px solid #888;border-bottom:1px solid #888;padding:5px 15px;word-break:break-all}}
th{{font-weight:bold;background:#ccc}}

 table.params_table{{margin:0;border-collapse:collapse;border-spacing:0;border-left:0px solid #888;border-top:0px solid #888;background:#efefef;width:auto}}
table.params_table td{{word-break:break-all;text-align:left;border-right:0px solid #888;border-bottom:0px solid #888;padding:0}}
table.params_table td:first-child{{padding-right:5px;text-align:right;word-break: normal;}} 
</style>
</head><body>");




            //写标题
            strb.Append($"<div><h2>{title}</h2></div>");
            strb.Append(SecrechKey);
            /*
            padding: 0 0 15px 0;font-size: 20px;font-weight: bold;
            */
            //写请求数量
            strb.Append($"<div style=\"padding: 0 0 15px 0;font-size: 20px;font-weight: bold;\"></div>");

            //开始写表
            strb.Append($"<table>");
            //写入列标题
            strb.Append($"<tr>");
            int index = 0;
            foreach (DataColumn dcol in dt.Columns)
            {
                strb.Append($"<td {(size.Length < index + 1 ? "" : $"style=\"width:{size[index++]}px\"")}>{dcol.ColumnName}</td>");
            }
            strb.Append($"</tr>");


            var cols = dt.Columns;
            //写入数据行
            foreach (DataRow dr in dt.Rows)
            {
                strb.Append($"<tr>");
                foreach (DataColumn col in cols)
                {
                    strb.Append($"<td>{getvalue(dr[col.ColumnName].ToString(), col)}</td>");
                }
                strb.Append($"</tr>");
            }
            strb.Append($"</table>");
            strb.Append($@"</body></html>");


            return strb.ToString();
        }
        static string GetValue(string value, DataColumn dr)
        {
            if (string.IsNullOrEmpty(value)) return "";
            value = value.Trim();
            return value;
        }




        #endregion

        #region 接口初始化相关方法

        static ConcurrentDictionary<string, IAPIProcesserCreator> _list = new ConcurrentDictionary<string, IAPIProcesserCreator>();

        /// <summary>
        /// 根据接口名称得到一个新的接口处理器
        /// </summary>
        /// <param name="handle_name"></param>
        /// <returns></returns>
        public static TBase GetAPIProcessor(string handle_name)
        {
            IAPIProcesserCreator creator;
            _list.TryGetValue(handle_name, out creator);
            return creator?.GetNewAPIProcessor();
        }
        /// <summary>
        /// 根据接口名称获取接口处理处理器的创建器对象
        /// </summary>
        /// <param name="handle_name">接口的名称</param>
        /// <returns></returns>
        IAPIProcesserCreator GetAPIProcesserCreator(string handle_name)
        {
            IAPIProcesserCreator creator;
            _list.TryGetValue(handle_name, out creator);
            return creator;
        }
        /// <summary>
        /// 获取所有接口数据集合
        /// </summary>
        /// <returns></returns>
        public static IAPIProcesser[] GetAllApiArray()
        {
            List<IAPIProcesser> l = new List<IAPIProcesser>();

            foreach (var r in _list)
            {
                l.Add(r.Value);
            }
            return l.ToArray();
        }

        #endregion

        #region 静态构建器初始化方法
        /// <summary>
        /// 
        /// </summary>
        public APIProcesserManager()
        {
            if (isInit) return;
            lock (root)
            {
                if (isInit) return;
                ApiInterfacesInializer();
                isInit = true;
            }



        }
        /// <summary>
        /// 
        /// </summary>
        public void ApiInterfacesInializer()
        {
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

            try
            {
                //获取所有已经加载到这个程序域的程序集
                var appAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assem in appAssemblies)
                {
                    try
                    {
                        BuilderForAssemblie(assem);
                    } 
                    catch
                    {
                        continue;
                    }
                }
            } 
            catch //(Exception)
            {
            }
        }

        void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            try
            {
                BuilderForAssemblie(args.LoadedAssembly);
            }
            catch (System.Exception ex)
            {
                SystemErrorProvide.OnSystemErrorHandleEvent(null, ex);
            }
        }
        void BuilderForAssemblie(Assembly assem)
        {

            if (assem == null) return;

            //获取这个程序集的所有类型信息
            try
            {
                var types = assem.GetTypes();

                foreach (var type in types)
                {
                    BuilderForType(type);
                }
            } 
            catch (System.Exception ex)
            {
                SystemErrorProvide.OnSystemErrorHandleEvent(null, ex);
            }

        }
        void BuilderForType(Type type)
        {
            if (type == null) return;
            if (!type.IsClass) return;
            if (type.IsAbstract) return;
            if (!type.IsSubclassOf(typeof(TBase))) return;

            try
            {
                var o = Activator.CreateInstance(typeof(APIProcesserManager<>.APIProcesserCreator<>).MakeGenericType(typeof(TBase), type)) as IAPIProcesserCreator;
                if (o == null) return; 
                _list[o.HandleName] = o;

            } 
            catch (System.Exception ex)
            {
                SystemErrorProvide.OnSystemErrorHandleEvent(null, ex);
            }
        }

        #endregion

        #region 接口创建器

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class APIProcesserCreator<T> : IAPIProcesserCreator
             where T : TBase
        {

            delegate T CreateMethod();
            private CreateMethod CreateHandle;
            void BuildCreatorHandle()
            {
                var gameType = typeof(T);
                var defaultCtor = gameType.GetConstructor(Type.EmptyTypes);
                if (defaultCtor == null)
                {
                    throw new System.Exception($"类型：{gameType.FullName}无参数的构造方法");
                }
                var method = new DynamicMethod("GetNewObject"
                    , gameType
                    , null
                    , this.GetType()
                    , true);

                //获取il编辑器
                var il = method.GetILGenerator();

                //变量声明
                var ret = il.DeclareLocal(gameType);

                il.Emit(OpCodes.Newobj, defaultCtor);
                il.Emit(OpCodes.Stloc, ret);
                il.Emit(OpCodes.Ldloc, ret);
                il.Emit(OpCodes.Ret);

                CreateHandle = (CreateMethod)method.CreateDelegate(typeof(CreateMethod));

            }
            private string _APIMark;
            private string _handleName;
            private string _paramRemark;
            private string _apiCode;

            /// <summary>
            /// 
            /// </summary>
            public APIProcesserCreator()
            {

                BuildCreatorHandle();

                var o = this.GetNewAPIProcessor();
                _APIMark = o.APIMark;
                _handleName = o.HandleName;
                _paramRemark = o.ParamRemark;
                _apiCode = o.ApiCode;

            }
            /// <summary>
            /// 接口的备注描述信息
            /// </summary>
            public string APIMark
            {
                get
                {
                    return _APIMark;
                }
            }
            /// <summary>
            /// 接口参数注释内容
            /// </summary>
            public string ParamRemark
            {
                get
                {
                    return _paramRemark;
                }
            }
            /// <summary>
            /// 接口的名称
            /// </summary>
            public string HandleName
            {
                get
                {
                    return _handleName;
                }
            }

            public string ApiCode => _apiCode;

            /// <summary>
            /// 得到一个新的接口处理器对象
            /// </summary>
            /// <returns></returns>
            public TBase GetNewAPIProcessor()
            {
                return CreateHandle();
            }

        }
        /// <summary>
        /// 向接口管理器公开接口处理器的创建器
        /// </summary>
        [TypeLibImportClass(typeof(APIProcesserCreator<>))]
        interface IAPIProcesserCreator : IAPIProcesser
        {
            /// <summary>
            /// 得到一个新的接口处理器对象
            /// </summary>
            /// <returns></returns>
            TBase GetNewAPIProcessor();
        }



        #endregion



    }

}

