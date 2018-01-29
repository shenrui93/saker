/***************************************************************************
 * 
 * 创建时间：   2016/6/24 12:05:43
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   文件操作帮助类
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Saker.Extension;

namespace Saker.IO
{
    /// <summary>
    /// 文件操作帮助类
    /// </summary>
    public static partial class FileHelper
    {  
        /// <summary>
        /// 将文件删除进回收站而不直接删除
        /// </summary>
        /// <param name="fullName"></param>
        public static bool DeleteFileToRecycle(string fullName)
        {
            try
            {
                if (CheckFileIsOpen(fullName)) return false;
                //为何不始用File.Delete()，是因为该方法不经过回收站，直接删除文件
                //要删除至回收站，可使用VisualBasic删除文件，需引用Microsoft.VisualBasic
                //删除确认对话框是根据电脑系统-回收站-显示删除确认对话框   是否打勾 自动添加的
                //为何不使用c#的File.Delete()方法？？？因为该方法是直接删除，而不是放入回收站
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(fullName,
                Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin,
                Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);

                return true;
            }
            catch //(Exception ex)
            {
                return false;
            }

        }
    }

}