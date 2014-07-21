using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Conejo
{
    public class BuiltInJsonSerializer : ISerializer
    {
        private readonly JavaScriptSerializer _serializer;

        public BuiltInJsonSerializer()
        {
            _serializer = new JavaScriptSerializer();
            _serializer.RegisterConverters(new[] { new EnumConverter() });
        }

        public string Serialize(object @object)
        {
            return _serializer.Serialize(@object);
        }

        public T Deserialize<T>(string source)
        {
            return _serializer.Deserialize<T>(source);
        }

        private class EnumConverter : JavaScriptConverter
        {
            public override IEnumerable<Type> SupportedTypes
            {
                get { return new[] { typeof(Enum) }; }
            }

            public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                return serializer.ConvertToType(dictionary, type);
            }

            public override IDictionary<string, object> Serialize(object @object, JavaScriptSerializer serializer)
            {
                return new StringValue(@object.ToString());
            }
        }

        private class StringValue : Uri, IDictionary<string, object>
        {
            public StringValue(string value) : base(value, UriKind.Relative) { }

            ICollection<string> IDictionary<string, object>.Keys { get { throw new NotImplementedException(); } }
            ICollection<object> IDictionary<string, object>.Values { get { throw new NotImplementedException(); } }
            object IDictionary<string, object>.this[string key] { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
            void IDictionary<string, object>.Add(string key, object value) { throw new NotImplementedException(); }
            bool IDictionary<string, object>.ContainsKey(string key) { throw new NotImplementedException(); }
            bool IDictionary<string, object>.Remove(string key) { throw new NotImplementedException(); }
            bool IDictionary<string, object>.TryGetValue(string key, out object value) { throw new NotImplementedException(); }
            void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) { throw new NotImplementedException(); }
            void ICollection<KeyValuePair<string, object>>.Clear() { throw new NotImplementedException(); }
            bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) { throw new NotImplementedException(); }
            void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) { throw new NotImplementedException(); }
            int ICollection<KeyValuePair<string, object>>.Count { get { throw new NotImplementedException(); } }
            bool ICollection<KeyValuePair<string, object>>.IsReadOnly { get { throw new NotImplementedException(); } }
            bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item) { throw new NotImplementedException(); }
            IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() { throw new NotImplementedException(); }
            IEnumerator IEnumerable.GetEnumerator() { throw new NotImplementedException(); }
        }
    }
}