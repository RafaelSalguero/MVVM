using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tonic
{
    /// <summary>
    /// Provides methods for calling generic IEnumerable and IQueryable methods
    /// </summary>
    public static class RLinq
    {
        static string SqlToString(object Value)
        {
            if (Value == null)
            {
                return "null";
            }

            var T = Value.GetType();
            if (T.IsGenericType && T.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Value = ((dynamic)Value).Value;
            }

            if (Value is DateTime)
            {
                var aux = (DateTime)Value;
                return aux.ToString("yyyy-MM-dd");
            }
            else
                return Value.ToString();
        }

        /// <summary>
        /// Format an SQL statement onto a human friendly format
        /// </summary>
        /// <param name="Sql"></param>
        /// <returns></returns>
        public static string FormatSql(string Sql, params object[] Params)
        {
            var Extents = new Dictionary<string, string>();
            var Replaces = new Dictionary<string, string>();
            StringBuilder B = new StringBuilder();
            int indent = 0;
            int subindent = 0;

            for (int i = 0; i < Params.Length; i++)
            {
                Replaces.Add($"@p__linq__{i}", SqlToString(Params[i]));
            }

            Action<int> NewLineI = (indentX) =>
            {
                if (B.Length > 0 && B[B.Length - 1] != '\n')
                {
                    B.AppendLine();
                    B.Append(' ', 3 * (indentX));
                }
            };
            Action NewLine = () =>
            {
                NewLineI(indent + subindent);
            };


            Func<int, string, bool> OnWordI = (i, word) =>
             {
                 if (word.Length > i || i >= Sql.Length)
                     return false;
                 else
                 {
                     for (int j = i; j > i - word.Length; j--)
                     {
                         if (Sql[j] != word[j - i + word.Length - 1])
                             return false;
                     }
                     return true;
                 }
             };

            Func<int, string> GetLastWord = (index) =>
             {
                 int i;
                 for (i = index; i >= 0; i--)
                 {
                     if (Sql[i] == ' ')
                     {
                         break;
                     }
                 }
                 return Sql.Substring(i + 1, index - i);
             };

            Func<int, string> GetLastDotWord = (index) =>
              {
                  int i;
                  for (i = index; i >= 0; i--)
                  {
                      if (Sql[i] == '.')
                      {
                          break;
                      }
                  }
                  return Sql.Substring(i + 1, index - i).Trim('"');
              };

            Func<int, string> GetNextWord = (index) =>
            {
                int i;
                for (i = index; i < Sql.Length; i++)
                {
                    if (Sql[i] == ' ')
                    {
                        break;
                    }
                }
                return Sql.Substring(index, i - index);
            };




            var jumpWords = new[] { "WHERE", "JOIN", "FROM" };

            var indents = new Stack<int>();
            for (int i = 0; i < Sql.Length; i++)
            {
                Func<string, bool> OnWord = x => OnWordI(i, x);

                var c = Sql[i];
                if (c == ')')
                {
                    indent--;
                    NewLineI(indents.Pop());
                    subindent = 0;
                }
                else if (c == '(')
                {
                    NewLine();
                    indents.Push(indent + subindent);
                }
                else if (jumpWords.Select(x => OnWordI(i + x.Length, x)).Any(x => x))
                {
                    NewLine();
                }

                B.Append(c);
                if (c == ',')
                {
                    NewLine();
                }
                else if (c == '(')
                {
                    indent++;
                    NewLine();
                }
                else if (OnWord("and"))
                {
                    NewLine();
                }
                else if (jumpWords.Select(x => OnWord(x)).Where(x => x).Any(x => x))
                {
                    subindent++;
                    NewLine();
                }

                var extent = " as \"Extent";
                if (OnWord(extent))
                {
                    var Name = GetLastWord(i - extent.Length);
                    var Extent = GetNextWord(i - "Extent".Length);
                    var N = Name.Substring(Name.LastIndexOf('.') + 2).Trim('"');
                    var FriendlyE = "\""
                        + N.Substring(0, Math.Min(3, N.Length)) + "_e" + Extent.Substring("Extent".Length + 1).Trim('"')
                        + "\"";

                    var Friendly =
                     N.Substring(0, Math.Min(3, N.Length));

                    Extents.Add(Extent, Name);

                    if (Replaces.ContainsValue(Friendly))
                        Replaces.Add(Extent, FriendlyE);
                    else
                        Replaces.Add(Extent, Friendly);
                }
            }

            StringBuilder Comments = new StringBuilder();
            foreach (var ex in Extents)
            {
                Comments.Append("-- ");
                Comments.Append(ex.Key);
                Comments.Append(" == ");
                Comments.Append(ex.Value);
                Comments.AppendLine();
            }

            Comments.AppendLine();
            Comments.Append(B.ToString());

            foreach (var R in Replaces)
            {
                Comments.Replace(R.Key, R.Value);
            }
            return Comments.ToString();
        }

        /// <summary>
        /// Gets whether a collection implements the ICollection(T) interface 
        /// </summary>
        /// <param name="Collection"></param>
        /// <returns></returns>
        public static bool IsICollectionOfT(IEnumerable Collection)
        {
            var collectionType = Collection.GetType();
            return IsICollectionOfT(collectionType);
        }

        /// <summary>
        /// Gets whether a collection implements the ICollection(T) interface 
        /// </summary>
        /// <param name="CollectionType">Collection type</param>
        public static bool IsICollectionOfT(Type CollectionType)
        {
            Func<Type, bool> predicate = x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>);

            if (predicate(CollectionType))
                return true;

            return CollectionType
            .GetInterfaces()
            .Where(predicate)
            .Any();
        }

        /// <summary>
        /// Gets the first type argument of a generic IEnumerable 
        /// </summary>
        /// <param name="Collection"></param>
        /// <returns></returns>
        public static Type GetEnumerableType(this IEnumerable Collection)
        {
            var collectionType = Collection.GetType();
            var IEnumerable =
                collectionType
                .GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .First();

            return IEnumerable.GetGenericArguments()[0];
        }

        /// <summary>
        /// Gets the first type argument of a generic IEnumerable 
        /// </summary>
        /// <param name="Collection"></param>
        /// <returns></returns>
        public static Type GetEnumerableType(Type collectionType)
        {
            Func<Type, bool> predicate = x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>);
            if (predicate(collectionType))
                return collectionType.GetGenericArguments()[0];

            var IEnumerable =
                collectionType
                .GetInterfaces()
                .Where(predicate)
                .First();

            return IEnumerable.GetGenericArguments()[0];
        }


        /// <summary>
        /// Calls a given static method 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="Query"></param>
        /// <param name="Method"></param>
        /// <returns></returns>
        public static void CallStatic(IEnumerable Query, Expression<Action<ICollection<int>>> Method)
        {
            var collectionType = Query.GetType();
            var IQueryable =
                collectionType
                .GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>))
                .First();

            var ElementType = IQueryable.GetGenericArguments()[0];

            var mc = ((MethodCallExpression)Method.Body)
                .Method
                 .GetGenericMethodDefinition()
                   .MakeGenericMethod(ElementType);

            mc.Invoke(null, new object[] { Query });
        }



        /// <summary>
        /// Calls a given static method 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="Query"></param>
        /// <param name="Method"></param>
        /// <returns></returns>
        public static object CallStatic<T>(IEnumerable Query, Expression<Func<ICollection<int>, T>> Method)
        {
            var collectionType = Query.GetType();
            var IQueryable =
                collectionType
                .GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>))
                .FirstOrDefault();

            if (IQueryable == null)
            {
                IQueryable =
                collectionType
                .GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .FirstOrDefault();
            }

            var ElementType = IQueryable.GetGenericArguments()[0];

            var mc = ((MethodCallExpression)Method.Body)
                .Method
                 .GetGenericMethodDefinition()
                   .MakeGenericMethod(ElementType);

            return mc.Invoke(null, new object[] { Query });
        }



        /// <summary>
        /// Calls a given static method 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="Query"></param>
        /// <param name="Method"></param>
        /// <returns></returns>
        public static object CallStatic<T>(IQueryable Query, Expression<Func<IQueryable<int>, T>> Method)
        {
            var collectionType = Query.GetType();
            var IQueryable =
                collectionType
                .GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IQueryable<>))
                .First();

            var ElementType = IQueryable.GetGenericArguments()[0];

            var mc = ((MethodCallExpression)Method.Body)
                .Method
                 .GetGenericMethodDefinition()
                   .MakeGenericMethod(ElementType);

            return mc.Invoke(null, new object[] { Query });
        }

        /// <summary>
        /// Calls a given static method 
        /// </summary>
        /// <returns></returns>
        public static object CallStatic<TInput>(IQueryable Query, Expression<Func<IQueryable<int>, TInput>> Method, object Arg1)
        {
            var collectionType = Query.GetType();
            var IQueryable =
                collectionType
                .GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IQueryable<>))
                .First();

            var ElementType = IQueryable.GetGenericArguments()[0];

            var mc = ((MethodCallExpression)Method.Body)
                .Method
                 .GetGenericMethodDefinition()
                   .MakeGenericMethod(ElementType);

            return mc.Invoke(null, new object[] { Query, Arg1 });
        }

        /// <summary>
        /// Calls the first LINQ method via reflection
        /// </summary>
        public static object First(IQueryable Query)
        {
            return CallStatic(Query, x => x.First());
        }

        /// <summary>
        /// Calls the first LINQ method via reflection
        /// </summary>
        public static object First(IEnumerable Query)
        {
            return CallStatic(Query, x => x.First());
        }




        /// <summary>
        /// Calls the where LINQ method via reflection
        /// </summary>
        public static IQueryable Where(IQueryable Query, Expression Predicate)
        {
            return (IQueryable)CallStatic(Query, x => x.Where(y => y == 1), Predicate);
        }

        /// <summary>
        /// Calls the where LINQ method via reflection
        /// </summary>
        public static IEnumerable Where(IEnumerable Query, Expression Predicate)
        {
            return Where(Query.AsQueryable(), Predicate);
        }


        /// <summary>
        /// Calls the Add method from an object that implements the IQuerable(T) method
        /// </summary>
        public static IQueryable Select(IQueryable ICollectionOfT, string PropertyName)
        {
            var collectionType = ICollectionOfT.GetType();
            var IQueryable =
                collectionType
                .GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IQueryable<>))
                .First();

            var ElementType = IQueryable.GetGenericArguments()[0];

            var xParam = Expression.Parameter(ElementType);
            var Property = Expression.Property(xParam, PropertyName);

            var SelectArgument = Expression.Lambda(Property, xParam);

            var mc = ((MethodCallExpression)
               (((Expression<Func<IEnumerable<int>>>)
               (() => (new int[0]).AsQueryable().Select(x => 1)))
               .Body)).Method
               .GetGenericMethodDefinition()
               .MakeGenericMethod(ElementType, Property.Type);

            return (IQueryable)mc.Invoke(null, new object[] { ICollectionOfT, SelectArgument });
        }

        /// <summary>
        /// Calls the Add method from an object that implements the ICollection(T) method
        /// </summary>
        /// <param name="ICollectionOfT"></param>
        /// <param name="Item"></param>
        public static void Add(IEnumerable ICollectionOfT, object Item)
        {
            var collectionType = ICollectionOfT.GetType();
            var ICollection =
                collectionType
                .GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>))
                .First();

            ICollection.GetMethod("Add").Invoke(ICollectionOfT, new object[] { Item });
        }

        /// <summary>
        /// Calls the Remove method from an object that implements the ICollection(T) method
        /// </summary>
        /// <param name="ICollectionOfT"></param>
        /// <param name="Item"></param>
        public static void Remove(IEnumerable ICollectionOfT, object Item)
        {
            var collectionType = ICollectionOfT.GetType();
            var ICollection =
                collectionType
                .GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>))
                .First();

            ICollection.GetMethod("Remove").Invoke(ICollectionOfT, new object[] { Item });
        }

        /// <summary>
        /// Apply the Where (x=> true) operator to the given query
        /// </summary>
        /// <param name="Collection"></param>
        /// <returns></returns>
        public static IQueryable WhereTrue(IQueryable Collection)
        {
            var ElementType = Collection.ElementType;

            var mc = ((MethodCallExpression)
                   (((Expression<Func<IEnumerable<int>>>)
                   (() => (new int[0]).AsQueryable().Where(x => true)))
                   .Body)).Method
                   .GetGenericMethodDefinition()
                   .MakeGenericMethod(ElementType);

            var param = Expression.Parameter(ElementType);
            var predicate = Expression.Lambda(Expression.Constant(true), param);

            var ret = (IQueryable)mc.Invoke(null, new object[] { Collection, predicate });
            return ret;
        }



        /// <summary>
        /// Calls the generic ToList method via reflection
        /// </summary>
        public static IEnumerable ToList(IEnumerable Collection)
        {
            var EnumerableInterface =
                   Collection.GetType()
                   .GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)).First();

            if (EnumerableInterface == null)
                throw new ArgumentException("Collection must implement the generic interface IEnumerable");

            var ElementType = EnumerableInterface.GetGenericArguments()[0];

            var mc = ((MethodCallExpression)
                   (((Expression<Func<IEnumerable<int>>>)
                   (() => (new int[0]).ToList()))
                   .Body)).Method
                   .GetGenericMethodDefinition()
                   .MakeGenericMethod(ElementType);

            return (IEnumerable)mc.Invoke(null, new object[] { Collection });
        }

        /// <summary>
        /// Calls the generic Count method
        /// </summary>
        public static int Count(IEnumerable Collection)
        {
            return CountQueryable(AsQueryable(Collection));
        }

        /// <summary>
        /// Calls the generic Count method
        /// </summary>
        private static int CountQueryable(IQueryable Collection)
        {
            var EnumerableInterface =
                   Collection.GetType()
                   .GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IQueryable<>)).First();

            if (EnumerableInterface == null)
                throw new ArgumentException("Collection must implement the generic interface IQueryable");

            var ElementType = EnumerableInterface.GetGenericArguments()[0];

            var mc = ((MethodCallExpression)
                   (((Expression<Func<int>>)
                   (() => (new int[0]).AsQueryable().Count()))
                   .Body)).Method
                   .GetGenericMethodDefinition()
                   .MakeGenericMethod(ElementType);

            return (int)mc.Invoke(null, new object[] { Collection });
        }
        /// <summary>
        /// Calls the generic AsQueryable method
        /// </summary>
        public static IQueryable AsQueryable(IEnumerable Collection)
        {
            if (Collection is IQueryable)
            {
                return (IQueryable)Collection;
            }

            var EnumerableInterface =
           Collection.GetType()
           .GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)).First();

            if (EnumerableInterface == null)
                throw new ArgumentException("Collection must implement the generic interface IEnumerable");

            var ElementType = EnumerableInterface.GetGenericArguments()[0];

            var mc = ((MethodCallExpression)
                   (((Expression<Func<IEnumerable<int>>>)
                   (() => (new int[0]).AsQueryable()))
                   .Body)).Method
                   .GetGenericMethodDefinition()
                   .MakeGenericMethod(ElementType);

            return (IQueryable)mc.Invoke(null, new object[] { Collection });
        }

        /// <summary>
        /// Apply an orderby to a collection of elements
        /// </summary>
        /// <param name="Collection">A collection that implements the generic IEnumerable or IQueryable interfaces</param>
        /// <param name="PropertyName">The property to use as an argument to the OrderBy</param>
        /// <param name="Descending">Descensing sorting</param>
        /// <returns></returns>
        public static IEnumerable OrderBy(IEnumerable Collection, string PropertyName, bool Descending)
        {
            var EnumerableInterface =
                Collection.GetType()
                .GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)).First();

            if (EnumerableInterface == null)
                throw new ArgumentException("Collection must implement the generic interface IEnumerable");

            var ElementType = EnumerableInterface.GetGenericArguments()[0];
            //var Property = ElementType.GetProperty(PropertyName);

            var QueryableInterface =
                Collection.GetType()
                .GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IQueryable<>)).FirstOrDefault();

            MethodCallExpression mc;
            if (QueryableInterface != null)
            {
                if (Descending)
                    mc = (MethodCallExpression)
                        (((Expression<Func<IQueryable<int>>>)
                        (() => (new int[0]).AsQueryable().OrderByDescending(y => y)))
                        .Body);
                else
                    mc = (MethodCallExpression)
                     (((Expression<Func<IQueryable<int>>>)
                     (() => (new int[0]).AsQueryable().OrderBy(y => y)))
                     .Body);
            }
            else
            {
                if (Descending)
                    mc = (MethodCallExpression)
                     (((Expression<Func<IEnumerable<int>>>)
                     (() => (new int[0]).OrderByDescending(y => y)))
                     .Body);
                else
                    mc = (MethodCallExpression)
                            (((Expression<Func<IEnumerable<int>>>)
                            (() => (new int[0]).OrderBy(y => y)))
                            .Body);
            }


            var param = Expression.Parameter(ElementType);
            var PropNames = PropertyName.Split('.');
            var PropertyType = ElementType;
            Expression propertyExpression = param;
            foreach (var P in PropNames)
            {
                PropertyType = PropertyType.GetProperty(P).PropertyType;
                propertyExpression = Expression.Property(propertyExpression, P);
            }



            MethodInfo OrderByMethod;
            OrderByMethod =
                 mc
                 .Method
                 .GetGenericMethodDefinition()
                 .MakeGenericMethod(ElementType, PropertyType);


            var PropertyGetter = Expression.Lambda(propertyExpression, param);

            var Ordered =
                QueryableInterface != null ?
                OrderByMethod.Invoke(null, new object[] { Collection, PropertyGetter }) :
                OrderByMethod.Invoke(null, new object[] { Collection, PropertyGetter.Compile() });

            return (IEnumerable)Ordered;
        }
    }
}
