using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Saker.Win32API
{
    /// <summary>
    /// 
    /// </summary>
    public static class SystemTools
    { 
        /// <summary>
        /// 设定，获取系统时间,SetSystemTime()默认设置的为UTC时间，比北京时间少了8个小时。
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        [DllImport("Kernel32.dll")]
        public static extern bool SetSystemTime(ref SYSTEMTIME time);
        /// <summary>
        ///  设定，获取系统时间,SetSystemTime()默认设置的为UTC时间，比北京时间少了8个小时。
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        [DllImport("Kernel32.dll")]
        public static extern bool SetLocalTime(ref SYSTEMTIME time);
        /// <summary>
        ///  设定，获取系统时间,SetSystemTime()默认设置的为UTC时间，比北京时间少了8个小时。
        /// </summary>
        /// <param name="time"></param>
        [DllImport("Kernel32.dll")]
        public static extern void GetSystemTime(ref SYSTEMTIME time);
        /// <summary>
        ///  设定，获取系统时间,SetSystemTime()默认设置的为UTC时间，比北京时间少了8个小时。
        /// </summary>
        /// <param name="time"></param>
        [DllImport("Kernel32.dll")]
        public static extern void GetLocalTime(ref SYSTEMTIME time);

        /// <summary>
        /// 检查当前进程的程序是否有系统管理员权限身份
        /// </summary>
        /// <returns></returns>
        public static bool IsAdministrator()
        {
            WindowsIdentity current = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator); 
        }

        public static bool RunAsAdministrator(string[] Args)
        {            
            //获得当前登录的Windows用户标示  
            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
            //判断当前登录用户是否为管理员  
            if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            {
                return true;
            }
            else
            {
                //创建启动对象  
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                //设置运行文件  
                startInfo.FileName = System.Windows.Forms.Application.ExecutablePath;
                //设置启动参数  
                startInfo.Arguments = String.Join(" ", Args);
                //设置启动动作,确保以管理员身份运行  
                startInfo.Verb = "runas";
                //如果不是管理员，则启动UAC  
                System.Diagnostics.Process.Start(startInfo);
                //退出  
                System.Windows.Forms.Application.Exit();
                return false;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEMTIME
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMilliseconds; 

        public void FromDateTime(DateTime dateTime)
        {
            wYear = (ushort)dateTime.Year;
            wMonth = (ushort)dateTime.Month;
            wDayOfWeek = (ushort)dateTime.DayOfWeek;
            wDay = (ushort)dateTime.Day;
            wHour = (ushort)dateTime.Hour;
            wMinute = (ushort)dateTime.Minute;
            wSecond = (ushort)dateTime.Second;
            wMilliseconds = (ushort)dateTime.Millisecond;
        } 

        public DateTime ToDateTime()
        {
            return new DateTime(wYear, wMonth, wDay, wHour, wMinute, wSecond);
        }
    }

}
