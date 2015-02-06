using Jint.Runtime;

using Xunit;

namespace Jint.Tests
{
	public class ErrorTests
	{
		[Fact]
		public void T1()
		{
			var script = @"
var a = {};

var b = a.user.name;
";

			var engine = new Engine();
			var e = Assert.Throws<JavaScriptException>(() => engine.Execute(script));
			Assert.Equal("user is undefined", e.Message);
			Assert.Equal(4, e.Location.Start.Line);
			Assert.Equal(8, e.Location.Start.Column);
		}

		[Fact]
		public void T2()
		{
			var script = @"
test();
";

			var engine = new Engine();
			var e = Assert.Throws<JavaScriptException>(() => engine.Execute(script));
			Assert.Equal("test is not defined", e.Message);
			Assert.Equal(2, e.Location.Start.Line);
			Assert.Equal(0, e.Location.Start.Column);
		}
	}
}