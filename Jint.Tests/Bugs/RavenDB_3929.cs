using Jint.Native;
using Jint.Parser;
using Jint.Runtime;

using Xunit;

namespace Jint.Tests.Bugs
{
	public class RavenDB_3929
	{
		[Fact]
		public void NullPropagation()
		{
			var engine = new Engine(cfg => cfg.NullPropagation());

			var jsObject = engine.Object.Construct(Arguments.Empty);
			jsObject.Put("Field", JsValue.Null, true);

			const string Script = @"
var input = { 
	Address : null 
};

var address = input.Address;
var city = input.Address.City;
var length = input.Address.City.length;

var output = {
	Count1 : input.Address.City.length,
	Count2 : this.XYZ.length
};
";

			engine.Execute(Script);

			var address = engine.GetValue("address");
			var city = engine.GetValue("city");
			var length = engine.GetValue("length");
			var output = engine.GetValue("output").AsObject();

			Assert.Equal(Null.Instance, address);
			Assert.Equal(Null.Instance, city);
			Assert.Equal(Null.Instance, length);

			Assert.Equal(Null.Instance, output.Get("Count1"));
			Assert.Equal(Undefined.Instance, output.Get("Count2"));
		}

		[Fact]
		public void NullPropagationShouldNotAffectOperators()
		{
			var engine = new Engine(cfg => cfg.NullPropagation());

			var jsObject = engine.Object.Construct(Arguments.Empty);
			jsObject.Put("NullField", JsValue.Null, true);

			var script = @"
this.is_nullfield_not_null = this.NullField !== null;
this.is_notnullfield_not_null = this.NotNullField !== null;
this.has_emptyfield_not_null = this.EmptyField !== null;
";

			var wrapperScript = string.Format(@"function ExecutePatchScript(docInner){{ (function(doc){{ {0} }}).apply(docInner); }};", script);

			engine.Execute(wrapperScript, new ParserOptions
			{
				Source = "main.js"
			});

			engine.Invoke("ExecutePatchScript", jsObject);

			Assert.False(jsObject.Get("is_nullfield_not_null").AsBoolean());
			Assert.True(jsObject.Get("is_notnullfield_not_null").AsBoolean());
			Assert.True(jsObject.Get("has_emptyfield_not_null").AsBoolean());
		}
	}
}