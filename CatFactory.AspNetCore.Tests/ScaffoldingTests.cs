using CatFactory.EntityFrameworkCore;
using CatFactory.ObjectRelationalMapping.Actions;
using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.AspNetCore.Tests
{
    public class ScaffoldingTests
    {
        [Fact]
        public void TestScaffoldingWebApiFromOnlineStoreDatabase()
        {
            // Import database
            var database = SqlServerDatabaseFactory
                .Import("server=(local);database=OnlineStore;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core Project
            var entityFrameworkProject = new EntityFrameworkCoreProject
            {
                Name = "OnlineStore.Core",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.AspNetCore\OnlineStore.Core"
            };

            // Apply settings for project
            entityFrameworkProject.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.ConcurrencyToken = "Timestamp";
                settings.AuditEntity = new AuditEntity
                {
                    CreationUserColumnName = "CreationUser",
                    CreationDateTimeColumnName = "CreationDateTime",
                    LastUpdateUserColumnName = "LastUpdateUser",
                    LastUpdateDateTimeColumnName = "LastUpdateDateTime"
                };
            });

            entityFrameworkProject.Selection("Sales.OrderHeader", settings => settings.EntitiesWithDataContracts = true);

            // Build features for project, group all entities by schema into a feature
            entityFrameworkProject.BuildFeatures();

            // Scaffolding =^^=
            entityFrameworkProject
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();

            var aspNetCoreProject = entityFrameworkProject
                .CreateAspNetCoreProject("OnlineStore.WebAPI", @"C:\Temp\CatFactory.AspNetCore\OnlineStore.WebAPI");

            aspNetCoreProject.GlobalSelection(settings => settings.ForceOverwrite = true);

            aspNetCoreProject.Selection("Sales.OrderDetail", settings =>
            {
                settings
                    .RemoveAction<ReadAllAction>()
                    .RemoveAction<ReadByKeyAction>()
                    .RemoveAction<AddEntityAction>()
                    .RemoveAction<UpdateEntityAction>()
                    .RemoveAction<RemoveEntityAction>();
            });

            aspNetCoreProject.ScaffoldAspNetCore();
        }

        [Fact]
        public void TestScaffoldingWebApiWithFluentValidationFromOnlineStoreDatabase()
        {
            // Import database
            var database = SqlServerDatabaseFactory
                .Import("server=(local);database=OnlineStore;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core Project
            var entityFrameworkProject = new EntityFrameworkCoreProject
            {
                Name = "OnlineStore.Core",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.AspNetCore\OnlineStore.Core"
            };

            // Apply settings for project
            entityFrameworkProject.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.ConcurrencyToken = "Timestamp";
                settings.AuditEntity = new AuditEntity
                {
                    CreationUserColumnName = "CreationUser",
                    CreationDateTimeColumnName = "CreationDateTime",
                    LastUpdateUserColumnName = "LastUpdateUser",
                    LastUpdateDateTimeColumnName = "LastUpdateDateTime"
                };
            });

            entityFrameworkProject.Selection("Sales.OrderHeader", settings => settings.EntitiesWithDataContracts = true);

            // Build features for project, group all entities by schema into a feature
            entityFrameworkProject.BuildFeatures();

            // Scaffolding =^^=
            entityFrameworkProject
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();

            var aspNetCoreProject = entityFrameworkProject
                .CreateAspNetCoreProject("OnlineStoreWithFluentValidation.WebAPI", @"C:\Temp\CatFactory.AspNetCore\OnlineStoreWithFluentValidation.WebAPI");

            aspNetCoreProject.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.UseDataAnnotationsToValidateRequestModels = false;
            });

            aspNetCoreProject.Selection("Sales.OrderDetail", settings =>
            {
                settings
                    .RemoveAction<ReadAllAction>()
                    .RemoveAction<ReadByKeyAction>()
                    .RemoveAction<AddEntityAction>()
                    .RemoveAction<UpdateEntityAction>()
                    .RemoveAction<RemoveEntityAction>();
            });

            aspNetCoreProject
                .ScaffoldFluentValidation()
                .ScaffoldAspNetCore()
                ;
        }

        [Fact]
        public void TestScaffoldingWebApiFromNorthwindDatabase()
        {
            // Import database
            var database = SqlServerDatabaseFactory
                .Import("server=(local);database=Northwind;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core Project
            var entityFrameworkProject = new EntityFrameworkCoreProject
            {
                Name = "Northwind.Core",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.AspNetCore\Northwind.Core"
            };

            // Apply settings for project
            entityFrameworkProject.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.ConcurrencyToken = "Timestamp";
                settings.AuditEntity = new AuditEntity
                {
                    CreationUserColumnName = "CreationUser",
                    CreationDateTimeColumnName = "CreationDateTime",
                    LastUpdateUserColumnName = "LastUpdateUser",
                    LastUpdateDateTimeColumnName = "LastUpdateDateTime"
                };
            });

            entityFrameworkProject.Selection("dbo.Orders", settings => settings.EntitiesWithDataContracts = true);

            // Build features for project, group all entities by schema into a feature
            entityFrameworkProject.BuildFeatures();

            // Scaffolding =^^=
            entityFrameworkProject
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();

            var aspNetCoreProject = entityFrameworkProject
                .CreateAspNetCoreProject("Northwind.WebAPI", @"C:\Temp\CatFactory.AspNetCore\Northwind.WebAPI");

            aspNetCoreProject.GlobalSelection(settings => settings.ForceOverwrite = true);

            aspNetCoreProject.ScaffoldAspNetCore();
        }
    }
}
