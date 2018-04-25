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
 'Hydrogen',
  'Helium',
  'Lithium',
  'Beryllium'
];
materials.map(function(material) {return  material.length;}); // [8, 6, 7, 9]

materials.map(material => material.length); // [8, 6, 7, 9]
");
            var value = engine.GetCompletionValue();
            Assert.Equal(4u, value.AsArray().GetLength());
            Assert.Equal(8, value.AsArray().Get("0").AsNumber());
            Assert.Equal(6, value.AsArray().Get("1").AsNumber());
            Assert.Equal(7, value.AsArray().Get("2").AsNumber());
            Assert.Equal(9, value.AsArray().Get("3").AsNumber());
        }

        [Fact]
        public void CanIdentifyThisAndOuterParam()
        {
            var engine = new Engine();
            engine.Execute(@"
var self =this;
var str = 'a';
var funcA = () => {
return !!this && !!self && this==self && str=='a';
};

funcA();
            
");
            var value = engine.GetCompletionValue();
            Assert.Equal(true, value.AsBoolean());

        }

        [Fact]
        public void CanIdentifyInnerVariableWithSameName()
        {
            var engine = new Engine();
            engine.Execute(@"
var outer = 2;

var funcA = () => {
var outer = 4;
return outer;
};

funcA();
            
");
            var value = engine.GetCompletionValue();
            Assert.Equal(4, value.AsNumber());

        }

        [Fact]
        public void ShouldNotIdentifyOuterParameter()
        {
            var engine = new Engine();
            var script = @"
var outer = 2;

var funcA = () => {
var inner = 4;
return outer;
};

var funcB = () =>{
return inner;
};

funcA();
funcB();


            
";
            var exception = Assert.Throws<Jint.Runtime.JavaScriptException>(() => engine.Execute(script));
            Assert.Equal("inner is not defined", exception.Message);
        }

        [Fact]
        public void ShouldRecognizeArrayOutsideOfFunction()
        {
            var engine = new Engine();
            var script = @"
var numbers = [];
var fives = [];

for (var i=0; i<20; i++)
    numbers.push(i);

numbers.forEach(x=>{
if (x%5==0)
fives.push(x);
});

fives;
            
";
            engine.Execute(script);
            Native.JsValue value = engine.GetCompletionValue();
            var fivesArray = value.AsArray();
            Assert.Equal(4u, fivesArray.GetLength());

            for (var i = 0; i < 4; i++)
            {                
                Assert.Equal(i*5d, fivesArray.Get(i.ToString()).AsNumber());
            }                      
        }


        [Fact]
        public void ShouldRecognizeArrayOutsideOfFunctionInThis()
        {
            var engine = new Engine();
            var script = @"
var numbers = [];
var fives = [];

for (var i=0; i<20; i++)
    numbers.push(i);

numbers.forEach(x=>{
if (x%5==0)
this.fives.push(x);
});

fives;
            
";
            engine.Execute(script);
            Native.JsValue value = engine.GetCompletionValue();
            var fivesArray = value.AsArray();
            Assert.Equal(4u, fivesArray.GetLength());

            for (var i = 0; i < 4; i++)
            {
                Assert.Equal(i * 5d, fivesArray.Get(i.ToString()).AsNumber());
            }
        }

        [Fact]
        public void ThisShouldInheritedFromParentObject()
        {
            var engine = new Engine();
            var script = @"

var parentClass = function(){
this.prop = 'aaa';
this.innerFunc = function(){
return this.prop;
};
};

var parentObject = new parentClass();
parentObject.innerFunc();

";
            engine.Execute(script);
            Native.JsValue value = engine.GetCompletionValue();
            Assert.Equal("aaa", value.AsString());
            
        }
    }
}