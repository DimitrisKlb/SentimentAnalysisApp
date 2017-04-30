using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System.Collections.Generic;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

using WebSite.Models;
using WebSite.ViewModels;

namespace WebSite.Controllers {

    public class UserRolesController: FluentDisplayController {
        private ApplicationDbContext userDb = new ApplicationDbContext();

        public ActionResult Index() {
            UserRolesViewModel theVM = new UserRolesViewModel();

            theVM.TheRolesList = from r in userDb.Roles
                                 select new SelectListItem {
                                     Value = r.Id.ToString(),
                                     Text = r.Name
                                 };

            theVM.TheUsersList = from u in userDb.Users
                                 select new SelectListItem {
                                     Value = u.Id.ToString(),
                                     Text = u.Email
                                 };

            theVM.TheBannerMsg = (UR_BannerMsg)LoadBannerMsg();

            theVM.RolesForTheUser = (IEnumerable<string>)TempData["rolesForTheUser"];
            theVM.TheUserID = (string)TempData["TheUserID"];

            return View( theVM );
        }


        [HttpGet]
        [ChildActionOnly]
        public PartialViewResult CreateRole() {
            if(ModelErrorHappened() == false) {
                return PartialView( "_CreateRole", new UserRoleInfo() );
            } else {
                LoadModelState();
                return PartialView( "_CreateRole" );
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRole(UserRoleInfo newRoleInfo) {
            if(ModelState.IsValid) {
                userDb.Roles.Add( new IdentityRole() {
                    Name = newRoleInfo.roleName
                } );

                userDb.SaveChanges();
                return RedirectToIndex( UR_BannerMsg.RoleCreated );
            } else {
                return RedirectToIndex();
            }
        }


        public ActionResult DeleteRole(string roleID) {

            var theRole = from r in userDb.Roles
                          where r.Id == roleID
                          select r;
            if(theRole != null) {
                userDb.Roles.Remove( theRole.FirstOrDefault() );
                userDb.SaveChanges();
                return RedirectToIndex( UR_BannerMsg.RoleDeleted );
            } else {
                return RedirectToIndex();
            }

        }

        public ActionResult EditRole(string roleID) {
            var theRole = (from r in userDb.Roles
                           where r.Id == roleID
                           select r).FirstOrDefault();

            var theRoleInfo = new UserRoleInfo {
                roleName = theRole.Name,
                roleID = theRole.Id
            };

            if(theRole != null) {
                return View( theRoleInfo );
            } else {
                return RedirectToIndex();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRole(UserRoleInfo userRoleInfo) {

            if(ModelState.IsValid) {
                var theRole = (from r in userDb.Roles
                               where r.Id == userRoleInfo.roleID
                               select r).FirstOrDefault();

                if(theRole != null) {
                    theRole.Name = userRoleInfo.roleName;
                    userDb.Entry( theRole ).State = EntityState.Modified;
                    userDb.SaveChanges();
                    return RedirectToIndex( UR_BannerMsg.RoleEdited );
                } else {
                    return RedirectToIndex( UR_BannerMsg.RoleNotEdited );
                }                               
               
            } else {
                return View( userRoleInfo );
            }

        }

        // Get the Roles a specific User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetRolesForUser(string userID) {
            if(!string.IsNullOrWhiteSpace( userID )) {
                var userManager = new UserManager<ApplicationUser>( new UserStore<ApplicationUser>( userDb ) );
                var rolesForTheUser = userManager.GetRoles( userID );
                TempData["rolesForTheUser"] = rolesForTheUser.Count != 0 ? rolesForTheUser : null;
                TempData["theUserID"] = userID;
            }

            return RedirectToIndex();
        }   

        // Delete a Role from a User
        public ActionResult DeleteRoleFromUser(string userID, string roleName) {
            if(roleName != null) {
                var userManager = new UserManager<ApplicationUser>( new UserStore<ApplicationUser>( userDb ) );

                if(userManager.IsInRole( userID, roleName )) {
                    userManager.RemoveFromRole( userID, roleName );
                    return RedirectToIndex( UR_BannerMsg.RoleRemovedUser );
                } else {
                    return RedirectToIndex( UR_BannerMsg.RoleNotRemovedUser );
                }
            }
            return RedirectToIndex( UR_BannerMsg.RoleNotRemovedUser );
        }

        // Add a Role to a User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRoleToUser(string userID, string roleID) {
            var roleName = from r in userDb.Roles
                           where r.Id == roleID
                           select r.Name;

            if(roleName != null) {
                var userManager = new UserManager<ApplicationUser>( new UserStore<ApplicationUser>( userDb ) );
                userManager.AddToRole( userID, roleName.FirstOrDefault() );
                return RedirectToIndex( UR_BannerMsg.RoleAddedUser );
            } else {
                return RedirectToIndex( UR_BannerMsg.RoleNotAddedUser );
            }
        }


    }

}
