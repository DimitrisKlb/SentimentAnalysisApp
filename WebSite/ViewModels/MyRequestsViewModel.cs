using System.Collections.Generic;

using WebSite.Models;

namespace WebSite.ViewModels {

    public class MR_BannerMsg: BannerMsg {
        public static readonly MR_BannerMsg CreateOk = new MR_BannerMsg( "Your Search Request was successfully created", "alert-sucess" );
        public static readonly MR_BannerMsg ErrorNotCreated = new MR_BannerMsg( "Unfortunately, your Search Request could not be created. Please try again later.", "alert-danger" );
        public static readonly MR_BannerMsg ErrorNotExecuted = new MR_BannerMsg( "Unfortunately, your Search Request could not be be programmed for execution right now, and was saved to your Drafts. Please try again later.", "alert-danger" );

        private MR_BannerMsg(string Text, string AlertType)
            : base( Text, AlertType ) {
        }
    }

    public class MyRequestsViewModel {
        public IEnumerable<FESearchRequest> TheSearchRequests { get; set; }        
        public MR_BannerMsg TheBannerMsg { get; set; }        
    }
}