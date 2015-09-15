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
        /// Calls the where method
        /// </summary>
        /// <param name="Query"></param>
        /// <param name="Predicate"></param>
        /// <returns></returns>
        public static IQueryable Where(IQueryable Query, Expression Predicate)
        {
            return (IQueryable)CallStatic(Query, x => x.Where(y => y == 1), Predicate);
        }

        /// <summary>
        /// Returns an expression that test each property equality to a given value
        /// </summary>
        /// <param name="PropertyValues">A collection that matches the property and the values to test</param>
        /// <returns></returns>
        public static Expression PredicateEqual(Type ElementType, IEnumerable<Tuple<string, object>> PropertyValues)
        {
            var Instance = Expression.Parameter(ElementType);
            Expression REx = null;
            foreach (var P in PropertyValues)
            {
                var Property = Expression.Property(Instance, P.Item1);
                var Value = Expression.Constant(P.Item2);

                var Eq = Expression.Equal(Property, Value);
                if (REx == null)
                    REx = Eq;
                else
                    REx = Expression.And(REx, Eq);
            }

            var Lambda = Expression.Lambda(REx, Instance);
            return Lambda;
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
        /// Calls the generic ToList method
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
