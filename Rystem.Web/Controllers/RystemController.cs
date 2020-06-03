using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Web.Backoffice
{
    public abstract class RystemController<T> : Controller
        where T : class
    {
        public IActionResult Index()
          => View("_Index", this.GetNavigationIndex());
        public abstract NavigationIndex<T> GetNavigationIndex();
        protected IActionResult NotOkMessage(ModelStateDictionary modelState)
            => NotOkMessage(string.Join(",", modelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)));
        protected IActionResult NotOkMessage(string message)
            => Conflict(message);
    }
}
