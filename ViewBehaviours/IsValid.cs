using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Tonic.UI
{
    /// <summary>
    /// Bind UI validation HasError of any child control to a view model property
    /// </summary>
    public static class IsValid
    {
        public static readonly DependencyProperty BindingProperty =
         DependencyProperty.RegisterAttached(
             "Binding",
             typeof(bool?),
             typeof(IsValid),
             new PropertyMetadata(BindingChanged));


        //Objetos que ya estan siendo manejados por la propiedad Binding
        private static HashSet<DependencyObject> doneObjects = new HashSet<DependencyObject>();


        private static void BindingChanged(
       DependencyObject d,
       DependencyPropertyChangedEventArgs e)
        {
            if (doneObjects.Contains(d))
                return;

            doneObjects.Add(d);

            var Binding = BindingOperations.GetBinding(d, BindingProperty);
            if (Binding == null)
                throw new ArgumentException("Only binding expressions are supported");

            var expression = BindingOperations.GetBindingExpression(d, BindingProperty);
            var source = expression.ResolvedSource;
            var path = Binding.Path.Path;

            Validation.AddErrorHandler(d, (sender, o) =>
           {
               PropertyPathHelper.SetValue(source, path, IsDpValid(d));
           });

            PropertyPathHelper.SetValue(source, path, IsDpValid(d));
        }


        public static void SetBinding(DependencyObject target, bool? value)
        {
            target.SetValue(BindingProperty, value);
        }

        private static bool IsDpValid(DependencyObject obj)
        {
            // The dependency object is valid if it has no errors and all
            // of its children (that are dependency objects) are error-free.
            return !Validation.GetHasError(obj) &&
            LogicalTreeHelper.GetChildren(obj)
            .OfType<DependencyObject>()
            .All(IsDpValid);
        }



    }
}
