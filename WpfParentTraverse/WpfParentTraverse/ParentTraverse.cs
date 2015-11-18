using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Tonic.UI
{
    /// <summary>
    /// Search data context traversing the visual and logical tree
    /// </summary>
    public static class ParentTraverse
    {
        /// <summary>
        /// Search a dependency object of the given type. If not found returns null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Element"></param>
        /// <returns>Return the first apparence of a dependency object of type T or null if not found</returns>
        public static T TrySearchDependencyObject<T>(DependencyObject Element)
            where T : class
        {
            var Ret = TrySearch(Element, x => x is T);
            return Ret as T;
        }

        /// <summary>
        /// Search a dependency object of the given type. If not found throws an exception
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Element"></param>
        /// <returns>Return the first apparence of a dependency object of type T</returns>
        public static T SearchDependencyObject<T>(DependencyObject Element)
            where T : DependencyObject
        {
            var Ret = Search(Element, x => x is T);
            return (T)Ret;
        }

        /// <summary>
        /// Search data context traversing the visual and logical tree
        /// </summary>
        public static object TrySearchDataContext(DependencyObject Element)
        {
            return TrySearchDataContext<object>(Element);
        }

        /// <summary>
        /// Search data context traversing the visual and logical tree
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Element"></param>
        /// <returns></returns>
        public static T SearchDataContext<T>(DependencyObject Element)
            where T : class
        {
            var Ret = Search(Element, (x) =>
                {
                    var current = x as FrameworkElement;
                    if (current != null)
                    {
                        var dataContext = current.DataContext;
                        var AsT = dataContext as T;
                        return AsT != null;
                    }
                    else
                        return false;
                });

            return (T)((FrameworkElement)Ret).DataContext;

        }


        /// <summary>
        /// Search data context traversing the visual and logical tree
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Element"></param>
        /// <returns></returns>
        public static T TrySearchDataContext<T>(DependencyObject Element)
            where T : class
        {
            var Ret = TrySearch(Element, (x) =>
            {
                var current = x as FrameworkElement;
                if (current != null)
                {
                    var dataContext = current.DataContext;
                    var AsT = dataContext as T;
                    return AsT != null;
                }
                else
                    return false;
            });

            if (Ret == null) return null;
            return (T)((FrameworkElement)Ret).DataContext;

        }


        /// <summary>
        /// Search for a property with the given name and type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Element"></param>
        /// <param name="PropertyName"></param>
        /// <returns></returns>
        public static T SearchProperty<T>(DependencyObject Element, string PropertyName)
        {
            var Ret = Search(Element, (x) =>
            {
                var Prop = x.GetType().GetProperty(PropertyName);
                if (Prop == null) return false;
                if (!typeof(T).IsAssignableFrom(Prop.PropertyType)) return false;
                return true;
            });

            return (T)Ret.GetType().GetProperty(PropertyName).GetValue(Ret);
        }


        /// <summary>
        /// Search for a property with the given name and type. Returns default(T) if not found
        /// </summary>
        /// <returns></returns>
        public static T TrySearchProperty<T>(DependencyObject Element, string PropertyName)
        {
            var Ret = TrySearch(Element, (x) =>
            {
                var Prop = x.GetType().GetProperty(PropertyName);
                if (Prop == null) return false;
                if (!typeof(T).IsAssignableFrom(Prop.PropertyType)) return false;
                return true;
            });

            if (Ret == null)
                return default(T);
            else
                return (T)Ret.GetType().GetProperty(PropertyName).GetValue(Ret);
        }

        /// <summary>
        /// Search data context traversing the visual and logical tree. Throws an exception if no element is found
        /// </summary>
        /// <returns></returns>
        public static DependencyObject Search(DependencyObject Element, Func<DependencyObject, bool> Predicate)
        {
            var current = Element;
            if (Predicate(current))
            {
                return current;
            }

            var parent = VisualTreeHelper.GetParent(Element);
            if (parent == null && Element is FrameworkElement)
            {
                parent = ((FrameworkElement)Element).Parent;
            }
            if (parent != null)
                return Search(parent, Predicate);
            else
                throw new ArgumentException("The element was not found on the visual nor logical tree traversal");

        }

        /// <summary>
        /// Search a dependency object that pass the given predicate
        /// </summary>
        /// <returns></returns>
        public static DependencyObject TrySearch(DependencyObject Element, Func<DependencyObject, bool> Predicate)
        {
            var current = Element;
            if (Predicate(current))
            {
                return current;
            }

            var parent = VisualTreeHelper.GetParent(Element);
            if (parent == null && Element is FrameworkElement)
            {
                parent = ((FrameworkElement)Element).Parent;
            }
            if (parent != null)
                return TrySearch(parent, Predicate);
            else
                return null;
        }

        /// <summary>
        /// Search dependency object of type T that pass the given predicate
        /// </summary>
        /// <returns></returns>
        public static T TrySearch<T>(DependencyObject Element, Func<T, bool> Predicate) where T : DependencyObject
        {
            return (T)TrySearch(Element, x => x is T && Predicate((T)x));
        }


        /// <summary>
        /// Search dependency object of type T
        /// </summary>
        /// <returns></returns>
        public static T TrySearch<T>(DependencyObject Element) where T : DependencyObject
        {
            return (T)TrySearch(Element, x => x is T);
        }
    }
}
