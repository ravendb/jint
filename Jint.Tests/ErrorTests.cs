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
			Assert.Equal("user is undefined. Line 4. Column: 8.", e.Message);
		}

		[Fact]
		public void T2()
		{
			var script = @"
test();
";

			var engine = new Engine();
			var e = Assert.Throws<JavaScriptException>(() => engine.Execute(script));
			Assert.Equal("test is not defined. Line 2. Column: 0.", e.Message);
		}
	}
}