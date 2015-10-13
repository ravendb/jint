using System;
using System.Collections.Generic;
using System.Linq;
using Jint.Native;
using Jint.Native.Error;
using Jint.Parser;
using Jint.Runtime.CallStack;

namespace Jint.Runtime
{
    public class JavaScriptException : Exception
    {
        private readonly JsValue _errorObject;

        private readonly Location _location;

        private readonly string _callStack;

        public JavaScriptException(ErrorConstructor errorConstructor)
            : base(string.Empty)
        {
            _errorObject = errorConstructor.Construct(Arguments.Empty);
        }

        public JavaScriptException(ErrorConstructor errorConstructor, string message)
            : base(message)
        {
            _errorObject = errorConstructor.Construct(new JsValue[] { message });
        }

        public JavaScriptException(ErrorConstructor errorConstructor, string message, Location location)
            : base(message)
        {
            _errorObject = errorConstructor.Construct(new JsValue[] { message });
            _location = location;
        }

        public JavaScriptException(JsValue error, Location location, JintCallStack callStack)
            : base(GetErrorMessage(error, location))
        {
            _errorObject = error;
            _location = location;
            _callStack = ConstructCallStack(location, callStack);
        }

        /// <summary>
        /// Since JintCallStack differs in presentation between stacks we know from other languages 
        /// we modify it slightly to make it easier to read.
        /// 
        /// Also internal callstack lucks current statement - this is why we call location. 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="callStack"></param>
        /// <returns></returns>
        private string ConstructCallStack(Location location, JintCallStack callStack)
        {
            var stack = callStack.Stack;

            var functionsList = stack.Select(cse => cse.ToString()).ToList();
            functionsList.Add("anonymous function"); // append root invocation

            var locations = new List<string> { location.Source + ":" + location.Start.Line };
            locations.AddRange(stack.Select(cse => cse.CallExpression.Location.Source + ":" + cse.CallExpression.Location.Start.Line));

            return string.Join(Environment.NewLine, functionsList.Zip(locations, (f, l) => f + "@" + l));
        }

        private static string GetErrorMessage(JsValue error, Location location)
        {
            string message = string.Empty;
            if (error.IsObject())
            {
                var oi = error.AsObject();
                message = oi.Get("message").AsString();
            }

            if (string.IsNullOrEmpty(message))
                return "Unknown error.";

            return message;
        }

        public JsValue Error { get { return _errorObject; } }

        public Location Location { get { return _location; } }

        public int LineNumber { get { return null == Location ? 0 : Location.Start.Line; } }

        public int Column { get { return null == Location ? 0 : Location.Start.Column; } }

        public string CallStack { get { return _callStack; } }

        public override string ToString()
        {
            return _errorObject.ToString();
        }
    }
}
