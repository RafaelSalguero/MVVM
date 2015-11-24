using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace Tonic.EF
{
    public static class DbContextExtensions
    {
        [ThreadStatic]
        private static Dictionary<Type, string[]> primaryKeys = new Dictionary<Type, string[]>();

        /// <summary>
        /// Gets the property names that have the [Key] attribute
        /// </summary>
        /// <param name="EntityType">The entity type to obtain its primary key</param>
        /// <returns></returns>
        private static string[] GetPrimaryKey(Type EntityType)
        {
            string[] r;
            if (!primaryKeys.TryGetValue(EntityType, out r))
            {
                r = EntityType.GetProperties().Where(x => x.GetCustomAttribute<KeyAttribute>() != null).Select(x => x.Name).ToArray();
                primaryKeys.Add(EntityType, r);
            }
            return r;
        }


        public static bool IsNavProp(PropertyInfo P) =>
           P.CanRead &&
           P.CanWrite &&
           P.GetGetMethod().IsVirtual &&
           !Tonic.RLinq.IsICollectionOfT(P.PropertyType) &&
           !P.PropertyType.IsValueType &&
           P.PropertyType != typeof(string);

        public static bool IsFKCollection(PropertyInfo P) => P.CanRead && P.CanWrite && P.GetGetMethod().IsVirtual && Tonic.RLinq.IsICollectionOfT(P.PropertyType);

        public static bool IsColumn(PropertyInfo P) => P.CanRead && P.CanWrite && P.GetCustomAttribute<NotMappedAttribute>() == null && !IsNavProp(P) && !IsFKCollection(P);


        /// <summary>
        /// Gets the property names that have the [Key] attribute
        /// </summary>
        /// <param name="EntityType">The entity type to obtain its primary key</param>
        /// <returns></returns>
        private static IEnumerable<Tuple<string, object>> GetPrimaryKey(object Entity)
        {
            var EntityType = Entity.GetType();
            foreach (var P in GetPrimaryKey(EntityType))
            {
                yield return Tuple.Create(P, EntityType.GetProperty(P).GetValue(Entity));
            }
        }

        private static string PKToString(object Entity)
        {
            var PK = GetPrimaryKey(Entity);
            string r = "";
            foreach (var P in PK)
            {
                if (r != "")
                    r += ", ";
                r += $"{P.Item1} = {P.Item2}";
            }
            return r;
        }

        /// <summary>
        /// Gets an entity that have the same primary key as the detached entity, attached to the given context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="DetachedEntity">The detached entity that contains the primary key</param>
        /// <returns></returns>
        public static T GetEntity<T>(this DbSet<T> Set, T DetachedEntity) where T : class
        {
            var PK = GetPrimaryKey(DetachedEntity);
            var Predicate = Tonic.PredicateBuilder.PredicateEqual(typeof(T), PK);

            var Where = Tonic.RLinq.Where(Set, Predicate);

            var First = (T)Tonic.RLinq.CallStatic(Where, x => x.FirstOrDefault());

            if (First == null)
                throw new ArgumentException($"The entity of type {typeof(T).FullName} with the PK {PKToString(DetachedEntity)} was not found");
            else
                return First;
        }


        /// <summary>
        /// Check if an entity can be deleted from the table. Returns false if the server will throw an validation exception
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Set">The set parent of the entity</param>
        /// <param name="DetachedEntity">The entity to test</param>
        /// <returns></returns>
        public static bool CanDelete<T>(Func<DbContext> Context, T Entity)
            where T : class
        {
            try
            {
                using (var C = Context())
                {
                    using (var Transaction = C.Database.BeginTransaction())
                    {
                        var S = C.Set<T>();
                        T E = S.GetEntity(Entity);

                        S.Remove(E);
                        C.SaveChanges();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public static IEnumerable<Type> GetSetTypes(this DbContext Context)
        {
            foreach (var P in Context.GetType().GetProperties())
            {
                if (P.PropertyType.IsGenericType && P.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                    yield return P.PropertyType;
            }
        }

        public static void DeleteOrphans<T, TProperty>(this DbSet<T> Set, Expression<Func<T, TProperty>> ParentProperty)
            where T : class
        {
            var Prop = (PropertyInfo)((MemberExpression)ParentProperty.Body).Member;

            var Items = Set.Local.Where(x => Prop.GetValue(x) == null).ToList();
            foreach (var It in Items)
            {
                Set.Remove(It);
            }
        }
    }
}
