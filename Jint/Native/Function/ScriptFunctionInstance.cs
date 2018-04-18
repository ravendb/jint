﻿using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Esprima.Ast;
using Jint.Native.Argument;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Descriptors.Specialized;
using Jint.Runtime.Environments;

namespace Jint.Native.Function
{
    /// <summary>
    ///
    /// </summary>
    public sealed class ScriptFunctionInstance : FunctionInstance, IConstructor
    {
        private const string PropertyNameName = "name";
        private const int PropertyNameNameLength = 4;

        private PropertyDescriptor _name;

        private readonly IFunction _functionDeclaration;

        /// <summary>
        /// http://www.ecma-international.org/ecma-262/5.1/#sec-13.2
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="functionDeclaration"></param>
        /// <param name="scope"></param>
        /// <param name="strict"></param>
        public ScriptFunctionInstance(Engine engine, IFunction functionDeclaration, LexicalEnvironment scope, bool strict)
            : base(engine, GetParameterNames(functionDeclaration), scope, strict)
        {
            _functionDeclaration = functionDeclaration;

            Extensible = true;
            Prototype = engine.Function.PrototypeObject;

            DefineOwnProperty("length", new PropertyDescriptor(JsNumber.Create(FormalParameters.Length), PropertyFlag.AllForbidden), false);

            var proto = new ObjectInstanceWithConstructor(engine, this)
            {
                Extensible = true,
                Prototype = Engine.Object.PrototypeObject
            };

            SetOwnProperty("prototype", new PropertyDescriptor(proto, PropertyFlag.OnlyWritable));

            if (_functionDeclaration.Id != null)
            {
                _name = new PropertyDescriptor(_functionDeclaration.Id.Name, PropertyFlag.None);
            }

            if (strict)
            {
                var thrower = engine.Function.ThrowTypeError;
                const PropertyFlag flags = PropertyFlag.EnumerableSet | PropertyFlag.ConfigurableSet;
                DefineOwnProperty("caller", new GetSetPropertyDescriptor(thrower, thrower, flags), false);
                DefineOwnProperty("arguments", new GetSetPropertyDescriptor(thrower, thrower, flags), false);
            }
        }

        public override IEnumerable<KeyValuePair<string, PropertyDescriptor>> GetOwnProperties()
        {
            if (_name != null)
            {
                yield return new KeyValuePair<string, PropertyDescriptor>(PropertyNameName, _name);
            }

            foreach (var entry in base.GetOwnProperties())
            {
                yield return entry;
            }
        }

        public override PropertyDescriptor GetOwnProperty(string propertyName)
        {
            if (propertyName.Length == PropertyNameNameLength && propertyName == PropertyNameName)
            {
                return _name ?? PropertyDescriptor.Undefined;
            }

            return base.GetOwnProperty(propertyName);
        }

        protected internal override void SetOwnProperty(string propertyName, PropertyDescriptor desc)
        {
            if (propertyName.Length == PropertyNameNameLength && propertyName == PropertyNameName)
            {
                _name = desc;
            }
            else
            {
                base.SetOwnProperty(propertyName, desc);
            }
        }

        public override bool HasOwnProperty(string propertyName)
        {
            if (propertyName.Length == PropertyNameNameLength && propertyName == PropertyNameName)
            {
                return _name != null;
            }

            return base.HasOwnProperty(propertyName);
        }

        public override void RemoveOwnProperty(string propertyName)
        {
            if (propertyName.Length == PropertyNameNameLength && propertyName == PropertyNameName)
            {
                _name = null;
            }

            base.RemoveOwnProperty(propertyName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string[] GetParameterNames(IFunction functionDeclaration)
        {
            var list = functionDeclaration.Params;
            var count = list.Count;

            if (count == 0)
            {
                return System.Array.Empty<string>();
            }

            var names = new string[count];
            for (var i = 0; i < count; ++i)
            {
                names[i] = ((Identifier) list[i]).Name;
            }

            return names;
        }

        /// <summary>
        /// http://www.ecma-international.org/ecma-262/5.1/#sec-13.2.1
        /// </summary>
        /// <param name="thisArg"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public override JsValue Call(JsValue thisArg, JsValue[] arguments)
        {
            using (new StrictModeScope(Strict, true))
            {
                // setup new execution context http://www.ecma-international.org/ecma-262/5.1/#sec-10.4.3
                JsValue thisBinding;
                if (StrictModeScope.IsStrictModeCode)
                {
                    thisBinding = thisArg;
                }
                else if (ReferenceEquals(thisArg, Undefined) || ReferenceEquals(thisArg, Null))
                {
                    thisBinding = Engine.Global;
                }
                else if (!thisArg.IsObject())
                {
                    thisBinding = TypeConverter.ToObject(Engine, thisArg);
                }
                else
                {
                    thisBinding = thisArg;
                }

                var localEnv = LexicalEnvironment.NewDeclarativeEnvironment(Engine, Scope);

                Engine.EnterExecutionContext(localEnv, localEnv, thisBinding);

                Completion result = null;
                try
                {
                    var argumentInstanceRented = Engine.DeclarationBindingInstantiation(
                        DeclarationBindingType.FunctionCode,
                        _functionDeclaration.HoistingScope.FunctionDeclarations,
                        _functionDeclaration.HoistingScope.VariableDeclarations,
                        this,
                        arguments);

                    result = Engine.ExecuteStatement(_functionDeclaration.Body);
                    
                    var value = result.GetValueOrDefault();
                    
                    // we can safely release arguments if they don't escape the scope
                    if (argumentInstanceRented
                        && Engine.ExecutionContext.LexicalEnvironment?.Record is DeclarativeEnvironmentRecord der
                        && !(result.Value is ArgumentsInstance))
                    {
                        der.ReleaseArguments();
                    }

                    if (result.Type == Completion.Throw)
                    {
                        var ex = new JavaScriptException(value).SetCallstack(Engine, result.Location);
                        throw ex;
                    }

                    if (result.Type == Completion.Return)
                    {
                        return value;
                    }
                }
                finally
                {
                    Engine.CompletionPool.Return(result);
                    Engine.LeaveExecutionContext();
                }

                return Undefined;
            }
        }

        /// <summary>
        /// http://www.ecma-international.org/ecma-262/5.1/#sec-13.2.2
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public ObjectInstance Construct(JsValue[] arguments)
        {
            var proto = Get("prototype").TryCast<ObjectInstance>();
            var obj = new ObjectInstance(Engine)
            {
                Extensible = true,
                Prototype = proto ?? Engine.Object.PrototypeObject
            };

            var result = Call(obj, arguments).TryCast<ObjectInstance>();
            if (!ReferenceEquals(result, null))
            {
                return result;
            }

            return obj;
        }

        private class ObjectInstanceWithConstructor : ObjectInstance
        {
            private const string PropertyNameConstructor = "constructor";
            private const int PropertyNameConstructorLength = 11;
            private PropertyDescriptor _constructor;

            public ObjectInstanceWithConstructor(Engine engine, ObjectInstance thisObj) : base(engine)
            {
                _constructor = new PropertyDescriptor(thisObj, PropertyFlag.NonEnumerable);
            }

            public override IEnumerable<KeyValuePair<string, PropertyDescriptor>> GetOwnProperties()
            {
                if (_constructor != null)
                {
                    yield return new KeyValuePair<string, PropertyDescriptor>(PropertyNameConstructor, _constructor);
                }

                foreach (var entry in base.GetOwnProperties())
                {
                    yield return entry;
                }
            }

            public override PropertyDescriptor GetOwnProperty(string propertyName)
            {
                if (propertyName.Length == PropertyNameConstructorLength && propertyName == PropertyNameConstructor)
                {
                    return _constructor ?? PropertyDescriptor.Undefined;
                }

                return base.GetOwnProperty(propertyName);
            }

            protected internal override void SetOwnProperty(string propertyName, PropertyDescriptor desc)
            {
                if (propertyName.Length == PropertyNameConstructorLength && propertyName == PropertyNameConstructor)
                {
                    _constructor = desc;
                }
                else
                {
                    base.SetOwnProperty(propertyName, desc);
                }
            }

            public override bool HasOwnProperty(string propertyName)
            {
                if (propertyName.Length == PropertyNameConstructorLength && propertyName == PropertyNameConstructor)
                {
                    return _constructor != null;
                }

                return base.HasOwnProperty(propertyName);
            }

            public override void RemoveOwnProperty(string propertyName)
            {
                if (propertyName.Length == PropertyNameConstructorLength && propertyName == PropertyNameConstructor)
                {
                    _constructor = null;
                }
                else
                {
                    base.RemoveOwnProperty(propertyName);
                }
            }
        }

        public IFunction GetFunctionDeclaration => _functionDeclaration;
    }
}