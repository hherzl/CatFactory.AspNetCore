using CatFactory.EfCore;
using Xunit;

namespace CatFactory.AspNetCore.Tests
{
    public class CodeGenerationTests
    {
        [Fact]
        public void TestControllerGeneration()
        {
            var project = new EfCoreProject
            {
                Name = "Store",
                Database = StoreDatabase.Mock,
                OutputDirectory = "C:\\Temp\\CatFactory.AspNetCore\\Store.AspNetCore\\src\\Store.Core"
            };

            project.Settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");

            project.Settings.ConcurrencyToken = "Timestamp";

            project.BuildFeatures();

            project
                .GenerateEntityLayer()
                .GenerateDataLayer()
                .GenerateAspNetCoreProject();
        }
    }
}
