/***************************************************************************
 * 
 * 创建时间：   2016/5/7 10:25:05
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供user32操作的帮助类
 * 
 * *************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Saker.Win32API
{
    /// <summary>
    /// 提供user32操作的帮助类
    /// </summary>
    public class user32
    {

        /// <summary>    
        /// 该函数检索一指定窗口的客户区域或整个屏幕的显示设备上下文环境的句柄，
        /// 以后可以在GDI函数中使用该句柄来在设备上下文环境中绘图。hWnd：设备上下文环境被检索的窗口的句柄    
        /// </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetDC(IntPtr hWnd);
        /// <summary>    
        /// 函数释放设备上下文环境（DC）供其他应用程序使用。    
        /// </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        /// <summary>    
        /// 该函数返回桌面窗口的句柄。桌面窗口覆盖整个屏幕。    
        /// </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern IntPtr GetDesktopWindow();
        /// <summary>    
        /// 该函数设置指定窗口的显示状态。    
        /// </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern bool ShowWindow(IntPtr hWnd, short State);
        /// <summary>    
        /// 通过发送重绘消息 WM_PAINT 给目标窗体来更新目标窗体客户区的无效区域。    
        /// </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern bool UpdateWindow(IntPtr hWnd);
        /// <summary>    
        /// 该函数将创建指定窗口的线程设置到前台，并且激活该窗口。
        /// 键盘输入转向该窗口，并为用户改各种可视的记号。系统给创建前台窗口的线程分配的权限稍高于其他线程。    
        /// </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern bool SetForegroundWindow(IntPtr hWnd);
        /// <summary>    
        /// 该函数改变一个子窗口，弹出式窗口式顶层窗口的尺寸，位置和Z序。    
        /// </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int Width, int Height, uint flags);
        /// <summary>    
        /// 打开剪切板    
        /// </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern bool OpenClipboard(IntPtr hWndNewOwner);
        /// <summary>    
        /// 关闭剪切板    
        /// </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern bool CloseClipboard();
        /// <summary>    
        /// 打开清空剪切板 </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern bool EmptyClipboard();
        /// <summary>    
        /// 将存放有数据的内存块放入剪切板的资源管理中    
        /// </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern IntPtr SetClipboardData(uint Format, IntPtr hData);
        /// <summary>    
        /// 该函数获得一个指定子窗口的父窗口句柄。    
        /// </summary>    
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);
        /// <summary>    
        /// 该函数将指定的消息发送到一个或多个窗口。此函数为指定的窗口调用窗口程序，直到窗口程序处理完消息再返回。　    
        /// </summary>    
        /// <param name="hWnd">其窗口程序将接收消息的窗口的句柄</param>    
        /// <param name="msg">指定被发送的消息</param>    
        /// <param name="wParam">指定附加的消息指定信息</param>    
        /// <param name="lParam">指定附加的消息指定信息</param>    
        /// <returns></returns>    
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        /// <summary>
        /// 该函数将指定的消息发送到一个或多个窗口。此函数为指定的窗口调用窗口程序，直到窗口程序处理完消息再返回。
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);
        /// <summary>    
        /// 该函数将一个消息放入（寄送）到与指定窗口创建的线程相联系消息队列里    
        /// </summary>    
        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        /// <summary>    
        /// 该函数对指定的窗口设置键盘焦点。    
        /// </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr SetFocus(IntPtr hWnd);
        /// <summary>    
        /// 该函数改变指定子窗口的父窗口。    
        /// </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public extern static IntPtr SetParent(IntPtr hChild, IntPtr hParent);
        /// <summary>    
        /// 获取对话框中子窗口控件的句柄    
        /// </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public extern static IntPtr GetDlgItem(IntPtr hdlg, int nControlID);
        /// <summary>    
        /// 该函数向指定的窗体添加一个矩形，然后窗口客户区域的这一部分将被重新绘制。    
        /// </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public extern static int InvalidateRect(IntPtr hWnd, IntPtr rect, int bErase);
        /// <summary>    
        /// 该函数产生对其他线程的控制，如果一个线程没有其他消息在其消息队列里。    
        /// </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool WaitMessage();
        /// <summary>    
        /// 该函数从一个与应用事例相关的可执行文件（EXE文件）中载入指定的光标资源.    
        /// </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr LoadCursor(IntPtr hInstance, uint cursor);
        /// <summary>    
        /// 该函数确定光标的形状。    
        /// </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr SetCursor(IntPtr hCursor);
        /// <summary>    
        /// 确定当前焦点位于哪个控件上。    
        /// </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetFocus();
        /// <summary>    
        /// 该函数从当前线程中的窗口释放鼠标捕获，并恢复通常的鼠标输入处理。捕获鼠标的窗口接收所有的鼠标输入（无论光标的位置在哪里），除非点击鼠标键时，光标热点在另一个线程的窗口中。    
        /// </summary>    
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool ReleaseCapture();
        /// <summary>
        /// 窗体闪动效果
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="bInvert"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool FlashWindow(IntPtr hWnd, bool bInvert);
        /// <summary>
        /// 获得本窗体的句柄
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow(); 
        /// <summary>
        /// 向指定的窗口句柄发送消息
        /// </summary> 
        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint messageID, IntPtr wParam, IntPtr lParam);
        /// <summary>
        /// 向指定的窗口句柄发送消息
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint messageID, int wParam, int lParam);



    }

}
