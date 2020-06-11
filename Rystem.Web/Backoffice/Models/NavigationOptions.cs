using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Web.Backoffice
{
    public sealed class NavigationOptions
    {
        public IStringLocalizer Localizer { get; set; }
        public bool IsLocalized => Localizer != null;
        private string title;
        public string Title
        {
            get => title != null ? GetLocalizedString(title) : null;
            set => title = value;
        }
        public bool HasTitle
            => this.Title != null;
        public string GetLocalizedString(string value)
        {
            if (this.IsLocalized)
                return this.Localizer[value];
            else
                return value;
        }
        public bool CanCreate { get; set; }
        public bool CanModify { get; set; }
        public bool CanDelete { get; set; }
        public string EditAction { get; set; }
        public static NavigationOptions CanDoneAll(string title = null, IStringLocalizer localizer = null)
            => new NavigationOptions()
            {
                CanCreate = true,
                CanDelete = true,
                CanModify = true,
                Localizer = localizer,
                Title = title
            };
        public static NavigationOptions CanDone(bool canCreate, bool canModify, bool canDelete, string title = null, IStringLocalizer localizer = null)
            => new NavigationOptions()
            {
                CanCreate = canCreate,
                CanDelete = canDelete,
                CanModify = canModify,
                Localizer = localizer,
                Title = title
            };
    }
}