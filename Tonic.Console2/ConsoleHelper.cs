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
    /// Log event args
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new LogEventArgs 
        /// </summary>
        public LogEventArgs(string data)
        {
            this.data = data;
        }
        readonly string data;

        /// <summary>
        /// Log data
        /// </summary>
        public string Data
        {
            get
            {
                return data;
            }
        }

    }

    /// <summary>
    /// Provides methods for calling .NET code from console
    /// </summary>
    public class ConsoleHelper
    {
        enum StackOp
        {
            Begin,
            End
        }

        /// <summary>
        /// Converts an string onto a value of the given type
        /// </summary>
        static object FromString(string Value, Type Type)
        {
            if (Value == "null")
                return null;

            var Converter = TypeDescriptor.GetConverter(Type);
            return Converter.ConvertFromString(Value);
        }


        static List<List<string>> Split(string Input)
        {
            return WordSplitter.SplitLines(Input);
        }



        private List<MethodInfo> Methods = new List<MethodInfo>();


        public Stack<object> Stack = new Stack<object>();
        Dictionary<string, object> Locals = new Dictionary<string, object>();
        List<Type> Types = new List<Type>();

        public event EventHandler<LogEventArgs> LogEvent;

        /// <summary>
        /// The instance that will be passed to named methods
        /// </summary>
        public object Instance;

        /// <summary>
        /// Create a new console helper
        /// </summary>
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
            Types.Add(typeof(System.IO.File));
        }

        /// <summary>
        /// Raise the LogEvent
        /// </summary>
        /// <param name="Data"></param>
        public void Log(string Data)
        {
            LogEvent?.Invoke(this, new LogEventArgs(Data));
        }

        /// <summary>
        /// Load all types from the given assembly
        /// </summary>
        /// <param name="AssemblyReference"></param>
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
        /// <summary>
        /// Load all methods from the given instance
        /// </summary>
        /// <param name="Instance"></param>
        public void LoadInstance(object Instance)
        {
            Methods.AddRange(Instance.GetType().GetMethods());
        }

        Type GetType(string Name)
        {
            var SimpleName = Types.Where(x => x.Name.ToLowerInvariant() == Name.ToLowerInvariant());
            if (SimpleName.Count() > 1)
                throw new ArgumentException($"Can't solve between {Types.Select(x => x.FullName + Environment.NewLine).Aggregate("", (a, b) => a + b, x => x)}");
            else if (SimpleName.Count() == 1)
            {
                return SimpleName.First();
            }

            var FullName = Types.Where(x => x.FullName.ToLowerInvariant() == Name.ToLowerInvariant()).FirstOrDefault();
            if (FullName == null)
                throw new ArgumentException($"Type {Name} not found");

            return FullName;
        }

        string Help()
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

            B.AppendLine("str [string] - loads [string] to the stack");
            B.AppendLine("int [value] - loads [value] to the stack");
            B.AppendLine("null - load null onto the stack");
            B.AppendLine("json - serialize stack item");

            B.AppendLine("variables");
            B.AppendLine("***************");
            B.AppendLine();

            B.AppendLine("store [var] - store the stack item onto [var] (shortcut form: >)");
            B.AppendLine("load [var] - load [var] onto the stack (shortcut form: <)");
            B.AppendLine("locals - Show all variables");


            B.AppendLine("types");
            B.AppendLine("***************");
            B.AppendLine();

            B.AppendLine("types - show all loaded types");
            B.AppendLine("[value] [type] cast  - cast [value] to [type]");
            B.AppendLine("type [type] - load the given type by name onto the stack (short form: #)");
            B.AppendLine("[instance] typeof - gets the type of the stack item");
            B.AppendLine("[value] [type] cast - cast value to type");


            B.AppendLine("methods");
            B.AppendLine("***************");
            B.AppendLine();

            B.AppendLine("[params] [type] array - create an array of type, (short form: $)");
            B.AppendLine("[params] [type] new - calls the constructor for type, (short form: !)");
            B.AppendLine("[params] [type params] [instance] calli [method] - call method for instance with params (short form: .)");
            B.AppendLine("[params] [type params] [type] calls [method] - call static method from type with params (short form: :)");
            B.AppendLine("[params] [type params] [instance] [type] callt [method] - call method from type for instance with params");
            B.AppendLine("await - create a task continuation that push the task result onto the tasks when the task has been finished");

            B.AppendLine("******");

            B.AppendLine();
            B.AppendLine("log - returns a delegate Action<string> that raises the console log event");
            B.AppendLine();
            B.AppendLine("exit - exit the app");


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

        /// <summary>
        /// Returns true if the program must exit
        /// </summary>
        public bool Exit
        {
            get; private set;
        }

        /// <summary>
        /// Execute the given input and returns the console output
        /// </summary>
        /// <param name="Input">The input to execute</param>
        /// <returns>The console output</returns>
        public async Task<string> Execute(string Input)
        {
            try
            {
                if (Input == null) Input = "";
                if (Input == "") return await ExecuteSingle(new string[] { "" }.ToList());

                var Lines = Split(Input);
                string Last = "";
                foreach (var L in Lines)
                {
                    Last = await ExecuteSingle(L);
                }
                return Last;
            }
            catch (Exception ex)
            {
                return
                    $@"*****************
Exception:
{ex.GetAllExceptions().Select(x => x.GetType().Name + " - " + x.Message + x.StackTrace + "-->").Aggregate("", (a, b) => a + "\n\r" + b, x => x)}";
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

        private static MethodBase SolveMethod(IEnumerable<MethodBase> Methods, string MethodName, Stack<object> ParamStack, out object[] Params)
        {

            Methods = Methods.Where(x => x.Name.ToLowerInvariant() == MethodName.ToLowerInvariant());

            if (Methods.Any() == false)
                throw new ArgumentException($"The method {MethodName} was not found");

            bool Generic;
            if (Methods.Any(x => x.IsGenericMethod))
                Generic = true;
            else
                Generic = false;

            Type[] typeParams = null;
            if (Generic)
            {
                typeParams = GetStackParams(ParamStack).Cast<Type>().ToArray();
                Methods = Methods.Where(x => x.GetGenericArguments().Length == typeParams.Length);
            }

            if (Methods.All(x => x.GetParameters().Length == 0))
                Params = new object[0];
            else
                Params = GetStackParams(ParamStack).ToArray();

            int paramLenght = Params.Length;
            Methods = Methods.Where(x => x.GetParameters().Length == paramLenght);


            if (Methods.Count() > 1)
                throw new ArgumentException($"The method {MethodName} with parameter count {paramLenght} have multiple overloads that cannot be solved");
            else if (!Methods.Any())
                throw new ArgumentException($"The method {MethodName} with parameter count {paramLenght} was not found");

            var Result = Methods.Single();
            if (Result.IsGenericMethodDefinition)
            {
                Result = ((MethodInfo)Result).MakeGenericMethod(typeParams);
            }
            return Result;
        }

        string Call(IReadOnlyList<string> Words, Type Type, object Instance)
        {
            object[] Params;
            var Method = SolveMethod(Type.GetMethods(), Words[1], Stack, out Params);

            return InvokeMethod(Method, Instance, Params.ToArray());

        }
        string New(IReadOnlyList<string> Words, Type Type)
        {
            object[] Params;
            var Method = SolveMethod(Type.GetConstructors(), ".ctor", Stack, out Params);
            return InvokeMethod(Method, null, Params.ToArray());
        }

        string CreateArray(IReadOnlyList<string> Words, Type Type)
        {
            var Params = GetStackParams(Stack);

            var Instance = Array.CreateInstance(Type, Params.Length);
            for (int i = 0; i < Params.Length; i++)
            {
                Instance.SetValue(Params[i], i);
            }
            Stack.Push(Instance);
            return $"array[{Params.Length}]";
        }

        async Task<string> ExecuteSingle(List<string> Words)
        {
            if (Words[0] == "exit")
            {
                Exit = true;
                return "bye";
            }

            {
                //Parse numbers;
                decimal value;
                if (decimal.TryParse(Words[0], out value))
                {
                    if (Math.Round(value) == value)
                        Stack.Push((int)value);
                    else
                        Stack.Push((float)value);
                    return value.ToString();
                }
            }

            //Shortcuts:
            {
                if (Words[0] == "?" || Words[0] == "") return Help();
                if (Words[0].StartsWith("."))
                {
                    Words.Add(Words[0].Substring(1));
                    Words[0] = "calli";
                }
                if (Words[0].StartsWith(":"))
                {
                    Words.Add(Words[0].Substring(1));
                    Words[0] = "calls";
                }
                if (Words[0].StartsWith("#"))
                {
                    Words.Add(Words[0].Substring(1));
                    Words[0] = "type";
                }
                if (Words[0].StartsWith("!"))
                {
                    Words.Add(Words[0].Substring(1));
                    Words[0] = "new";
                }
                if (Words[0].StartsWith("$"))
                {
                    Words.Add(Words[0].Substring(1));
                    Words[0] = "array";
                }
                if (Words[0].StartsWith(">"))
                {
                    Words.Add(Words[0].Substring(1));
                    Words[0] = "store";
                }
                if (Words[0].StartsWith("<"))
                {
                    Words.Add(Words[0].Substring(1));
                    Words[0] = "load";
                }
            }



            switch (Words[0])
            {
                case "await":
                    {
                        var Task = (Task)Stack.Pop();
                        bool HaveResult = Task.GetType().IsSubClassOfGeneric(typeof(Task<>));

                        await Task;

                        if (HaveResult)
                        {
                            var Result = Task.GetType().GetProperty(nameof(Task<int>.Result)).GetValue(Task);
                            Stack.Push(Result);
                            return PrintValue(Result);
                        }
                        else
                            return "void";
                    }
                case "log":
                    {
                        Stack.Push((Action<string>)Log);
                        return "str = > log(str)";
                    }
                case "(":
                    {
                        Stack.Push(StackOp.Begin);
                        return "";
                    }
                case ")":
                    {
                        Stack.Push(StackOp.End);
                        return "";
                    }
                case "current":
                    {
                        Stack.Push(Instance);
                        return PrintValue(Instance);
                    }
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
                case "array":
                    {
                        if (Words.Count >= 2)
                            return CreateArray(Words, GetType(Words[1]));

                        else
                            return CreateArray(Words, (Type)Stack.Pop());
                    }
                case "new":
                    {
                        if (Words.Count >= 2)
                            return New(Words, GetType(Words[1]));

                        else
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
                        var Result = FromString(Value, Type);
                        Stack.Push(Result);
                        return PrintValue(Result);
                    }
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
                        Stack.Select(x => PrintValue(x) + Environment.NewLine + "____________" + Environment.NewLine).Aggregate("", (a, b) => a + b, x => x);
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

            return ExecuteMethod(Words[0]);

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

        private static object[] GetStackParams(Stack<object> Stack)
        {
            var End = Stack.Pop();
            if (!object.Equals(End, StackOp.End))
                throw new ArgumentException("')' operator expected");

            int balance = 0;

            List<object> result = new List<object>();
            while (true)
            {
                var value = Stack.Pop();
                if (object.Equals(value, StackOp.Begin))
                    balance--;
                else if (object.Equals(value, StackOp.End))
                    balance++;
                else
                    result.Insert(0, value);

                if (balance < 0)
                    break;

            }
            return result.ToArray();
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
            var Words = Split(Input)[0].ToArray();

            bool InstanceCall = Words[0].StartsWith(".");
            var MethodName = InstanceCall ? Words[0].Substring(1) : Words[0];

            var Method = Methods.Where(x => x.Name.ToLowerInvariant() == MethodName.ToLowerInvariant()).FirstOrDefault();

            if (Method == null)
                return $"Method {MethodName} not found";
            var MethodParams = Method.GetParameters();


            var Params = GetStackParams(MethodParams.Length);

            return InvokeMethod(Method, InstanceCall ? Stack.Pop() : Instance, Params.ToArray());
        }
    }
}
