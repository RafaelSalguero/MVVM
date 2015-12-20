using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.Patterns.PagedQuery.EF
{
    public static class PagedQueryEFExtensions
    {
        public static IQueryable<T> ToPagedQuery<T>(this DbSet<T> Set)
            where T : class
        {
            return ToPagedQuery(Set, false);
        }
        public static IQueryable<T> ToPagedQueryAsync<T>(this DbSet<T> Set)
            where T : class
        {
            return ToPagedQuery(Set, true);
        }
        private static IQueryable<T> ToPagedQuery<T>(this DbSet<T> Set, bool async)
            where T : class
        {
            var type = typeof(DbSet<T>);
            var InternalSetProperty = type.GetProperty("System.Data.Entity.Internal.Linq.IInternalSetAdapter.InternalSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (InternalSetProperty == null)
                throw new ArgumentException("Could not find the InternalSet property from the DbSet. Check your EF version");

            var InternalSet = InternalSetProperty.GetValue(Set);
            var InternalSetType = InternalSet.GetType();
            var InternalContextProperty = InternalSetType.GetProperty("InternalContext");

            if (InternalContextProperty == null)
                throw new ArgumentException("Could not find the InternalContext property from the InternalSet. Check your EF version");

            var InternalContext = InternalContextProperty.GetValue(InternalSet);

            var ConnectionStringProperty = InternalContext.GetType().GetProperty("OriginalConnectionString");

            if (InternalContextProperty == null)
                throw new ArgumentException("Could not find the OriginalConnectionString property from the InternalContext. Check your EF version");

            var ConnectionString = ConnectionStringProperty.GetValue(InternalContext);

            var OwnerProperty = InternalContext.GetType().GetProperty("Owner");
            if (OwnerProperty == null)
                throw new ArgumentException("Could not find the Owner property from the InternalContext. Check your EF version");
            var Owner = OwnerProperty.GetValue(InternalContext);

            var OwnerType = Owner.GetType();

            var OwnerConstructor = OwnerType.GetConstructor(new Type[] { typeof(string) });

            if (OwnerConstructor == null)
            {
                throw new ArgumentException("The DbContext '" + Owner.GetType().FullName + "' doesn't implement the connection string public constructor. Add a constructor with a single string parameter as the connection string");
            }

            var EntitySetProperty = InternalSetType.GetProperty("EntitySet");
            if (EntitySetProperty == null)
            {
                throw new ArgumentException("Could not find the EntitySet property. Check your EF version");
            }

            var EntitySetName = EntitySetProperty.GetValue(InternalSet).ToString();

            var ContextCtorExpression = Expression.New(OwnerConstructor, Expression.Constant(ConnectionString));


            //Call the Create method from the QueryFactory:
            var ContextCreatorType = typeof(Func<>).MakeGenericType(OwnerType);
            var QueryCreatorType = typeof(Func<,>).MakeGenericType(OwnerType, typeof(IQueryable<T>));

            var ContextCtor = Expression.Lambda(ContextCreatorType, ContextCtorExpression).Compile();

            var contextParam = Expression.Parameter(OwnerType);

            var queryCtor = Expression.Lambda(QueryCreatorType, Expression.Property(contextParam, EntitySetName), contextParam).Compile();

            Expression<Func<IQueryable<int>>> CreateMethod;

            if (async)
                CreateMethod = () => QueryFactory.CreateAsync<int, int>(() => 1, x => new int[0].AsQueryable(), 0, 0, false);
            else
                CreateMethod = () => QueryFactory.Create<int, int>(() => 1, x => new int[0].AsQueryable(), 0, 0, false);

            var MethodInfo =
                ((MethodCallExpression)CreateMethod.Body).Method
                .GetGenericMethodDefinition()
                .MakeGenericMethod(typeof(T), OwnerType);

            int PageCount = QueryFactory.DefaultPageCount, PageSize = QueryFactory.DefaultPageSize;

            var MethodCallResult = MethodInfo.Invoke(null, new object[] { ContextCtor, queryCtor, PageSize, PageCount, false });

            return (IQueryable<T>)MethodCallResult;
        }
    }
}
