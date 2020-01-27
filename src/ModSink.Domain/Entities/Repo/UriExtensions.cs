using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ModSink.Domain.Entities.Repo
{
public static class UriExtensions
{
    public static string ToSerializableString(this Uri uri)
    {
        var si = new SerializationInfo(typeof(Uri), new FormatterConverter());
        ((ISerializable)uri).GetObjectData(si, new StreamingContext());
        return si.GetString("RelativeUri");
    }

    public static Uri ToUri(string serializedUri)
    {
        var si = new SerializationInfo(typeof(Uri), new FormatterConverter());
        si.AddValue("RelativeUri", serializedUri);
        var c = new StreamingContext();
        return new SerializableUri(si, c);
    }

    private class SerializableUri : Uri
    {
        public SerializableUri(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {

        }
    }

    private class FormatterConverter : IFormatterConverter
    {
        public object Convert(object value, Type type)
        {
            throw new NotImplementedException();
        }

        public object Convert(object value, TypeCode typeCode)
        {
            throw new NotImplementedException();
        }

        public bool ToBoolean(object value)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(object value)
        {
            throw new NotImplementedException();
        }

        public char ToChar(object value)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(object value)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(object value)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(object value)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(object value)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(object value)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(object value)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(object value)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(object value)
        {
            throw new NotImplementedException();
        }

        public string ToString(object value)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(object value)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(object value)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(object value)
        {
            throw new NotImplementedException();
        }
    }
}
}
