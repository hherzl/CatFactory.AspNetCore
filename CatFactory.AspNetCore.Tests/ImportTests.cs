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
                Name = "Store.Core",
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

            // Add event handlers to before and after of scaffold

            aspNetCoreProject.ScaffoldingDefinition += (source, args) =>
            {
                // Add code to perform operations with code builder instance before to create code file
            };

            aspNetCoreProject.ScaffoldedDefinition += (source, args) =>
            {
                // Add code to perform operations after of create code file
            };

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
                Name = "Northwind.Core",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.AspNetCore\\Northwind.Core"
            };

            // Apply settings for project
            entityFrameworkProject.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.ConcurrencyToken = "Timestamp";
                settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");
            });

            entityFrameworkProject.Select("dbo.Orders", settings => settings.EntitiesWithDataContracts = true);

            // Build features for project, group all entities by schema into a feature
            entityFrameworkProject.BuildFeatures();

            // Scaffolding =^^=
            entityFrameworkProject
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();

            var aspNetCoreProject = entityFrameworkProject
                .CreateAspNetCoreProject("Northwind.Api", "C:\\Temp\\CatFactory.AspNetCore\\Northwind.Api", entityFrameworkProject.Database);

            // Add event handlers to before and after of scaffold

            aspNetCoreProject.ScaffoldingDefinition += (source, args) =>
            {
                // Add code to perform operations with code builder instance before to create code file
            };

            aspNetCoreProject.ScaffoldedDefinition += (source, args) =>
            {
                // Add code to perform operations after of create code file
            };

            aspNetCoreProject.ScaffoldAspNetCore();
        }
    }
}
