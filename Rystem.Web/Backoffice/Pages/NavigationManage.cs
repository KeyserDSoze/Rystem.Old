using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Web.Backoffice
{
    public interface INavigationManage
    {
        string Title { get; }
        string ConfirmDelete { get; }
        string BackToList { get; }
        string Delete { get; }
        string Id { get; }
        IStringLocalizer Localizer { get; }
        IEnumerable<(string Index, string Label, string Value, PropertyOptions Options, IEnumerable<object> Objects)> Values();
    }
    internal class NavigationManage<T> : NavigationPage<T>, INavigationManage
        where T : class
    {
        private readonly T Entity;
        internal NavigationManage(Navigation<T> navigation, T entity) : base(navigation)
            => this.Entity = entity;
        public string Title
            => this.Navigation.Options.Title;
        /// <summary>
        /// Localization: Are you sure you want to delete this?
        /// </summary>
        public string ConfirmDelete
            => this.Navigation.Options.GetLocalizedString("Are you sure you want to delete this?");
        /// <summary>
        /// Localization: Back To List
        /// </summary>
        public string BackToList
            => this.Navigation.Options.GetLocalizedString("Back To List");
        /// <summary>
        /// Localization: Delete
        /// </summary>
        public string Delete
            => this.Navigation.Options.GetLocalizedString("Delete");
        public IStringLocalizer Localizer
            => this.Navigation.Options.Localizer;
        public string Id { get; private set; }
        public IEnumerable<(string Index, string Label, string Value, PropertyOptions Options, IEnumerable<object> Objects)> Values()
        {
            IEnumerator<(string Value, string Localized)> enumerator = this.Navigation.GetHeaders().GetEnumerator();
            var navigationValue = this.Navigation.GetValues(this.Entity);
            if (navigationValue.Key != null)
                this.Id = navigationValue.Key;
            foreach (var element in navigationValue.Elements)
            {
                enumerator.MoveNext();
                yield return (enumerator.Current.Value, enumerator.Current.Localized, element.Value, element.Options, element.BaseObjects);
            }
        }
    }
}