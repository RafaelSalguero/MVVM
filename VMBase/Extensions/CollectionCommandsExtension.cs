using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
namespace Tonic.MVVM.Extensions
{
    /// <summary>
    /// Constains factory methods for creating CollectionCommands extensions
    /// </summary>
    public static class CollectionCommandsExtension
    {
        /// <summary>
        /// Create a new CollectionCommands extension that exposes Add, Delete and Update commands that bind to the given collection
        /// </summary>
        /// <typeparam name="TViewModel">Instance view model type</typeparam>
        /// <typeparam name="TElement">Collection element type</typeparam>
        /// <param name="Instance">View model instance</param>
        /// <param name="Collection">Collection property</param>
        /// <param name="ShowEditor">Shows the UI editor for the given collection element</param>
        /// <param name="AskDelete">Show the UI message that ask to delete the selected item</param>
        /// <returns></returns>
        public static CollectionCommandsExtension<TElement> Create<TViewModel, TElement>(TViewModel Instance,
            Expression<Func<TViewModel, ICollection<TElement>>> Collection,
            Action<TElement> ShowEditor,
            Func<object, bool> AskDelete)
           where TElement : ICommitViewModel, IModelViewModel, new()
        {
            return Create(Instance, Collection, null, ShowEditor, AskDelete, () => new TElement());
        }

        /// <summary>
        /// Create a new CollectionCommands extension that exposes Add, Delete and Update commands that bind to the given collection
        /// </summary>
        /// <typeparam name="TViewModel">Instance view model type</typeparam>
        /// <typeparam name="TElement">Collection element type</typeparam>
        /// <param name="Instance">View model instance</param>
        /// <param name="Collection">Collection property</param>
        /// <param name="SelectedItem">Selected item property. If null, the extension will expose a SelectedItem property</param>
        /// <param name="ShowEditor">Shows the UI editor for the given collection element</param>
        /// <param name="AskDelete">Show the UI message that ask to delete the selected item</param>
        /// <returns></returns>
        public static CollectionCommandsExtension<TElement> Create<TViewModel, TElement>(TViewModel Instance,
            Expression<Func<TViewModel, ICollection<TElement>>> Collection,
            Expression<Func<TViewModel, TElement>> SelectedItem,
            Action<TElement> ShowEditor,
            Func<object, bool> AskDelete)
           where TElement : ICommitViewModel, IModelViewModel, new()
        {
            return Create(Instance, Collection, SelectedItem, ShowEditor, AskDelete, () => new TElement());
        }

        /// <summary>
        /// Create a new CollectionCommands extension that exposes Add, Delete and Update commands that bind to the given collection
        /// </summary>
        /// <typeparam name="TViewModel">Instance view model type</typeparam>
        /// <typeparam name="TElement">Collection element type</typeparam>
        /// <param name="Instance">View model instance</param>
        /// <param name="Collection">Collection property</param>
        /// <param name="ShowEditor">Shows the UI editor for the given collection element</param>
        /// <param name="ElementFactory">Create a new collection element</param>
        /// <param name="AskDelete">Show the UI message that ask to delete the selected item</param>
        /// <returns></returns>
        public static CollectionCommandsExtension<TElement> Create<TViewModel, TElement>(TViewModel Instance,
            Expression<Func<TViewModel, ICollection<TElement>>> Collection,
            Action<TElement> ShowEditor,
            Func<object, bool> AskDelete,
            Func<TElement> ElementFactory)
           where TElement : ICommitViewModel, IModelViewModel
        {
            return Create(Instance, Collection, null, ShowEditor, AskDelete, ElementFactory);
        }


        /// <summary>
        /// Create a new CollectionCommands extension that exposes Add, Delete and Update commands that bind to the given collection
        /// </summary>
        /// <typeparam name="TViewModel">Instance view model type</typeparam>
        /// <typeparam name="TElement">Collection element type</typeparam>
        /// <param name="Instance">View model instance</param>
        /// <param name="Collection">Collection property</param>
        /// <param name="SelectedItem">Selected item property. If null, the extension will expose a SelectedItem property</param>
        /// <param name="ShowEditor">Shows the UI editor for the given collection element</param>
        /// <param name="AskDelete">Show the UI message that ask to delete the selected item</param>
        /// <param name="ElementFactory">Create a new collection element</param>
        /// <returns></returns>
        public static CollectionCommandsExtension<TElement> Create<TViewModel, TElement>(TViewModel Instance,
            Expression<Func<TViewModel, ICollection<TElement>>> Collection,
            Expression<Func<TViewModel, TElement>> SelectedItem,
            Action<TElement> ShowEditor,
            Func<object, bool> AskDelete,
             Func<TElement> ElementFactory)
           where TElement : ICommitViewModel, IModelViewModel
        {
            var ModelViewModel = typeof(TElement).GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IModelViewModel<>)).FirstOrDefault();
            if (ModelViewModel == null)
                throw new ArgumentException($"View model type {typeof(TElement)} must implement IModelViewModel interface");

            var ModelProperty = ModelViewModel.GetProperty(nameof(IModelViewModel<object>.Model));

            var ModelType = ModelViewModel.GenericTypeArguments[0];
            if (ModelType.GetConstructor(Type.EmptyTypes) == null)
                throw new ArgumentException($"Model type {ModelType} must have a public parameterless constructor");

            var cProp = ExtensionHelper.GetPropertyInfo(Collection);
            var sProp = SelectedItem == null ? null : ExtensionHelper.GetPropertyInfo(SelectedItem);

            return new CollectionCommandsExtension<TElement>(
                Instance,
                cProp.Name,
                () => (ICollection<TElement>)cProp.GetValue(Instance),
                sProp?.Name,
                () => (TElement)sProp.GetValue(Instance),
                ElementFactory,
                () => Activator.CreateInstance(ModelType),
                (a, b) => ModelProperty.SetValue(a, b),
                (a) => ModelProperty.GetValue(a),
                (a, b) => a.OnCommit += b,
                ShowEditor,
                AskDelete,
                (a) => a.ModelType
                );
        }

        //Syntax test:
        static void Test()
        {
            ViewModel v = new ViewModel();
            Create(v, x => x.Entities, x => x.SelectedItem, x => { }, x => true, () => new ExposedViewModel<object>());
        }

        class ViewModel
        {
            public ICollection<ExposedViewModel<object>> Entities { get; set; }
            public ExposedViewModel<object> SelectedItem { get; set; }
        }
    }



    /// <summary>
    /// Expose Add, Remove and Update commands for a given view model collections. Optionally expose a SelectedItem property
    /// </summary>
    /// <remarks>The commands follow the convention: [PropertyName][Add|Edit|Remove]Command
    /// The selected item follow the convention: [PropertyName]SelectedItem
    ///  </remarks>
    /// <typeparam name="TViewModel">View model type</typeparam>
    public class CollectionCommandsExtension<TViewModel> : IDynamicExtension
    {
        const string addPostfix = "AddCommand";
        const string editPostfix = "EditCommand";
        const string removePostfix = "RemoveCommand";
        const string selectedPostfix = "SelectedItem";

        private readonly DelegateCommand Add, Remove, Edit;
        private string collectionPrefix;
        private readonly Func<TViewModel> selectedItemGetter;

        private bool ExposeSelectedItem
        {
            get
            {
                return selectedItemGetter == null;
            }
        }
        private readonly string exposeSelectedName;

        private TViewModel exposedSelectedItem;
        private TViewModel SelectedItem
        {
            get
            {
                if (ExposeSelectedItem)
                    return exposedSelectedItem;
                else
                    return selectedItemGetter();
            }
        }


        /// <summary>
        /// Create a new extension that exposes Add, Remove and Update commands
        /// </summary>
        /// <param name="Instance">View model instance</param>
        /// <param name="CollectionPropertyName">Collection property name, used as prefix for the collection commands</param>
        /// <param name="CollectionPropertyGetter">Gets the collection</param>
        /// <param name="SelectedPropertyName">Selected item property name. If this property is null, the selected property getter is ignored</param>
        /// <param name="SelectedPropertyGetter">Selected item property getter, used by the Delete and Update commands. If null, the extension will expose the SelectedItem property</param>
        /// <param name="ViewModelFactory">View model constructor</param>
        /// <param name="ModelFactory">Model constructor</param>
        /// <param name="SetModel">Sets the view model model</param>
        /// <param name="ShowEditor">Show the editor UI for the given view model</param>
        /// <param name="AskDelete">Show the UI message that ask to delete the selected item</param>
        /// <param name="AddOnCommitHandler">Add an event handler to the view model when the user invokes the commit action</param>
        public CollectionCommandsExtension(
            object Instance,
            string CollectionPropertyName,
            Func<ICollection<TViewModel>> CollectionPropertyGetter,
            string SelectedPropertyName,
            Func<TViewModel> SelectedPropertyGetter,
            Func<TViewModel> ViewModelFactory,
            Func<object> ModelFactory,
            Action<TViewModel, object> SetModel,
            Func<TViewModel, object> GetModel,
            Action<TViewModel, EventHandler> AddOnCommitHandler,
            Action<TViewModel> ShowEditor,
            Func<object, bool> AskDelete,
            Func<TViewModel, Type> GetModelType)
        {
            if (CollectionPropertyGetter == null)
                throw new ArgumentException($"{nameof(CollectionPropertyGetter)} is null");

            //Si el prorperty name es null, se ignore el property getter
            if (SelectedPropertyName == null)
                SelectedPropertyGetter = null;
            this.selectedItemGetter = SelectedPropertyGetter;

            this.collectionPrefix = CollectionPropertyName;
            this.selectedItemGetter = SelectedPropertyGetter;
            this.exposeSelectedName = CollectionPropertyName + selectedPostfix;

            //Add command:
            Action Add = () =>
            {
                var VM = ViewModelFactory();
                var Model = ModelFactory();
                SetModel(VM, Model);

                AddOnCommitHandler(VM, (sender, e) =>
               {
                   var Collection = CollectionPropertyGetter();
                   if (Collection == null)
                       throw new ArgumentException($"The collection property getter for the property {CollectionPropertyName} returns null");

                   Collection.Add(VM);
               });

                ShowEditor(VM);
            };

            Action Edit = () =>
            {
                var VM = ViewModelFactory();
                var OriginalModel = GetModel(SelectedItem);

                //Obtiene una copia del modelo original, y lo asigna al view model, esto para que los cambios
                //se vean reflejados solamente al dar OK

                //propiedades que se deberan de copiar de vuelva al OriginalModel
                IEnumerable<string> CopyProperties;
                var MementoModel = MementoFactory.Lazy(GetModelType(SelectedItem), OriginalModel, out CopyProperties);
                SetModel(VM, MementoModel);

                AddOnCommitHandler(VM, (sender, e) =>
               {
                   //Copia las propiedades de vuelta al modelo original, 
                   var MementoAc = FastMember.ObjectAccessor.Create(MementoModel);
                   var OriginalAc = FastMember.ObjectAccessor.Create(OriginalModel);

                   foreach (var P in CopyProperties)
                       OriginalAc[P] = MementoAc[P];

                   SetModel(SelectedItem, OriginalModel);
               });

                ShowEditor(VM);
            };

            Action Remove = () =>
           {
               if (AskDelete(SelectedItem))
               {
                   var Collection = CollectionPropertyGetter();
                   if (Collection == null)
                       throw new ArgumentException($"The collection property getter for the property {nameof(CollectionPropertyName)} returns null");

                   Collection.Remove(SelectedItem);
               }
           };

            this.Add = new DelegateCommand(Add);
            this.Edit = new DelegateCommand(Edit, () => SelectedItem != null);
            this.Remove = new DelegateCommand(Remove, () => SelectedItem != null);

            //Actualiza el can execute cuando el selected item ha sido modificado
            var Notifier = Instance as INotifyPropertyChanged;
            if (Notifier != null)
            {
                Notifier.PropertyChanged += (sender, e) =>
                {
                    string selectedName = ExposeSelectedItem ? exposeSelectedName : SelectedPropertyName;
                    if (e.PropertyName == selectedName)
                    {
                        this.Edit.RaiseCanExecuteChanged();
                        this.Remove.RaiseCanExecuteChanged();
                    }
                };
            }
        }


        IEnumerable<string> IDynamicExtension.MemberNames
        {
            get
            {
                if (ExposeSelectedItem)
                    yield return exposeSelectedName;
                yield return collectionPrefix + addPostfix;
                yield return collectionPrefix + editPostfix;
                yield return collectionPrefix + removePostfix;
            }
        }

        bool IDynamicExtension.CanRead(string PropertyName)
        {
            return true;
        }

        bool IDynamicExtension.CanWrite(string PropertyName)
        {
            return (PropertyName == exposeSelectedName && ExposeSelectedItem);
        }

        object IDynamicExtension.Get(string PropertyName)
        {
            if (PropertyName == exposeSelectedName && ExposeSelectedItem)
                return SelectedItem;
            else
            {
                if (PropertyName == (collectionPrefix + addPostfix))
                    return Add;
                else if (PropertyName == (collectionPrefix + editPostfix))
                    return Edit;
                else if (PropertyName == (collectionPrefix + removePostfix))
                    return Remove;
                else
                    throw new InvalidOperationException();
            }
        }

        void IDynamicExtension.Set(string PropertyName, object Value)
        {
            if (PropertyName == exposeSelectedName && ExposeSelectedItem)
                exposedSelectedItem = (TViewModel)Value;
            else
                throw new NotImplementedException();
        }

        Type IDynamicExtension.GetPropertyType(string PropertyName)
        {
            if (PropertyName == exposeSelectedName && ExposeSelectedItem)
                return typeof(TViewModel);
            else
            {
                if ((PropertyName == (collectionPrefix + addPostfix)) || (PropertyName == (collectionPrefix + editPostfix) || (PropertyName == (collectionPrefix + removePostfix)))
                    return typeof(DelegateCommand);
                else
                    throw new InvalidOperationException();
            }
        }
    }
}
