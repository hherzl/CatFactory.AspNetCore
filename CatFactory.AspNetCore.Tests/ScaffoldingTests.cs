﻿using System.Threading.Tasks;
using CatFactory.EntityFrameworkCore;
using CatFactory.ObjectRelationalMapping.Actions;
using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.AspNetCore.Tests
{
    public class ScaffoldingTests
    {
        [Fact]
        public async Task ScaffoldingAPIFromOnlineStoreDatabaseAsync()
        {
            // Import database
            var database = await SqlServerDatabaseFactory
                .ImportAsync("server=(local);database=OnlineStore;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core Project
            var entityFrameworkProject = EntityFrameworkCoreProject
                .CreateForV2x("OnlineStore.Domain", database, @"C:\Temp\CatFactory.AspNetCore\OnlineStore.Domain");

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
                .ScaffoldDomain()
                ;

            var aspNetCoreProject = entityFrameworkProject
                .CreateAspNetCore2xProject("OnlineStore.API", @"C:\Temp\CatFactory.AspNetCore\OnlineStore.API");

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

        //[Fact]
        //public async Task ScaffoldingWebAPIFromOnlineStoreDatabaseAsync()
        //{
        //    // Import database
        //    var database = await SqlServerDatabaseFactory
        //        .ImportAsync("server=(local);database=OnlineStore;integrated security=yes;", "dbo.sysdiagrams");

        //    // Create instance of Entity Framework Core Project
        //    var entityFrameworkProject = EntityFrameworkCoreProject
        //        .CreateForV2x("OnlineStore.Core", database, @"C:\Temp\CatFactory.AspNetCore\OnlineStore.Core");

        //    // Apply settings for project
        //    entityFrameworkProject.GlobalSelection(settings =>
        //    {
        //        settings.ForceOverwrite = true;
        //        settings.ConcurrencyToken = "Timestamp";
        //        settings.AuditEntity = new AuditEntity
        //        {
        //            CreationUserColumnName = "CreationUser",
        //            CreationDateTimeColumnName = "CreationDateTime",
        //            LastUpdateUserColumnName = "LastUpdateUser",
        //            LastUpdateDateTimeColumnName = "LastUpdateDateTime"
        //        };
        //    });

        //    entityFrameworkProject.Selection("Sales.OrderHeader", settings => settings.EntitiesWithDataContracts = true);

        //    // Build features for project, group all entities by schema into a feature
        //    entityFrameworkProject.BuildFeatures();

        //    // Scaffolding =^^=
        //    entityFrameworkProject
        //        .ScaffoldEntityLayer()
        //        .ScaffoldDataLayer();

        //    var aspNetCoreProject = entityFrameworkProject
        //        .CreateAspNetCore2xProject("OnlineStore.WebAPI", @"C:\Temp\CatFactory.AspNetCore\OnlineStore.WebAPI");

        //    aspNetCoreProject.GlobalSelection(settings => settings.ForceOverwrite = true);

        //    aspNetCoreProject.Selection("Sales.OrderDetail", settings =>
        //    {
        //        settings
        //            .RemoveAction<ReadAllAction>()
        //            .RemoveAction<ReadByKeyAction>()
        //            .RemoveAction<AddEntityAction>()
        //            .RemoveAction<UpdateEntityAction>()
        //            .RemoveAction<RemoveEntityAction>();
        //    });

        //    aspNetCoreProject.ScaffoldAspNetCore();
        //}

        //[Fact]
        //public async Task ScaffoldingAPIWithFluentValidationFromOnlineStoreDatabaseAsync()
        //{
        //    // Import database
        //    var database = await SqlServerDatabaseFactory
        //        .ImportAsync("server=(local);database=OnlineStore;integrated security=yes;", "dbo.sysdiagrams");

        //    // Create instance of Entity Framework Core Project
        //    var entityFrameworkProject = EntityFrameworkCoreProject
        //        .CreateForV2x("OnlineStore.Core", database, @"C:\Temp\CatFactory.AspNetCore\OnlineStore.Core");

        //    // Apply settings for project
        //    entityFrameworkProject.GlobalSelection(settings =>
        //    {
        //        settings.ForceOverwrite = true;
        //        settings.ConcurrencyToken = "Timestamp";
        //        settings.AuditEntity = new AuditEntity
        //        {
        //            CreationUserColumnName = "CreationUser",
        //            CreationDateTimeColumnName = "CreationDateTime",
        //            LastUpdateUserColumnName = "LastUpdateUser",
        //            LastUpdateDateTimeColumnName = "LastUpdateDateTime"
        //        };
        //    });

        //    entityFrameworkProject.Selection("Sales.OrderHeader", settings => settings.EntitiesWithDataContracts = true);

        //    // Build features for project, group all entities by schema into a feature
        //    entityFrameworkProject.BuildFeatures();

        //    // Scaffolding =^^=
        //    entityFrameworkProject
        //        .ScaffoldEntityLayer()
        //        .ScaffoldDataLayer()
        //        ;

        //    var aspNetCoreProject = entityFrameworkProject
        //        .CreateAspNetCore2xProject("OnlineStoreWithFluentValidation.WebAPI", @"C:\Temp\CatFactory.AspNetCore\OnlineStoreWithFluentValidation.WebAPI");

        //    aspNetCoreProject.GlobalSelection(settings =>
        //    {
        //        settings.ForceOverwrite = true;
        //        settings.UseDataAnnotationsToValidateRequestModels = false;
        //    });

        //    aspNetCoreProject.Selection("Sales.OrderDetail", settings =>
        //    {
        //        settings
        //            .RemoveAction<ReadAllAction>()
        //            .RemoveAction<ReadByKeyAction>()
        //            .RemoveAction<AddEntityAction>()
        //            .RemoveAction<UpdateEntityAction>()
        //            .RemoveAction<RemoveEntityAction>();
        //    });

        //    aspNetCoreProject
        //        .ScaffoldFluentValidation()
        //        .ScaffoldAspNetCore()
        //        ;
        //}

        //[Fact]
        //public async Task ScaffoldingAPIFromNorthwindDatabaseAsync()
        //{
        //    // Import database
        //    var database = await SqlServerDatabaseFactory
        //        .ImportAsync("server=(local);database=Northwind;integrated security=yes;", "dbo.sysdiagrams");

        //    // Create instance of Entity Framework Core Project
        //    var entityFrameworkProject = EntityFrameworkCoreProject
        //        .CreateForV2x("Northwind.Core", database, @"C:\Temp\CatFactory.AspNetCore\Northwind.Core");

        //    // Apply settings for project
        //    entityFrameworkProject.GlobalSelection(settings =>
        //    {
        //        settings.ForceOverwrite = true;
        //        settings.ConcurrencyToken = "Timestamp";
        //        settings.AuditEntity = new AuditEntity
        //        {
        //            CreationUserColumnName = "CreationUser",
        //            CreationDateTimeColumnName = "CreationDateTime",
        //            LastUpdateUserColumnName = "LastUpdateUser",
        //            LastUpdateDateTimeColumnName = "LastUpdateDateTime"
        //        };
        //    });

        //    entityFrameworkProject.Selection("dbo.Orders", settings => settings.EntitiesWithDataContracts = true);

        //    // Build features for project, group all entities by schema into a feature
        //    entityFrameworkProject.BuildFeatures();

        //    // Scaffolding =^^=
        //    entityFrameworkProject
        //        .ScaffoldEntityLayer()
        //        .ScaffoldDataLayer()
        //        ;

        //    var aspNetCoreProject = entityFrameworkProject
        //        .CreateAspNetCore2xProject("Northwind.WebAPI", @"C:\Temp\CatFactory.AspNetCore\Northwind.WebAPI");

        //    aspNetCoreProject.GlobalSelection(settings => settings.ForceOverwrite = true);

        //    aspNetCoreProject.ScaffoldAspNetCore();
        //}
    }
}
