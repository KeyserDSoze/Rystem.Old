using System.Collections.Generic;

namespace Rystem.Web.Backoffice
{
    internal static class NavigationExtensions
    {
        public static NavigationIndex<T> ToIndex<T>(this INavigation<T> navigation, IEnumerable<T> values)
            where T : class
            => new NavigationIndex<T>(navigation.Navigation, values);
        public static NavigationDelete<T> ToDelete<T>(this INavigation<T> navigation, T value)
            where T : class
            => new NavigationDelete<T>(navigation.Navigation, value);
        public static NavigationManage<T> ToManage<T>(this INavigation<T> navigation, T value)
            where T : class
            => new NavigationManage<T>(navigation.Navigation, value);
    }
}