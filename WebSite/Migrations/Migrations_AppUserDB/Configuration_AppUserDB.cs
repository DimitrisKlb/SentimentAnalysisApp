using System.Data.Entity.Migrations;

namespace WebSite.Migrations.Migrations_AppUserDB {

    internal sealed class Configuration_AppUserDB: DbMigrationsConfiguration<WebSite.Models.ApplicationDbContext> {
        public Configuration_AppUserDB() {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations\Migrations_AppUserDB";
        }

        protected override void Seed(WebSite.Models.ApplicationDbContext context) {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
