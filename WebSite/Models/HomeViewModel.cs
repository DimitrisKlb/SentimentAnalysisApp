using System.Collections.Generic;

namespace WebSite.Models {

    public enum BannnerMsgCode: int {
        None,
        CreateOk,
        ErrorNotCreated,
        ErrorNotExecuted
    };

    public class HomeViewModel {
        public IEnumerable<FESearchRequest> TheSearchRequests { get; set; }
        public BannnerMsgCode TheBannerCode { get; set; }

        public static string[] BannerMsgText = {
            "",
            "Your Search Request was successfully created",
            "Unfortunately, your Search Request could not be created. Please try again later.",
            "Unfortunately, your Search Request could not be be programmed for execution right now, and was saved to your Drafts. Please try again later."
        };

        public static string[] AlertType = {
            "",
            "alert-success",
            "alert-danger",
            "alert-danger"
        };

        public string showBannerMsgText() {
            return BannerMsgText[(int)(this.TheBannerCode)];
        }

        public string showBannerMsgAlert() {
            return AlertType[(int)(this.TheBannerCode)];
        }
    }
}