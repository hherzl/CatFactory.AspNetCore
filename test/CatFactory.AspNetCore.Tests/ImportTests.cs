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
            var project = new EntityFrameworkCoreProject
            {
                Name = "Store",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.AspNetCore\\CatFactory.AspNetCore.Demo\\src\\Store.Core"
            };

            // Apply settings for project
            project.Settings.ForceOverwrite = true;
            project.Settings.ConcurrencyToken = "Timestamp";
            project.Settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");
            project.Settings.EntitiesWithDataContracts.Add("Sales.Order");

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Create settings for AspNetCore project
            var aspNetCoreProjectSettings = new AspNetCoreProjectSettings
            {
                ProjectName = "Store.AspNetCore",
                OutputDirectory = "C:\\Temp\\CatFactory.AspNetCore\\CatFactory.AspNetCore.Demo\\src\\Store.AspNetCore"
            };

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer()
                .ScaffoldAspNetCore(aspNetCoreProjectSettings);
        }

        [Fact]
        public void TestControllerScaffoldingFromNorthwindDatabase()
        {
            // Import database
            var database = SqlServerDatabaseFactory
                .Import(LoggerMocker.GetLogger<SqlServerDatabaseFactory>(), "server=(local);database=Northwind;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core Project
            var project = new EntityFrameworkCoreProject
            {
                Name = "Northwind",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.AspNetCore\\CatFactory.AspNetCore.Demo\\src\\Northwind.Core"
            };

            // Apply settings for project
            project.Settings.ForceOverwrite = true;
            project.Settings.EntitiesWithDataContracts.Add("Orders");

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Create settings for AspNetCore project
            var aspNetCoreProjectSettings = new AspNetCoreProjectSettings
            {
                ProjectName = "Northwind.AspNetCore",
                OutputDirectory = "C:\\Temp\\CatFactory.AspNetCore\\CatFactory.AspNetCore.Demo\\src\\Northwind.AspNetCore"
            };

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer()
                .ScaffoldAspNetCore(aspNetCoreProjectSettings);
        }

        [Fact]
        public void TestSimpleControllerScaffoldingFromStoreDatabase()
        {
            var logger = LoggerMocker.GetLogger<SqlServerDatabaseFactory>();

            // Import database
            var database = SqlServerDatabaseFactory
                .Import(logger, "server=(local);database=Store;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core Project
            var project = new EntityFrameworkCoreProject
            {
                Name = "Store.Simple",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.AspNetCore\\CatFactory.AspNetCore.Demo\\src\\Store.Core.Simple"
            };

            // Apply settings for project
            project.Settings.ForceOverwrite = true;

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Create settings for AspNetCore project
            var aspNetCoreProjectSettings = new AspNetCoreProjectSettings
            {
                ProjectName = "Store.AspNetCore.Simple",
                OutputDirectory = "C:\\Temp\\CatFactory.AspNetCore\\CatFactory.AspNetCore.Demo\\src\\Store.AspNetCore.Simple"
            };

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer()
                .ScaffoldAspNetCore(aspNetCoreProjectSettings);
        }
    }
}
