using System;
using Jint.Native;
using Jint.Native.Error;
using Jint.Parser;

namespace Jint.Runtime
{
	public class JavaScriptException : Exception
	{
		private readonly JsValue _errorObject;

		private readonly Location _location;

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

		public JavaScriptException(JsValue error, Location location)
			: base(GetErrorMessage(error, location))
		{
			_errorObject = error;
			_location = location;
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

		public override string ToString()
		{
			return _errorObject.ToString();
		}
	}
}
