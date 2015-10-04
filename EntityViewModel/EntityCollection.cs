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
    /// Generic delegate called for every view model on a persist operation
    /// </summary>
    /// <typeparam name="TViewModel">View model type</typeparam>
    /// <param name="ViewModel">The view model that contains the unnatached model</param>
    /// <param name="ModelAttached">The model attached to the current persist context</param>
    public delegate void PersistDelegate<TViewModel>(TViewModel ViewModel, object ModelAttached);

    /// <summary>
    /// Delegate called for every view model on a persist operation
    /// </summary>
    /// <param name="ViewModel">The view model that contains the unnatached model</param>
    /// <param name="ModelAttached">The model attached to the current persist context</param>
    public delegate void PersistDelegate(object ViewModel, object ModelAttached);

    /// <summary>
    /// Entity collection factory
    /// </summary>
    public static class EntityCollection
    {
        /// <summary>
        /// Create a new entity collection given the original model items and a view model factory
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="ModelItems">Items source used to fill the collection</param>
        /// <param name="Factory">View model </param>
        /// <param name="Persist">Persist delegate that will be called for each loaded view model on the persist operation</param>
        /// <returns></returns>
        public static EntityCollection<TViewModel> Create<TViewModel, TModel>(IEnumerable<TModel> ModelItems, Func<TViewModel> Factory, PersistDelegate<TViewModel> Persist)
            where TViewModel : IModelViewModel<TModel>
            where TModel : class
        {
            return new EntityCollection<TViewModel>(ModelItems, Factory, Persist);
        }

        /// <summary>
        /// Create a new entity collection given the original model items and a view model factory without any per-entity persist logic
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="ModelItems"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static EntityCollection<TViewModel> Create<TViewModel, TModel>(IEnumerable<TModel> ModelItems, Func<TViewModel> Factory)
            where TViewModel : IModelViewModel<TModel>
            where TModel : class
        {
            return new EntityCollection<TViewModel>(ModelItems, Factory, null);
        }

        /// <summary>
        /// Create a new entity collection given the original model items and a view model factory without any per-entity persist logic
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="ViewModelItems"></param>
        /// <returns></returns>
        public static EntityCollection<TViewModel> Create<TViewModel>(IEnumerable<TViewModel> ViewModelItems)
            where TViewModel : IModelViewModel
        {
            return new EntityCollection<TViewModel>(ViewModelItems, null);
        }

        /// <summary>
        /// Create a new entity collection given the original model items and a view model factory without any per-entity persist logic
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="ViewModelItems"></param>
        /// <param name="Persist">Persist delegate that will be called for each loaded view model on the persist operation</param>
        /// <returns></returns>
        public static EntityCollection<TViewModel> Create<TViewModel>(IEnumerable<TViewModel> ViewModelItems, PersistDelegate<TViewModel> Persist)
            where TViewModel : IModelViewModel
        {
            return new EntityCollection<TViewModel>(ViewModelItems, Persist);
        }
    }
    /// <summary>
    /// Provides deferred inserts and d eletes with validation checked delete for entity collections, with support for short-lived contexts
    /// </summary>
    /// <typeparam name="TViewModel">View model type</typeparam>
    public class EntityCollection<TViewModel> : PreviewCollection<TViewModel>
        where TViewModel : IModelViewModel
    {
        PersistDelegate<TViewModel> persist;


        /// <summary>
        /// Guarda las operaciones pendientes, que seran informadas al padre en el momento del Commit
        /// </summary>
        private Queue<EntityOperation> operations = new Queue<EntityOperation>();


        /// <summary>
        /// Create a new collection that maps entities to entity view models
        /// </summary>
        /// <param name="Items">The original collection of models</param>
        /// <param name="ViewModelProvider">Service provider for view model construction</param>
        /// <param name="Persist">Action that will be called once for every loaded entity when the user calls the Persist method. Can be null</param>
        public EntityCollection(
            IEnumerable Items,
            Func<Type, object> ViewModelProvider,
            PersistDelegate Persist)
            : this(Items, () => (TViewModel)ViewModelProvider(typeof(TViewModel)), (V, M) => Persist(V, M))
        {

        }

        /// <summary>
        /// Create a new collection that maps entities to entity view models
        /// </summary>
        /// <param name="ViewModelItems">The original collection of models</param>
        /// <param name="Persist">Action that will be called once for every loaded entity when the user calls the Persist method. Can be null</param>
        public EntityCollection(
            IEnumerable<TViewModel> ViewModelItems,
            PersistDelegate<TViewModel> Persist)
            : base(ViewModelItems)
        {

            Action<CrudOperation, object> AddOp = (a, b) =>
            {
                var O = new EntityOperation(a, b);
                this.operations.Enqueue(O);
            };

            this.persist = Persist;

            this.BeforeSet += (x, y) => { throw new NotSupportedException(); };

            this.AfterRemove += (_, e) =>
            {
                AddOp(CrudOperation.Delete, e.Item.Model);
            };

            this.AfterInsert += (_, e) =>
            {
                AddOp(CrudOperation.Insert, e.Item.Model);
            };
        }

        /// <summary>
        /// Create a new collection that maps entities to entity view models
        /// </summary>
        /// <param name="Items">The original collection of models</param>
        /// <param name="Factory">View model factory</param>
        /// <param name="Persist">Action that will be called once for every loaded entity when the user calls the Persist method. Can be null</param>
        public EntityCollection(
            IEnumerable Items,
            Func<TViewModel> Factory,
            PersistDelegate<TViewModel> Persist)
            : this(Items.Cast<object>().Select(x => { var vm = Factory(); vm.Model = x; return vm; }).ToList(), Persist)
        { }

        /// <summary>
        /// Apply all history of pending changes onto the given model collection
        /// </summary>
        /// <param name="DestModelCollection">The collection to modify</param>
        public void Persist(IEnumerable DestModelCollection)
        {
            Persist(DestModelCollection, o => o);
        }

        /// <summary>
        /// Calls the Persist action for every loaded entity and apply all history of pending changes onto the given model collection
        /// </summary>
        /// <param name="GetAttached">Gets the input entity attached to the ModelCollection context</param>
        /// <param name="DestModelCollection">The collection to modify</param>
        public void Persist(IEnumerable DestModelCollection, Func<object, object> GetAttached)
        {
            if (persist != null)
            {
                foreach (var E in this)
                    persist(E, GetAttached(E.Model));
            }

            while (operations.Count > 0)
            {
                var Op = operations.Dequeue();

                Op.Apply(GetAttached, DestModelCollection);
            }
        }

        /// <summary>
        /// Calls the Persist action for every loaded entity and apply all history of pending changes onto the given model collection
        /// </summary>
        /// <param name="GetAttached">Gets the input entity attached to the ModelCollection context</param>
        /// <param name="ModelCollection">The collection to modify</param>
        public void Persist<TModel>(IEnumerable<TModel> ModelCollection, Func<TModel, TModel> GetAttached)
            where TModel : class
        {
            Persist(ModelCollection, (object o) => GetAttached((TModel)o));
        }
    }
}
