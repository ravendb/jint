using Xunit;

namespace Jint.Tests.Runtime
{
    public class ArrowFunctions
    {
        [Fact]
        public void CanParseFunctionCallingIt()
        {
            var engine = new Engine();
            engine.Execute(@"
var materials = [

];
materials.map(material => material.length); // [8, 6, 7, 9]
");
            var value = engine.GetCompletionValue();
            Assert.Equal(4u, value.AsArray().GetLength());
            Assert.Equal(8, value.AsArray().Get("0").AsNumber());
            Assert.Equal(6, value.AsArray().Get("1").AsNumber());
            Assert.Equal(7, value.AsArray().Get("2").AsNumber());
            Assert.Equal(9, value.AsArray().Get("3").AsNumber());
        }
    }
}