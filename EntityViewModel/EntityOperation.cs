using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.EF
{
    public enum CrudOperation { Insert, Delete, Update }


    /// <summary>
    /// Holds an entity operation in memory
    /// </summary>
    public class EntityOperation
    {
        public EntityOperation(CrudOperation Type, object Item) { this.Type = Type; this.Item = Item; }
        public CrudOperation Type { get; private set; }
        public object Item { get; private set; }

        /// <summary>
        /// If false, the operation must be ignored
        /// </summary>
        public bool Commit
        {
            get; set;
        }

        public void Apply<T>(DbContext Context, IEnumerable Items)
            where T : class
        {
            var Entity = Context.Set<T>().GetEntity((T)Item);
            Apply((dynamic)Items, Type, Entity);
        }

        private static void Apply<T>(ICollection<T> Items, CrudOperation Type, object Item)
            where T : class
        {
            switch (Type)
            {
                case CrudOperation.Delete:
                    Items.Remove((T)Item);
                    break;
                case CrudOperation.Insert:
                    Items.Add((T)Item);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private static void Apply<T>(DbSet<T> Items, CrudOperation Type, object Item)
            where T : class
        {
            switch (Type)
            {
                case CrudOperation.Delete:
                    Items.Remove((T)Item);
                    break;
                case CrudOperation.Insert:
                    Items.Add((T)Item);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class EntityOperationArgs : EventArgs
    {
        public EntityOperationArgs(CrudOperation Operation, object Item) : this(new EntityOperation(Operation, Item))
        {
        }
        public EntityOperationArgs(EntityOperation Op)
        {
            this.Op = Op;
        }

        public EntityOperation Op { get; private set; }
    }

    public interface IEntityOperator
    {
        event EventHandler<EntityOperationArgs> InformEntityOperation;
        void Persist(DbContext Context);
    }

    public interface IEntityPersist
    {
        void Persist(DbContext Context);
    }

    public interface ICollectionPersist
    {
        void Persist(DbContext Context, IEnumerable Collection);
    }
}
