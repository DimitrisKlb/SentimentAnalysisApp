using System.Data.Entity.Migrations;

namespace WebSite.Migrations.Migrations_FEMainDB {

    internal sealed class Configuration_FEMainDB: DbMigrationsConfiguration<WebSite.Models.FEMainDBContext> {
        public Configuration_FEMainDB() {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(WebSite.Models.FEMainDBContext context) {
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
