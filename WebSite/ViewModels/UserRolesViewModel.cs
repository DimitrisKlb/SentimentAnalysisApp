using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace WebSite.ViewModels {

    public class UR_BannerMsg: BannerMsg {
        public static readonly UR_BannerMsg RoleCreated = new UR_BannerMsg( "User Role was successfully created.", "alert-success" );
        public static readonly UR_BannerMsg RoleDeleted = new UR_BannerMsg( "User Role was successfully deleted.", "alert-success" );
        public static readonly UR_BannerMsg RoleEdited = new UR_BannerMsg( "User Role was successfully edited.", "alert-success" );
        public static readonly UR_BannerMsg RoleNotEdited = new UR_BannerMsg( "User Role could not be edited.", "alert-danger" );
        public static readonly UR_BannerMsg RoleAddedUser = new UR_BannerMsg( "User Role was successfully added to the User.", "alert-success" );
        public static readonly UR_BannerMsg RoleNotAddedUser = new UR_BannerMsg( "User Role could not be added to the User.", "alert-danger" );
        public static readonly UR_BannerMsg RoleRemovedUser = new UR_BannerMsg( "User Role was successfully removed from the User.", "alert-success" );
        public static readonly UR_BannerMsg RoleNotRemovedUser = new UR_BannerMsg( "User Role could not be removed from the User.", "alert-danger" );

        private UR_BannerMsg(string Text, string AlertType)
            : base( Text, AlertType ) {
        }
    }

    public class UserRoleInfo {

        [Required]
        [StringLength( 15, MinimumLength = 5 )]
        [Display( Name = "Role Name" )]
        public string roleName { get; set; }
        public string roleID { get; set; }
    }

    public class UserRolesViewModel {
        public IEnumerable<SelectListItem> TheRolesList { get; set; }
        public IEnumerable<SelectListItem> TheUsersList { get; set; }
        public UR_BannerMsg TheBannerMsg { get; set; }

        public IEnumerable<string> RolesForTheUser { get; set; }
        public string TheUserID { get; set; }
    }

}