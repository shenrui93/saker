/***************************************************************************
 * 
 * 创建时间：   2016/4/14 14:16:56
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供一个只进的读写流。该类是线程安全的
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Uyi.Serialization.BigEndian;

namespace Uyi.IO
{
    /// <summary>
    /// 提供一个只进的读写流。该类是线程安全的
    /// </summary>
    public class NetworkStream : Stream
    {
        const int MaxBufferCount = 1024 * 1;
        const int MaxBufferCat = 1024 * 2;
        int _isDisposed = 0;
        int read_pos = 0;           //数据读取游标
        int writer_pos = 0;         //数据写入游标
        byte[] _buffer = new byte[256];
        int cat = 256;
        int count = 0;

        static readonly byte[] EmptyBytes = new byte[0];
        /// <summary>
        /// 
        /// </summary>
        public NetworkStream()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public NetworkStream(byte[] data)
        {
            if (data == null || data.Length == 0) return;
            this.Write(data, 0, data.Length);
        }



        private void WriteInternal(byte[] buffer, int offset, int count)
        {
            if (buffer.Length < offset + count) throw new IndexOutOfRangeException();
            EnterLock();
            try
            {
                //计算追加数据后新的数据长度
                var newcount = this.count + count;
                while (newcount + this.read_pos > cat)
                {
                    //检查数据是否溢出缓冲区，如果不是强制移动数据
                    if (newcount < cat && newcount <= cat)
                    {
                        //启动强制Buffer初始化操作
                        InitBuffer(true);
                        continue;
                    }
                    //数据缓冲区扩容
                    while (newcount > cat)
                    {
                        cat *= 2;
                    }
                    var newBuffer = new byte[cat];
                    InternalBlockCopy(_buffer, read_pos, newBuffer, 0, this.count);

                    this._buffer = newBuffer;
                    this.read_pos = 0;
                    this.writer_pos = this.count;
                }
                InternalBlockCopy(buffer, offset, this._buffer, this.writer_pos, count);
                this.count = newcount;
                this.writer_pos += count;
                InitBuffer();
            }
            finally
            {
                ExitLock();
            }

        }
        private int ReadInternal(byte[] buffer, int offset, int count)
        {
            EnterLock();
            try
            {
                if (this.count == 0) return -1;
                var relCount = this.count > count + offset ? count : this.count - offset;
                InternalBlockCopy(this._buffer, read_pos, buffer, offset, relCount);
                this.count -= relCount;
                this.read_pos += relCount;
                InitBuffer();
                return relCount;
            }
            finally
            {
                ExitLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public new void CopyTo(Stream stream)
        {
            throw new NotSupportedException();
        }

        //private int ReadOnlyInternal(byte[] buffer, int offset, int count)
        //{
        //    lock (root)
        //    {
        //        if (this.count == 0) return 0;
        //        var relCount = this.count > count + offset ? count : this.count - offset;
        //        System.InternalBlockCopy(this._buffer, offset, buffer, 0, relCount);
        //        return relCount;
        //    }
        //}
        private byte[] ReadBytesInternal(int count)
        {

            EnterLock();
            try
            {
                if (this.count == 0) return EmptyBytes;
                int relCount;
                if (count < 0)
                    relCount = this.count;
                else
                    relCount = this.count > count ? count : this.count;


                var buffer = new byte[relCount];
                InternalBlockCopy(this._buffer, this.read_pos, buffer, 0, relCount);
                this.count -= relCount;
                this.read_pos += relCount;
                InitBuffer();
                return buffer;
            }
            finally
            {
                ExitLock();
            }

        }
        private byte[] ReadOnlyBytesInternal(int offset, int count)
        {
            EnterLock();
            try
            {
                if (this.count == 0) return EmptyBytes;
                int relCount;
                if (count < 0)
                    relCount = this.count;
                else
                    relCount = this.count > offset + count ? count : this.count - offset;

                var buffer = new byte[relCount];
                InternalBlockCopy(this._buffer, read_pos + offset, buffer, 0, relCount);
                return buffer;
            }
            finally
            {
                ExitLock();
            }

        }
        private byte[] ReadAllBytesInternal()
        {
            return ReadBytesInternal(-1);
        }
        private byte[] ReadOnlyAllBytesInternal()
        {
            return ReadOnlyBytesInternal(0, -1);
        }
        private void RemoveInternal(int count)
        {
            EnterLock();
            try
            {
                var newcount = this.count - count;
                if (newcount > 0)
                    this.read_pos += count;
                this.count = newcount < 0 ? 0 : newcount;
                InitBuffer();
            }
            finally
            {
                ExitLock();
            }
        }
        /// <summary>
        /// 读取一个字节数据
        /// </summary>
        /// <returns></returns>
        public override int ReadByte()
        {
            EnterLock();
            try
            {
                if (this.count == 0)
                {
                    return -1;
                }
                byte ret = this._buffer[this.read_pos];
                this.count--;
                this.read_pos++;
                InitBuffer();
                return ret;
            }
            finally
            {
                ExitLock();
            }
        }

        #region Stream  相关成员
        /// <summary>
        /// 只是该流是否支持读取
        /// </summary>
        public override bool CanRead => true;
        /// <summary>
        /// 指示该流是否支持查找
        /// </summary>
        public override bool CanSeek => false;
        /// <summary>
        /// 指示该流是否支持写入
        /// </summary>
        public override bool CanWrite => true;
        /// <summary>
        /// 获取该流的数据长度
        /// </summary>
        public override long Length
        {
            get
            {
                EnterLock();
                try
                {
                    return this.count;
                }
                finally
                {
                    ExitLock();
                }
            }
        }
        /// <summary>
        /// 获取或者设置该流的游标位置
        /// </summary>
        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        /// <summary>
        /// 将数据写入基础支持流并清空缓冲区
        /// </summary>
        public override void Flush()
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// 读取并删除读取的数据
        /// </summary> 
        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadInternal(buffer, offset, count);
        }
        /// <summary>
        /// 设置当前流的游标位置
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <param name="origin"></param>
        /// <returns>该流不支持该方法，调用该方法永远引发 <see cref="NotSupportedException"/> 异常</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// 设置数据流的长度，执行该操作一般会导致截断数据
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// 向流的末尾处追加数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteInternal(buffer, offset, count);
        }

        #endregion 

        /// <summary>
        /// 长度
        /// </summary>
        public int Count
        {
            get
            {
                EnterLock();
                try
                {
                    return this.count;
                }
                finally
                {
                    ExitLock();
                }
            }
        }
        /// <summary>
        /// 获取指定索引处的字节数据
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte this[int index]
        {
            get
            {
                EnterLock();
                try
                {
                    return this._buffer[index + this.read_pos];
                }
                finally
                {
                    ExitLock();
                }

            }
        }
        /// <summary>
        /// 读取但不删除数据
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] ReadArray(int startIndex, int count)
        {
            return this.ReadOnlyBytesInternal(startIndex, count);
        }
        /// <summary>
        /// 读取并删除读取的数据
        /// </summary> 
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] ReadAndRemoveBytes(int count)
        {
            return this.ReadBytesInternal(count);
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="count"></param>
        public void Remove(int count)
        {
            RemoveInternal(count);
        }
        /// <summary>
        /// 返回缓冲区内所有数据的一个数组
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            return ReadOnlyAllBytesInternal();
        }


        /// <summary>
        /// 返回缓冲区内所有数据的一个数组，并清空缓存区
        /// </summary>
        /// <returns></returns>
        public virtual byte[] ToArray()
        {
            return ReadAllBytesInternal();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void Write(byte[] data)
        {
            Write(data, 0, data.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (System.Threading.Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;
            base.Dispose(disposing);

        }


        //自旋锁对象
        System.Threading.SpinLock spinLock = new System.Threading.SpinLock();

        private void EnterLock()
        {
            bool lockToken = false;
            spinLock.Enter(ref lockToken);
        }
        private void ExitLock()
        {
            spinLock.Exit();
        }



        private void InitBuffer(bool force = false)
        {
            if (force)
            {
                if (this.read_pos > 0)
                {
                    if (this.count > 0)
                    {
                        //启动强制缩容
                        InternalBlockCopy(this._buffer, this.read_pos, this._buffer, 0, this.count);
                        this.read_pos = 0;
                    }
                    else
                    {
                        this.read_pos = this.writer_pos = 0;
                        return;
                    }
                }
                this.writer_pos = this.count;
                return;
            }

            //重新调整缓冲区大小增加伸缩性
            if (this.cat >= MaxBufferCat && this.count <= MaxBufferCount)
            {
                this.cat = MaxBufferCount;
                var newbuffer = new byte[MaxBufferCount];
                if (this.count > 0)
                    InternalBlockCopy(this._buffer, read_pos, newbuffer, 0, this.count);
                this._buffer = newbuffer;
                this.read_pos = 0;
                this.writer_pos = this.count;
            }

            return;

        }
        void InternalBlockCopy(byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
        {
            Buffer.BlockCopy(src, srcOffset, dst, dstOffset, count); 
        }
    }


}
