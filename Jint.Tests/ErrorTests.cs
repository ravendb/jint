using System;

using Jint.Native;
using Jint.Parser;
using Jint.Runtime;

using Xunit;

namespace Jint.Tests
{
	public class ErrorTests
	{
		[Fact]
		public void Error()
		{
			var script = @"function test(a, b) { 
return {
	'one' : 's' instanceof String,
	'two' : a instanceof String,
	'three': b instanceof String
};

}";

			var engine = new Engine();
			engine.Execute(script);

			var a = new JsValue("v");

			var b = new JsValue(engine.String.Construct("w"));

			var result = engine.Invoke("test", engine.Global, a, b);
			var r = result.ToObject();

		}

		[Fact]
		public void CanReturnCorrectErrorMessageAndLocation1()
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
		public void CanReturnCorrectErrorMessageAndLocation2()
		{
			var script = @"
 test();
";

			var engine = new Engine();
			var e = Assert.Throws<JavaScriptException>(() => engine.Execute(script));
			Assert.Equal("test is not defined", e.Message);
			Assert.Equal(2, e.Location.Start.Line);
			Assert.Equal(1, e.Location.Start.Column);
		}

		[Fact]
		public void CanProduceCorrectStackTrace()
		{
			var engine = new Engine();

			engine.Options.LimitRecursion(100);

			engine.Execute(@"var a = function(v) {
	return v.xxx.yyy;
}

var b = function(v) {
	return a(v);
}", new ParserOptions
			{
				Source = "custom.js"
			});

			var e = Assert.Throws<JavaScriptException>(() => engine.Execute("var x = b(7);", new ParserOptions { Source = "main.js"}));
			Assert.Equal("xxx is undefined", e.Message);
			Assert.Equal(2, e.Location.Start.Line);
			Assert.Equal(8, e.Location.Start.Column);
			Assert.Equal("custom.js", e.Location.Source);

			var stack = e.CallStack;
			Assert.Equal("a@custom.js:2" + Environment.NewLine + "b@custom.js:6" + Environment.NewLine + "anonymous function@main.js:1", stack);
		}
	}
}