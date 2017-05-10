using System.Data.Entity.Migrations;

using SentimentAnalysisApp.SharedModels;

namespace WebSite.Migrations.Migrations_FEMainDB {

    internal sealed class Configuration_FEMainDB: DbMigrationsConfiguration<WebSite.Models.FEMainDBContext> {
        public Configuration_FEMainDB() {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations\Migrations_FEMainDB";
        }

        protected override void Seed(WebSite.Models.FEMainDBContext context) {
            
        }
    }
}
