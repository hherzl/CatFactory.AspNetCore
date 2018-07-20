using CatFactory.EntityFrameworkCore;
using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.AspNetCore.Tests
{
    public class ImportTests
    {
        [Fact]
        public void TestControllerScaffoldingFromStoreDatabase()
        {
            // Import database
            var database = SqlServerDatabaseFactory
                .Import(SqlServerDatabaseFactory.GetLogger(), "server=(local);database=Store;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core Project
            var entityFrameworkProject = new EntityFrameworkCoreProject
            {
                Name = "Store",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.AspNetCore\\Store.Core"
            };

            // Apply settings for project
            entityFrameworkProject.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.ConcurrencyToken = "Timestamp";
                settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");
            });

            entityFrameworkProject.Select("Sales.Order", settings => settings.EntitiesWithDataContracts = true);

            // Build features for project, group all entities by schema into a feature
            entityFrameworkProject.BuildFeatures();

            // Scaffolding =^^=
            entityFrameworkProject
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();

            var aspNetCoreProject = entityFrameworkProject
                .CreateAspNetCoreProject("Store.Api", "C:\\Temp\\CatFactory.AspNetCore\\Store.Api", entityFrameworkProject.Database);

            aspNetCoreProject.ScaffoldAspNetCore();
        }

        [Fact]
        public void TestControllerScaffoldingFromNorthwindDatabase()
        {
            // Import database
            var database = SqlServerDatabaseFactory
                .Import(SqlServerDatabaseFactory.GetLogger(), "server=(local);database=Northwind;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core Project
            var entityFrameworkProject = new EntityFrameworkCoreProject
            {
                Name = "Northwind",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.AspNetCore\\Northwind.Core"
            };

            // Apply settings for project
            entityFrameworkProject.GlobalSelection(settings => settings.ForceOverwrite = true);
            entityFrameworkProject.Select("Sales.Order", settings => settings.EntitiesWithDataContracts = true);

            // Build features for project, group all entities by schema into a feature
            entityFrameworkProject.BuildFeatures();

            // Scaffolding =^^=
            entityFrameworkProject
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();

            var aspNetCoreProject = entityFrameworkProject
                .CreateAspNetCoreProject("Northwind.AspNetCore", "C:\\Temp\\CatFactory.AspNetCore\\Northwind.Api", entityFrameworkProject.Database);

            aspNetCoreProject.ScaffoldAspNetCore();
        }

        [Fact]
        public void TestControllerScaffoldingFromAdventureWorks2017Database()
        {
            // Import database
            var database = SqlServerDatabaseFactory
                .Import(SqlServerDatabaseFactory.GetLogger(), "server=(local);database=AdventureWorks2017;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core Project
            var entityFrameworkProject = new EntityFrameworkCoreProject
            {
                Name = "Northwind",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.AspNetCore\\AdventureWorks2017.Core"
            };

            // Apply settings for project
            entityFrameworkProject.GlobalSelection(settings => settings.ForceOverwrite = true);
            entityFrameworkProject.Select("Sales.Order", settings => settings.EntitiesWithDataContracts = true);

            // Build features for project, group all entities by schema into a feature
            entityFrameworkProject.BuildFeatures();

            // Scaffolding =^^=
            entityFrameworkProject
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();

            var aspNetCoreProject = entityFrameworkProject
                .CreateAspNetCoreProject("Northwind.AspNetCore", "C:\\Temp\\CatFactory.AspNetCore\\AdventureWorks2017.Api", entityFrameworkProject.Database);

            aspNetCoreProject.ScaffoldAspNetCore();
        }
    }
}
