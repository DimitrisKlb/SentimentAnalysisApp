namespace WebSite.Migrations.Migrations_FEMainDB {
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration_FEMainDB: DbMigrationsConfiguration<WebSite.Models.FEMainDBContext> {
        public Configuration_FEMainDB() {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations\Migrations_FEMainDB";
        }

        protected override void Seed(WebSite.Models.FEMainDBContext context) {

        }
    }
}
