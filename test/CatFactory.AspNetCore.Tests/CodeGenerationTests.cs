using CatFactory.EfCore;
using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.AspNetCore.Tests
{
    public class CodeGenerationTests
    {
        [Fact]
        public void TestControllerGenerationFromExistingDatabase()
        {
            var logger = LoggerMocker.GetLogger<SqlServerDatabaseFactory>();

            var database = SqlServerDatabaseFactory
                .Import(logger, "server=(local);database=Store;integrated security=yes;", "dbo.sysdiagrams");

            var project = new EfCoreProject
            {
                Name = "Store",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.AspNetCore\\Store.AspNetCore\\src\\Store.Core"
            };

            project.Settings.ForceOverwrite = true;

            project.Settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");

            project.Settings.ConcurrencyToken = "Timestamp";

            project.BuildFeatures();

            var aspNetCoreProjectSettings = new AspNetCoreProjectSettings
            {
                ProjectName = "Store.AspNetCore",
                OutputDirectory = "C:\\Temp\\CatFactory.AspNetCore\\Store.AspNetCore\\src\\Store.AspNetCore"
            };

            project
                .GenerateEntityLayer()
                .GenerateDataLayer()
                .GenerateAspNetCoreProject(aspNetCoreProjectSettings);
        }
    }
}
