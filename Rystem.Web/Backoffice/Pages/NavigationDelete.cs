using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Web.Backoffice
{
    public interface INavigationDelete
    {
        string Title { get; }
        string ConfirmDelete { get; }
        string BackToList { get; }
        string Delete { get; }
        string Id { get; }
        IEnumerable<(string Label, string Value)> Values();
    }
    public class NavigationDelete<T> : NavigationPage<T>, INavigationDelete
        where T : class
    {
        internal NavigationDelete(Navigation<T> navigation) : base(navigation)
        {
        }
        public string Title => this.Navigation.Options.Title;
        /// <summary>
        /// Localization: Are you sure you want to delete this?
        /// </summary>
        public string ConfirmDelete => this.Navigation.Options.GetLocalizedString("Are you sure you want to delete this?");
        /// <summary>
        /// Localization: Back To List
        /// </summary>
        public string BackToList => this.Navigation.Options.GetLocalizedString("Back To List");
        /// <summary>
        /// Localization: Delete
        /// </summary>
        public string Delete => this.Navigation.Options.GetLocalizedString("Delete");
        public string Id { get; private set; }
        public IEnumerable<(string Label, string Value)> Values()
        {
            IEnumerator<string> enumerator = this.Navigation.GetHeaders().GetEnumerator();
            foreach (var entity in this.Navigation.Values)
            {
                var navigationValue = this.Navigation.GetValues(entity);
                if (navigationValue.Key != null)
                    this.Id = navigationValue.Key;
                foreach (var value in navigationValue.Values)
                {
                    enumerator.MoveNext();
                    yield return (enumerator.Current, value);
                }
            }
        }

    }
}
