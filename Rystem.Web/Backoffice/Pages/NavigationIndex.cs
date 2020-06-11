using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Web.Backoffice
{
    public interface INavigationIndex
    {
        string Title { get; }
        string Create { get; }
        string Edit { get; }
        string Delete { get; }
        bool CanCreate { get; }
        bool CanModify { get; }
        bool CanDelete { get; }
        string EditAction { get; }
        IEnumerable<string> Headers();
        IEnumerable<NavigationValue> Values();
    }
    internal class NavigationIndex<T> : NavigationPage<T>, INavigationIndex
        where T : class
    {
        private readonly IEnumerable<T> _values;
        internal NavigationIndex(Navigation<T> navigation, IEnumerable<T> values) : base(navigation)
            => this._values = values;
        public string Title => this.Navigation.Options.Title;
        public string Create => this.Navigation.Options.GetLocalizedString("Create New");
        public string Edit => this.Navigation.Options.GetLocalizedString("Edit");
        public string Delete => this.Navigation.Options.GetLocalizedString("Delete");
        public bool CanCreate => this.Navigation.Options.CanCreate;
        public bool CanModify => this.Navigation.Options.CanModify;
        public bool CanDelete => this.Navigation.Options.CanDelete;
        private const string Manage = "Manage";
        public string EditAction => this.Navigation.Options.EditAction ?? Manage;
        public IEnumerable<string> Headers()
            => this.Navigation.GetHeaders().Select(x => x.Localized);
        public IEnumerable<NavigationValue> Values()
        {
            foreach (var entity in this._values)
                yield return this.Navigation.GetValues(entity);
        }
    }
}