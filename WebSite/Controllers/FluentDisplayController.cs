using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;

using WebSite.ViewModels;

namespace WebSite.Controllers {

    // Base MVC Controller that provides functionality for correctly passing data to display in the respective Views such us:
    // Dynamic Banner Messages concerning multiple cases
    // Form validation for multiple embedded creation forms (display using Partial Views)
    public abstract class FluentDisplayController: Controller {

        protected System.Web.Mvc.RedirectToRouteResult RedirectToIndex(BannerMsg bannerMsg = null) {
            if(!ModelState.IsValid) {
                SaveModelState();
            }

            if(bannerMsg != null) {
                SaveBannerMsg( bannerMsg );
            }

            return RedirectToAction( "Index" );
        }

        private void SaveModelState() {
            TempData["modelKeys"] = ModelState.Keys.ToList();
            
            TempData["modelValues"] = (from v in ModelState.Values
                                       select v.Value).ToList();

            var errors = (from v in ModelState.Values
                          select v.Errors.FirstOrDefault()).ToList();

            string[] errorMessages = new string[errors.Count];
            for(int i=0; i<errors.Count; i++) {
                if(errors.ElementAt(i) != null) {
                    errorMessages[i] = errors.ElementAt( i ).ErrorMessage;
                } else {
                    errorMessages[i] = null;
                }
            }
            TempData["modelErrors"] = errorMessages;                             
        }

        protected void LoadModelState() {
            var keys = (IEnumerable<string>)TempData["modelKeys"];
            var values = (IEnumerable<ValueProviderResult>)TempData["modelValues"];
            var errorMessages = (IEnumerable<string>)TempData["modelErrors"];

            for(int i = 0; i < keys.Count(); i++) {
                ModelState.Add( keys.ElementAt( i ), new ModelState {
                    Value = values.ElementAt( i )
                } );
                var errorMessage = errorMessages.ElementAt( i );
                if(errorMessage != null) {
                    ModelState.AddModelError( keys.ElementAt( i ), errorMessage );
                }
            }
        }

        protected bool ModelErrorHappened() {
            return TempData["modelKeys"] == null ? false : true;
        }

        private void SaveBannerMsg(BannerMsg bannerMsg) {
            TempData["bannerMsg"] = bannerMsg;
        }

        protected BannerMsg LoadBannerMsg() {
            return (BannerMsg)TempData["bannerMsg"];
        }

    }

}
