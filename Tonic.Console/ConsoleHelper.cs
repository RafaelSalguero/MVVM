using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tonic;
namespace Tonic.Console
{
    /// <summary>
    /// Provides methods for calling .NET code from console
    /// </summary>
    public class ConsoleHelper
    {
        /// <summary>
        /// Converts an string onto a value of the given type
        /// </summary>
        /// <param name="Value">The given value</param>
        /// <param name="Data">The data type</param>
        /// <returns></returns>
        public static object FromString(string Value, Type Type)
        {
            if (Value == "null")
                return null;

            var Converter = TypeDescriptor.GetConverter(Type);
            return Converter.ConvertFromString(Value);
        }

        public static string[] SplitLines(string Input)
        {
            return Input.Split(';').Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
        }

        private static string[] Split(string Input)
        {
            return Input.Split(' ').Where(x => x.Length > 0).ToArray();
        }

        private List<MethodInfo> Methods = new List<MethodInfo>();

        public Stack<object> Stack = new Stack<object>();
        public Dictionary<string, object> Locals = new Dictionary<string, object>();
        public List<Type> Types = new List<Type>();
        public object Instance;

        public ConsoleHelper()
        {
            Types.Add(typeof(Int64));
            Types.Add(typeof(Double));
            Types.Add(typeof(Single));
            Types.Add(typeof(Byte));
            Types.Add(typeof(Boolean));
            Types.Add(typeof(DateTime));
            Types.Add(typeof(String));
            Types.Add(typeof(Int32));
            Types.Add(typeof(Boolean));
            Types.Add(typeof(Enumerable));
            Types.Add(typeof(Queryable));
        }

        public void LoadAssemblyTypes(Type AssemblyReference)
        {
            foreach (var Type in AssemblyReference.Assembly.GetTypes())
                Types.Add(Type);
        }

        /// <summary>
        /// Load a callable method to the console helper
        /// </summary>
        /// <param name="Method"></param>
        public void LoadMethod(MethodInfo Method)
        {
            Methods.Add(Method);
        }
        public void LoadInstance(object Instance)
        {
            Methods.AddRange(Instance.GetType().GetMethods());
        }

        public Type GetType(string Name)
        {
            var SimpleName = Types.Where(x => x.Name == Name);
            if (SimpleName.Count() > 1)
                throw new ArgumentException($"Can't solve between {Types.Select(x => x.FullName + Environment.NewLine).Aggregate("", (a, b) => a + b, x => x)}");
            else if (SimpleName.Count() == 1)
            {
                return SimpleName.First();
            }

            var FullName = Types.Where(x => x.FullName == Name).FirstOrDefault();
            if (FullName == null)
                throw new ArgumentException($"Type {Name} not found");

            return FullName;
        }

        public string Help()
        {
            StringBuilder B = new StringBuilder();

            B.AppendLine("stack");
            B.AppendLine("***************");
            B.AppendLine();

            B.AppendLine("current - print the current instance");
            B.AppendLine("reset - clear the stack and the local dictionary");
            B.AppendLine("stack - shows the content of the stack");
            B.AppendLine("dup - duplicate the stack item");
            B.AppendLine("pop - drop the stack item");


            B.AppendLine("values");
            B.AppendLine("***************");
            B.AppendLine();

            B.AppendLine("[value] [type] cast  - cast [value] to [type]");
            B.AppendLine("str [string] - loads [string] to the stack");
            B.AppendLine("int [value] - loads [value] to the stack");
            B.AppendLine("null - load null onto the stack");
            B.AppendLine("json - serialize stack item");

            B.AppendLine("variables");
            B.AppendLine("***************");
            B.AppendLine();

            B.AppendLine("store [var] - store the stack item onto [var]");
            B.AppendLine("load [var] - load [var] onto the stack");
            B.AppendLine("locals - Show all variables");


            B.AppendLine("types");
            B.AppendLine("***************");
            B.AppendLine();

            B.AppendLine("types - show all loaded types");
            B.AppendLine("type [type] - load the given type by name onto the stack");
            B.AppendLine("[instance] typeof - gets the type of the stack item");
            B.AppendLine("[value] [type] cast - cast value to type");


            B.AppendLine("methods");
            B.AppendLine("***************");
            B.AppendLine();

            B.AppendLine("[params] [type] new [?param count] - calls the constructor for type");
            B.AppendLine("[params] [type params] [instance] calli [method] [?param count] [?type count] - call method for instance with params");
            B.AppendLine("[params] [type params] [instance] [type] callt [method] [?param count] [?type count] - call method from type for instance with params");
            B.AppendLine("[params] [type params] [type] calls [method] [?param count] [?type count] - call static method from type with params");
            B.AppendLine("******");

            B.Append(MethodHelp(Methods));
            return B.ToString();
        }

        private static string MethodHelp(IEnumerable<MethodBase> Methods)
        {
            StringBuilder B = new StringBuilder();

            foreach (var M in Methods)
            {
                B.Append(
$@"
{M.Name} 
    class {M.DeclaringType.FullName}
    {(M.IsStatic ? "static" : "instance")}
    generic arguments: {(M.IsGenericMethodDefinition ? M.GetGenericArguments().Length : 0)}
    returns {(M as MethodInfo)?.ReturnType.Name}
    parameters:{M.GetParameters().Select((x, i) => $"      " + x.ParameterType.Name + " " + x.Name).Aggregate("", (a, b) => a + Environment.NewLine + b, x => x)}");
                B.AppendLine();
            }
            return B.ToString();

        }

        public string Execute(string Input)
        {
            try
            {
                if (Input == null) Input = "";
                if (Input == "") return ExecuteSingle("");

                var Lines = SplitLines(Input);
                string Last = "";
                foreach (var L in Lines)
                {
                    Last = ExecuteSingle(L.Trim());
                }
                return Last;
            }
            catch (Exception ex)
            {
                return
                    $@"*****************
Exception:
{ex.GetAllExceptions().Select(x => x.GetType().Name + " - " + x.Message + "-->").Aggregate("", (a, b) => a + "\n\r" + b, x => x)}";
            }
        }

        private static string PrintType(Type Type)
        {
            StringBuilder B = new StringBuilder();
            B.AppendLine(Type.FullName);

            B.AppendLine(MethodHelp(Type.GetConstructors()));

            B.Append(MethodHelp(Type.GetMethods()));
            return B.ToString();
        }

        private static MethodBase SolveGenericMethod(MethodBase Method, Stack<object> Stack)
        {
            if (Method.IsGenericMethod)
            {
                var TypeCount = Method.GetGenericArguments().Length;
                var Types = GetStackParams(TypeCount, Stack);

                if (!Types.All(x => x is Type))
                {
                    throw new ArgumentException($"Expected generic type parameters for method {Method.Name}");
                }

                return ((MethodInfo)Method).MakeGenericMethod(Types.Cast<Type>().ToArray());
            }
            else
            {
                return Method;
            }
        }

        private static MethodBase SolveMethod(IEnumerable<MethodBase> Methods, string MethodName, int? ParamCount, int? TypeCount, Stack<object> ParamStack, out object[] Params)
        {
            Methods = Methods.Where(x => x.Name == MethodName);
            if (Methods.Count() == 1)
            {
                var M = Methods.First();
                if (ParamCount.HasValue && ParamCount.Value != M.GetParameters().Length)
                    throw new ArgumentException($"The argument count for the method {MethodName} ({M.GetParameters().Length}) is not equal to {ParamCount}");

                M = SolveGenericMethod(M, ParamStack);
                Params = GetStackParams(M.GetParameters().Length, ParamStack);
                return M;
            }
            if (Methods.Any() == false)
                throw new ArgumentException($"The method {MethodName} was not found");

            if (ParamCount == null)
                throw new ArgumentException
                    ($"The method {MethodName} have multiple overloads that cannot be solved. Specify {Methods.Select(x => x.GetParameters().Length).Aggregate("", (a, b) => a + ", " + b, x => x)} parameters count");


            Methods = Methods.Where(x => x.GetParameters().Length == ParamCount);

            var TypeArgumentCounts = Methods.Select(x => x.GetGenericArguments()).GroupBy(x => x.Length).Select(x => (int?)x.Key);
            if (TypeCount == null)
            {
                TypeCount = TypeArgumentCounts.SingleOrDefault();
            }

            if (TypeCount == null)
                throw new ArgumentException($"Could not determine the type argument count for the method {MethodName}, use {TypeArgumentCounts.Aggregate("", (a, b) => a + ", " + b, x => x)}");

            var Types = GetStackParams(TypeCount.Value, ParamStack).Cast<Type>();

            Params = GetStackParams(ParamCount.Value, ParamStack);


            if (Methods.Count() > 1)
                throw new ArgumentException($"The method {MethodName} with parameter count {ParamCount} have multiple overloads that cannot be solved");
            else if (!Methods.Any())
                throw new ArgumentException($"The method {MethodName} with parameter count {ParamCount} was not found");

            var Result = Methods.Single();
            if (Result.IsGenericMethodDefinition)
            {
                Result = ((MethodInfo)Result).MakeGenericMethod(Types.ToArray());
            }
            return Result;
        }

        string Call(string[] Words, Type Type, object Instance)
        {
            object[] Params;
            var Method = SolveMethod(Type.GetMethods(), Words[1], Words.Length >= 3 ? (int?)int.Parse(Words[2]) : null, Words.Length >= 4 ? (int?)int.Parse(Words[4]) : null, Stack, out Params);

            return InvokeMethod(Method, Instance, Params.ToArray());

        }
        string New(string[] Words, Type Type)
        {
            object[] Params;
            var Method = SolveMethod(Type.GetConstructors(), ".ctor", Words.Length >= 2 ? (int?)int.Parse(Words[1]) : null, 0, Stack, out Params);
            return InvokeMethod(Method, null, Params.ToArray());
        }

        string ExecuteSingle(string Input)
        {
            if (Input == "") return Help();

            var Words = Split(Input);


            switch (Words[0])
            {

                case "int":
                    {
                        Stack.Push(FromString((string)Words[1], typeof(int)));
                        return "ok";
                    }
                case "json":
                    {
                        var data = JsonConvert.SerializeObject(Stack.Pop());

                        return data;
                    }
                case "store":
                    {
                        Locals[Words[1]] = Stack.Pop();
                        return "ok";
                    }
                case "load":
                    {
                        var value = Locals[Words[1]];
                        Stack.Push(value);
                        return PrintValue(value);
                    }
                case "typeof":
                    {
                        var Type = Stack.Pop().GetType();
                        Stack.Push(Type);
                        return PrintType(Type);
                    }
                case "new":
                    {
                        return New(Words, (Type)Stack.Pop());
                    }
                case "calli":
                    {
                        return Call(Words, Stack.Peek().GetType(), Stack.Pop());
                    }
                case "callt":
                    {
                        return Call(Words, (Type)Stack.Pop(), Stack.Pop());

                    }
                case "calls":
                    {
                        return Call(Words, (Type)Stack.Pop(), null);
                    }
                case "null":
                    Stack.Push(null);
                    return "ok";
                case "str":
                    Stack.Push(Words[1]);
                    return "ok";
                case "cast":
                    {
                        var Type = (Type)Stack.Pop();
                        var Value = (string)Stack.Pop();
                        Stack.Push(FromString(Value, Type));
                    }
                    return "ok";
                case "types":
                    return Types.Select(x => x.FullName + Environment.NewLine).Aggregate("", (a, b) => a + b, x => x);
                case "type":
                    {
                        var GType = GetType(Words[1]);
                        Stack.Push(GType);
                        return PrintType(GType);

                    }
                case "reset":
                    Stack.Clear();
                    Locals.Clear();
                    return "ok";
                case "stack":
                    return
                        "Stack count: " + Stack.Count + Environment.NewLine +
                        Stack.Select(x => PrintValue(x) + Environment.NewLine).Aggregate("", (a, b) => a + b, x => x);
                case "dup":
                    Stack.Push(Stack.Peek());
                    return "ok";
                case "pop":
                    Stack.Pop();
                    return "ok";
                case "locals":
                    return "Locals count: " + Locals.Count + Environment.NewLine +
                        Locals.Select(x => x.Key + "-->" + PrintValue(x.Value) + Environment.NewLine).Aggregate("", (a, b) => a + b, x => x);

            }

            return ExecuteMethod(Input);



        }

        private object[] GetStackParams(int Count)
        {
            return GetStackParams(Count, Stack);
        }
        private static object[] GetStackParams(int Count, Stack<object> Stack)
        {
            var Params = new List<object>();
            for (int i = 0; i < Count; i++)
            {
                Params.Insert(0, Stack.Pop());
            }
            return Params.ToArray();
        }

        private string PrintValue(object Value)
        {
            var SerializedValue = Value.ToString();

            return $"type: {Value.GetType().FullName} {Environment.NewLine} data: {Environment.NewLine}{SerializedValue}";
        }

        private string InvokeMethod(MethodBase Method, object Instance, object[] Params)
        {
            var Result = (Method is ConstructorInfo) ? ((ConstructorInfo)Method).Invoke(Params) : Method.Invoke(Instance, Params);

            if (Method is ConstructorInfo || (Method is MethodInfo && ((MethodInfo)Method).ReturnType != typeof(void)))
            {
                Stack.Push(Result);
                return PrintValue(Result);
            }
            else
                return "void";

        }

        private string ExecuteMethod(string Input)
        {
            var Words = Split(Input);

            bool InstanceCall = Words[0].StartsWith(".");
            var MethodName = InstanceCall ? Words[0].Substring(1) : Words[0];

            var Method = Methods.Where(x => x.Name == MethodName).FirstOrDefault();

            if (Method == null)
                return $"Method {MethodName} not found";
            var MethodParams = Method.GetParameters();


            var Params = GetStackParams(MethodParams.Length);

            return InvokeMethod(Method, InstanceCall ? Stack.Pop() : Instance, Params.ToArray());
        }
    }
}
