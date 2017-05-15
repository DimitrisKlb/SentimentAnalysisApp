using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using SentimentAnalysisApp.SharedModels;
using WebSite.Models;

namespace WebSite.ViewModels {

    public class MR_BannerMsg: BannerMsg {
        public static readonly MR_BannerMsg CreateOk = new MR_BannerMsg( "Your Search Request was successfully created", "alert-success" );
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

    public class CreateSReqViewModel {
        public FESearchRequest TheSearchRequest { get; set; }

        [Display( Name = "Sources " )]
        [CheckboxListNotEmpty( ErrorMessage = "You must select at least one option." )]
        public List<SourceOption> SelectedSources { get; set; }

        public CreateSReqViewModel() {
            TheSearchRequest = new FESearchRequest();
            SelectedSources = new List<SourceOption>();
        }

        public CreateSReqViewModel(string[] previousOptions) : this() {
            foreach(var prevOption in previousOptions) {
                var option = (SourceOption)Enum.Parse( typeof( SourceOption ), prevOption );
                SelectedSources.Add( option );
            }
        }
    }

    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false )]
    sealed public class CheckboxListNotEmptyAttribute: ValidationAttribute {
        public override bool IsValid(object value) {
            var theList = (List<SourceOption>)value;
            return (theList.Count != 0) ? true : false;
        }
    }

}