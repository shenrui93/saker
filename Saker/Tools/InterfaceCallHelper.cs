
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace Saker.Tools
{
    /// <summary>
    /// 
    /// </summary>
    public static class InterfaceCallHelper
    {
        const int MaxTryCount = 2;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="interfaceType"></param> 
        /// <param name="instanceMethod"></param>
        /// <param name="maxTryCount"></param>
        /// <returns></returns>
        public static Type DynamicCreateType(Type interfaceType, MethodInfo instanceMethod, int maxTryCount = MaxTryCount)
        {
            if (maxTryCount <= 0)
                throw new ArgumentException("参数" + nameof(maxTryCount) + "的值不能小于或者等于 0 ");

            //var assemblyName = "testAssembly";
            //var typeName = "testType";
            var assemblyName = Guid.NewGuid().ToString("n");
            var typeName = Guid.NewGuid().ToString("n");

            //动态创建程序集
            AssemblyName DemoName = new AssemblyName(assemblyName);
            AssemblyBuilder dynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(DemoName, AssemblyBuilderAccess.RunAndCollect);
            //动态创建模块 
            ModuleBuilder mb = dynamicAssembly.DefineDynamicModule(DemoName.Name, DemoName.Name + ".dll");

            //动态创建类MyClass 
            TypeBuilder tb = mb.DefineType(typeName, TypeAttributes.Public, null, new Type[] { interfaceType });
            //插入接口的实现申明
            tb.AddInterfaceImplementation(interfaceType);

            //动态创建构造函数  
            ConstructorBuilder ctor = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            //生成指令 
            ILGenerator ilg = ctor.GetILGenerator();//生成 Microsoft 中间语言 (MSIL) 指令

            //加载第一个参数
            ilg.Emit(OpCodes.Ldarg_0);
            //调用基类的方法
            ilg.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
            ilg.Emit(OpCodes.Ret);

            var methods = interfaceType.GetMethods();
            //动态创建接口方法
            foreach (var method in methods)
            {
                //final hidebysig newslot virtual
                var bind = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
                var dm = tb.DefineMethod(method.Name, bind, method.ReturnType, Type.EmptyTypes);

                var paramsInfo = SetMethodParams(dm, method.GetParameters());

                var dmil = dm.GetILGenerator();

                var retType = method.ReturnType;
                LocalBuilder retValue = null;
                //方法返回值
                if (retType != null && retType != typeof(void))
                    retValue = dmil.DeclareLocal(retType);
                LocalBuilder trycount = dmil.DeclareLocal(typeof(int));
                var loopLabel = dmil.DefineLabel();

                var endofLable = dmil.DefineLabel();
                var endtryLable = dmil.DefineLabel();

                var looptrue = dmil.DefineLabel();

                //将尝试次数写入变量
                dmil.Emit(OpCodes.Ldc_I4, maxTryCount);
                dmil.Emit(OpCodes.Stloc_S, trycount);

                if (maxTryCount >= 1)
                {
                    #region 循环块

                    dmil.MarkLabel(loopLabel);
                    dmil.Emit(OpCodes.Nop);

                    #region 方法的Cache块

                    dmil.BeginExceptionBlock();

                    CallRunMethod(dmil, instanceMethod, interfaceType, retValue, paramsInfo, method);
                    dmil.Emit(OpCodes.Leave, endofLable);
                    dmil.BeginCatchBlock(typeof(System.Exception));


                    #endregion
                    //插入循环块
                    dmil.Emit(OpCodes.Pop);
                    dmil.Emit(OpCodes.Ldloc_S, trycount);
                    dmil.Emit(OpCodes.Ldc_I4_1);
                    dmil.Emit(OpCodes.Sub);
                    dmil.Emit(OpCodes.Dup);
                    dmil.Emit(OpCodes.Stloc_S, trycount);
                    dmil.Emit(OpCodes.Ldc_I4_0);
                    dmil.Emit(OpCodes.Bgt_S, looptrue);

                    dmil.Emit(OpCodes.Rethrow);
                    dmil.MarkLabel(looptrue);
                    dmil.Emit(OpCodes.Leave_S, loopLabel);

                    dmil.EndExceptionBlock();

                    #endregion
                }

                dmil.MarkLabel(endofLable);
                if (retValue != null)
                    dmil.Emit(OpCodes.Ldloc_S, retValue);
                dmil.Emit(OpCodes.Ret);
            }


            Type classType = tb.CreateType();

            //保存动态创建的程序集 (程序集将保存在程序目录下调试时就在Debug下) 
            //dynamicAssembly.Save(DemoName.Name + ".dll");

            //创建类

            return classType;

        }



        private static Type[] SetMethodParams(MethodBuilder dm, ParameterInfo[] parameterInfo)
        {
            var paramsInfo = parameterInfo.Select(p => p.ParameterType).ToArray();
            dm.SetParameters(paramsInfo);
            int i = 0;
            foreach (var r in parameterInfo)
            {
                i++;
                var attr = ParameterAttributes.None;
                if (r.IsOut)
                    attr |= ParameterAttributes.Out;
                if (r.IsIn)
                    attr |= ParameterAttributes.In;

                var pb = dm.DefineParameter(i, attr, r.Name);
            }
            return paramsInfo;
        }
        private static void CallRunMethod(ILGenerator dmil, MethodInfo instanceMethod, Type instanceType, LocalBuilder retValue, Type[] paramsInfo, MethodInfo method)
        {
            #region 方法调用

            dmil.Emit(OpCodes.Call, instanceMethod);
            int i = 0;
            int paramsCount = paramsInfo.Length;
            while (++i <= paramsCount)
            {
                //加载参数
                dmil.Emit(OpCodes.Ldarg_S, i);
            }
            dmil.Emit(OpCodes.Callvirt, instanceType.GetMethod(method.Name, paramsInfo));
            //调用方法 
            if (retValue != null)
                dmil.Emit(OpCodes.Stloc_S, retValue);

            #endregion
        }
    }
}
