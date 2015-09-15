using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tonic.EF;
using Tonic.Patterns.Collections;

namespace Tonic.MVVM
{
    /// <summary>
    /// Provides deferred inserts and deletes with validation checked delete for entity collections
    /// </summary>
    /// <typeparam name="TViewModel">View model type</typeparam>
    /// <typeparam name="TModel">Entity type</typeparam>
    public class EntityCollection<TViewModel, TModel> : PreviewCollection<TViewModel>, IEntityOperator, ICollectionPersist
        where TViewModel : IModelViewModel<TModel>, IEntityOperator
        where TModel : class
    {
        Func<TViewModel> factory;

        /// <summary>
        /// Guarda las operaciones pendientes, que seran informadas al padre en el momento del Commit
        /// </summary>
        private Queue<EntityOperation> operations = new Queue<EntityOperation>();


        /// <summary>
        /// Create a new collection that maps entities to entity view models
        /// </summary>
        /// <param name="Items">A delegate that gets the list of items given a context</param>
        /// <param name="Remove">A delegate that removes an item from the collection returned from the Items delegate</param>
        /// <param name="Add">A delegate that removes an item from the collection returned from the Items delegate</param>
        /// <param name="Factory">Entity to view model mapper</param>
        /// <param name="ImmediateMode">True to commit changes on every collection modification, false to commit until the commit method is called</param>
        public EntityCollection(
            IEnumerable<TModel> Items,
            Func<TViewModel> Factory)
            : base(Items.Select(x => { var vm = Factory(); vm.Model = x; return vm; }).ToList())
        {
            Action<CrudOperation, TModel> AddOp = (a, b) =>
            {
                var O = new EntityOperation(a, b);
                this.operations.Enqueue(O);
            };

            //Se subscribe a los eventos de todas las entidades:
            foreach (var ViewModel in this)
                ViewModel.InformEntityOperation += ViewModel_InformEntityOperation;

            this.factory = Factory;

            this.BeforeSet += (x, y) => { throw new NotSupportedException(); };

            //this.AfterClear += (_, e) => { foreach (var x in this) log.Add(new Op(Op.OpType.Delete, x)); };

            this.AfterRemove += (_, e) =>
            {
                AddOp(CrudOperation.Delete, e.Item.Model);
                e.Item.InformEntityOperation -= ViewModel_InformEntityOperation;
            };

            this.AfterInsert += (_, e) =>
            {
                AddOp(CrudOperation.Insert, e.Item.Model);
                e.Item.InformEntityOperation += ViewModel_InformEntityOperation;
            };
        }

        private void ViewModel_InformEntityOperation(object sender, EntityOperationArgs e)
        {
            InformEntityOperation?.Invoke(this, e);
        }

        public event EventHandler<EntityOperationArgs> InformEntityOperation;

        /// <summary>
        /// Mark all pending operations as commited
        /// </summary>
        public void Commit()
        {
            while (operations.Count > 0)
            {
                operations.Dequeue().Commit = true;
            };
        }

        void IEntityOperator.Persist(DbContext Context)
        {
            throw new NotImplementedException();
        }

        void ICollectionPersist.Persist(DbContext Context, IEnumerable ModelCollection)
        {
            foreach (var E in this) E.Persist(Context);

            while (operations.Count > 0)
            {
                var Op = operations.Dequeue();

                Op.Apply<TModel>(Context, ModelCollection);
            }
        }
    }
}
