using Jint.Native;
using Xunit;

namespace Jint.Tests.Runtime
{
    public class CallingReduce
    {
        [Fact]
        public void NormalFunctionUseReduce()
        {
            var engine = new Engine();

            const string Script = @"
var input = [{a: 1} , {a: 2}, {a: 3}, {a: 4}, {a: 5}];
input.reduce(function (a,v) { return a+v.a; }, 0);
";

            engine.Execute(Script);

            var completionValue = engine.GetCompletionValue();
            Assert.Equal(15, completionValue.AsNumber());
        }

        [Fact]
        public void ArrowFunctionUseReduce()
        {
            var engine = new Engine();

            const string Script = @"
var input = [{a: 1} , {a: 2}, {a: 3}, {a: 4}, {a: 5}];
input.reduce((a, v) => { return a + v.a; }, 0 );
";

            engine.Execute(Script);

            var completionValue = engine.GetCompletionValue();
            Assert.Equal(15, completionValue.AsNumber());
        }

        [Fact]
        public void ArrowFunctionUseReduce2()
        {
            var engine = new Engine();

            const string Script = @"
var input = [{a: 1} , {a: 2}, {a: 3}, {a: 4}, {a: 5}];
var a = input.reduce((a, v) => { return {a: a.a + v.a}; }, {a: 0} );
 a.a;
";

            engine.Execute(Script);

            var completionValue = engine.GetCompletionValue();
            Assert.Equal(15, completionValue.AsNumber());
        }

        [Fact]
        public void ArrowFunctionUseReduce3()
        {
            var engine = new Engine();

            const string Script = @"
var input = [{a: 1} , {a: 2}, {a: 3}, {a: 4}, {a: 5}];
var a = input.reduce((a, v) => ({a: a.a + v.a}), {a: 0} );
 a.a;
";

            engine.Execute(Script);

            var completionValue = engine.GetCompletionValue();
            Assert.Equal(15, completionValue.AsNumber());
        }
    }
}