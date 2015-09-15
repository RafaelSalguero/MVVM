using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tonic.EF;
namespace Tonic.MVVM
{
    /// <summary>
    /// Entity view model, opens short-lived DbContext instances for retriving navigation properties
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EntityViewModel<TEntity, TContext> : ExposedViewModel<TEntity>, IEntityOperator
        where TEntity : class
        where TContext : DbContext
    {
        private readonly Func<TContext> context;
        /// <summary>
        /// Persistence context
        /// </summary>
        public Func<TContext> Context
        {
            get
            {
                return context;
            }
        }

        private Queue<EntityOperation> operations = new Queue<EntityOperation>();

        class PropertyCache
        {
            public PropertyCache(PropertyInfo Property)
            {
                this.Property = Property;
            }
            public PropertyInfo Property { get; private set; }
            public object Value { get; set; }
        }

        private Dictionary<string, PropertyCache> collections = new Dictionary<string, PropertyCache>();
        private PropertyInfo[] columnProperties;
        public event EventHandler<EntityOperationArgs> InformEntityOperation;

        public EntityViewModel(Func<TContext> Context)
        {
            this.context = Context;

            foreach (var P in typeof(TEntity).GetProperties())
            {
                if (P.GetGetMethod().IsVirtual && Tonic.RLinq.IsICollectionOfT(P.PropertyType))
                    collections.Add(P.Name, new PropertyCache(P));
            }

            columnProperties = typeof(TEntity).GetProperties().Where(DbContextExtensions.IsColumn).ToArray();
        }

        protected override object DynamicGet(string PropertyName)
        {
            PropertyCache aux;
            if (collections.TryGetValue(PropertyName, out aux))
            {
                return GetCollection(aux.Property);
            }
            else
                return base.DynamicGet(PropertyName);
        }

        private object GetCollection(PropertyInfo Property)
        {
            var TChild = Tonic.RLinq.GetEnumerableType(Property.PropertyType);
            Expression<Action> Ex = () => this.GetCollection<string>(Property.Name);
            var Method = ((MethodCallExpression)Ex.Body).Method.GetGenericMethodDefinition().MakeGenericMethod(TChild);
            return Method.Invoke(this, new object[] { Property });
        }

        protected ICollection<EntityViewModel<TChild, TContext>> GetCollection<TChild>(string Property)
               where TChild : class
        {
            return GetCollection<TChild, EntityViewModel<TChild, TContext>>(Property, c => new EntityViewModel<TChild, TContext>(c));
        }
        protected ICollection<TChildViewModel> GetCollection<TChildViewModel>(string PropertyName, Func<Func<TContext>, TChildViewModel> Factory)
        {
            var Property = collections[PropertyName].Property;
            var TChild = Tonic.RLinq.GetEnumerableType(Property.PropertyType);
            Expression<Action> Ex = () => this.GetCollection<string, EntityViewModel<string, TContext>>(Property.Name, x => new MVVM.EntityViewModel<string, TContext>(x));
            var Method = ((MethodCallExpression)Ex.Body).Method.GetGenericMethodDefinition().MakeGenericMethod(TChild, typeof(TChildViewModel));
            return (ICollection<TChildViewModel>)Method.Invoke(this, new object[] { PropertyName, Factory });
        }
        /// <summary>
        /// Gets a new entity collection from a navigation property
        /// </summary>
        /// <typeparam name="TChild">The element type of the child collection</typeparam>
        /// <param name="Property">The navigation property</param>
        /// <returns></returns>
        private ICollection<TChildViewModel> GetCollection<TChild, TChildViewModel>(string PropertyName, Func<Func<TContext>, TChildViewModel> Factory)
            where TChild : class
            where TChildViewModel : IModelViewModel<TChild>, IEntityOperator
        {
            var Col = collections[PropertyName];
            if (Col.Value != null) return (ICollection<TChildViewModel>)Col.Value;

            var Property = Col.Property;

            using (var c = context())
            {
                //Gets the collection from the database:
                var AtEntity = c.Set<TEntity>().GetEntity(Model);
                var Collection = (IEnumerable<TChild>)Property.GetValue(AtEntity);

                //Create the entity collection
                var ret = new EntityCollection<TChildViewModel, TChild>(Collection, () => Factory(context));
                ret.InformEntityOperation += (_, e) =>
                {
                    operations.Enqueue(e.Op);
                };

                Col.Value = ret;
            }

            return (ICollection<TChildViewModel>)Col.Value;
        }



        public void Commit()
        {

        }

        /// <summary>
        /// Save this view model changes to the database
        /// </summary>
        /// <param name="Context"></param>
        public void Persist(DbContext Context)
        {
            //Get this entity:
            var Entity = Context.Set<TEntity>().GetEntity(Model);

            //Update all entity properties:
            foreach (var P in columnProperties) P.SetValue(Entity, P.GetValue(Model));

            //Persist all getted collections:
            foreach (var C in collections.Where(x => x.Value.Value != null && x.Value.Value is ICollectionPersist))
            {
                //Collection persist:
                var CP = (ICollectionPersist)C.Value.Value;

                //Model collection:
                var MC = (IEnumerable)C.Value.Property.GetValue(Entity);

                CP.Persist(Context, MC);
            }
        }
    }
}
