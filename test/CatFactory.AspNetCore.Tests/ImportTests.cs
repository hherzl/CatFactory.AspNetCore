using CatFactory.EfCore;
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
                .Import(LoggerMocker.GetLogger<SqlServerDatabaseFactory>(), "server=(local);database=Store;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core Project
            var entityFrameworkProject = new EntityFrameworkCoreProject
            {
                Name = "Store",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.AspNetCore\\CatFactory.AspNetCore.Demo\\src\\Store.Core"
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

            entityFrameworkProject
                .ScaffoldAspNetCore("Store", "C:\\Temp\\CatFactory.AspNetCore\\CatFactory.AspNetCore.Demo\\src\\Store.AspNetCore", entityFrameworkProject.Database);
        }

        //[Fact]
        //public void TestControllerScaffoldingFromNorthwindDatabase()
        //{
        //    // Import database
        //    var database = SqlServerDatabaseFactory
        //        .Import(LoggerMocker.GetLogger<SqlServerDatabaseFactory>(), "server=(local);database=Northwind;integrated security=yes;", "dbo.sysdiagrams");

        //    // Create instance of Entity Framework Core Project
        //    var entityFrameworkProject = new EntityFrameworkCoreProject
        //    {
        //        Name = "Northwind",
        //        Database = database,
        //        OutputDirectory = "C:\\Temp\\CatFactory.AspNetCore\\CatFactory.AspNetCore.Demo\\src\\Northwind.Core"
        //    };

        //    // Apply settings for project
        //    entityFrameworkProject.GlobalSelection(settings => settings.ForceOverwrite = true);

        //    entityFrameworkProject.Select("Sales.Order", settings => settings.EntitiesWithDataContracts = true);

        //    // Build features for project, group all entities by schema into a feature
        //    entityFrameworkProject.BuildFeatures();

        //    // Scaffolding =^^=
        //    entityFrameworkProject
        //        .ScaffoldEntityLayer()
        //        .ScaffoldDataLayer();

        //    entityFrameworkProject
        //        .ScaffoldAspNetCore("Northwind.AspNetCore", "C:\\Temp\\CatFactory.AspNetCore\\CatFactory.AspNetCore.Demo\\src\\Northwind.AspNetCore", entityFrameworkProject.Database);
        //}

        //[Fact]
        //public void TestSimpleControllerScaffoldingFromStoreDatabase()
        //{
        //    var logger = LoggerMocker.GetLogger<SqlServerDatabaseFactory>();

        //    // Import database
        //    var database = SqlServerDatabaseFactory
        //        .Import(logger, "server=(local);database=Store;integrated security=yes;", "dbo.sysdiagrams");

        //    // Create instance of Entity Framework Core Project
        //    var entityFrameworkProject = new EntityFrameworkCoreProject
        //    {
        //        Name = "Store.Simple",
        //        Database = database,
        //        OutputDirectory = "C:\\Temp\\CatFactory.AspNetCore\\CatFactory.AspNetCore.Demo\\src\\Store.Core.Simple"
        //    };

        //    // Apply settings for project
        //    entityFrameworkProject.GlobalSelection(settings => settings.ForceOverwrite = true);

        //    // Build features for project, group all entities by schema into a feature
        //    entityFrameworkProject.BuildFeatures();

        //    // Create settings for AspNetCore project
        //    var aspNetCoreProjectSettings = new AspNetCoreProject
        //    {
        //        Name = "Store.AspNetCore.Simple",
        //        OutputDirectory = ""
        //    };

        //    // Scaffolding =^^=
        //    entityFrameworkProject
        //        .ScaffoldEntityLayer()
        //        .ScaffoldDataLayer();

        //    entityFrameworkProject
        //        .ScaffoldAspNetCore("Store.AspNetCore.Simple", "C:\\Temp\\CatFactory.AspNetCore\\CatFactory.AspNetCore.Demo\\src\\Store.AspNetCore.Simple", entityFrameworkProject.Database);
        //}
    }
}
