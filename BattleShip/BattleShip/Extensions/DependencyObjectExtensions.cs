namespace BattleShip.Extensions {
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;

    // DependencyObject magic..
    public static class DependencyObjectExtensions {

        // gets the first child of the specified type within the parent's visualtree
        public static T FirstOrDefaultChild<T>(this DependencyObject parent, Func<T, bool> selector) where T : DependencyObject {
            return FirstOrDefaultVisualChildWhere(parent, selector, out var foundChild) ? foundChild : default(T);
        }

        // gets a multiple of visualchildren of the same type in the parent's visualtree
        public static List<T> VisualChildrenOrDefault<T>(this DependencyObject parent, List<T> childrenToAdd, Func<T, bool> selector) where T : DependencyObject {
            return VisualChildrenOrDefaultWhere(parent, childrenToAdd, selector, out var foundChildren) ? foundChildren : new List<T>();
        }

        private static bool FirstOrDefaultVisualChildWhere<T>(DependencyObject parent, Func<T, bool> selector, out T foundChild) where T : DependencyObject {
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < count; i++) {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T tChild) {
                    if (!selector(tChild)) continue;
                    foundChild = tChild;
                    return true;
                }

                if (FirstOrDefaultVisualChildWhere(child, selector, out foundChild)) {
                    return true;
                }
            }

            foundChild = default(T);
            return false;
        }

        private static bool VisualChildrenOrDefaultWhere<T>(DependencyObject parent, List<T> childrenToAdd, Func<T, bool> selector, out List<T> foundChildren) where T : DependencyObject {
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < count; i++) {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T tChild) {
                    if (!selector(tChild)) continue;
                    childrenToAdd.Add(tChild);
                    foundChildren = childrenToAdd;
                }

                if (VisualChildrenOrDefaultWhere(child, childrenToAdd, selector, out foundChildren)) {
                }
            }

            if (childrenToAdd.Count != 0) {
                foundChildren = childrenToAdd;
                return true;
            }
            foundChildren = new List<T>();
            return false;
        }

        // gets the parent of an element that has the specified type
        public static T GetParent<T>(DependencyObject child) where T : DependencyObject {
            var dependencyObject = VisualTreeHelper.GetParent(child);

            if (dependencyObject != null) {
                if (dependencyObject is T parent) {
                    return parent;
                }

                return GetParent<T>(dependencyObject);
            }

            return null;
        }
    }
}
