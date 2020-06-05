using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Web.Backoffice
{
    public abstract class BackOfficeController<T> : Controller
        where T : class
    {
        public abstract INavigation<T> GetNavigation();
        public abstract Task<IEnumerable<T>> GetList();
        public abstract Task<T> GetModel(string id);
        public abstract Task<bool> Remove(string id);
        public async Task<IActionResult> Index()
          => View("_Index", this.GetNavigation().ToIndex(await this.GetList()));
        public async Task<IActionResult> Delete(string id)
          => View("_Delete", this.GetNavigation().ToDelete(await this.GetModel(id)));
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await this.Remove(id);
            return RedirectToAction(nameof(Index));
        }
        protected IActionResult NotOkMessage(ModelStateDictionary modelState)
            => NotOkMessage(string.Join(",", modelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)));
        protected IActionResult NotOkMessage(string message)
            => Conflict(message);
    }
}
