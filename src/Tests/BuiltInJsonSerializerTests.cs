using Conejo;
using NUnit.Framework;
using Should;

namespace Tests
{
    [TestFixture]
    public class BuiltInJsonSerializerTests
    {
        public enum SomeEnum { Oh, Hai }

        public class SomeType
        {
            public SomeEnum Value { get; set; }
        }

        [Test]
        public void should_serialize_enums_as_string()
        {
            var serializer = new BuiltInJsonSerializer();
            var json = serializer.Serialize(
                new SomeType { Value = SomeEnum.Hai });
            serializer.Deserialize<SomeType>(json)
                .Value.ShouldEqual(SomeEnum.Hai);
        }
    }
}
