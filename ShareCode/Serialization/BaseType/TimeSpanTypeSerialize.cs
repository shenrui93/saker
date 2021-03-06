﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Saker.Serialization.BigEndian;

namespace Saker.Serialization.BaseType
{
    internal class TimeSpanTypeSerialize : TypeSerializationBase<TimeSpan>
    {
        public override void Serialize(TimeSpan obj, System.IO.Stream stream)
        {
            Write(obj, stream);
        }
        public override TimeSpan Deserialize(System.IO.Stream stream)
        {
            return Read(stream);

        }

        public static void Write(TimeSpan obj, System.IO.Stream stream)
        {
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, obj);
        }
        public static TimeSpan Read(System.IO.Stream stream)
        {
            TimeSpan v;
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out v);
            return v;
        }

        public TimeSpanTypeSerialize()
        {
            this._serializerType = typeof(TimeSpan);
            this._serializerName = this._serializerType.FullName;
        }

        public override System.Reflection.MethodInfo GetReaderMethod()
        {
            Func<System.IO.Stream, TimeSpan> reader = Read;
            return reader.Method;
        }
        public override System.Reflection.MethodInfo GetWriterMethod()
        {
            Action<TimeSpan, System.IO.Stream> writer = Write;
            return writer.Method;
        }


        public override unsafe TimeSpan UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeRead(stream, pos, length);
        }
        public unsafe static TimeSpan UnsafeRead(byte* stream, int* pos, int length)
        {
            TimeSpan v;
            BigEndian.BigEndianPrimitiveTypeSerializer
                .Instance.ReadValue(stream, pos, length, out v);
            return v;

        }
        public unsafe override System.Reflection.MethodInfo GetUnsafeReaderMethod()
        {
            delUnsafeReaderMethod reader = UnsafeRead;
            return reader.Method;
        }

    }
}
