using CatFactory.EntityFrameworkCore;
using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.AspNetCore.Tests;

public class ScaffoldingTests
{
    [Fact]
    public async Task ScaffoldingNorthwindAPIAsync()
    {
        // Import database
        var database = await SqlServerDatabaseFactory
            .ImportAsync("server=(local); database=Northwind; integrated security=yes; TrustServerCertificate=True;", "dbo.sysdiagrams");

        // Create instance of Entity Framework Core Project
        var efCoreProject = EntityFrameworkCoreProject
            .CreateForV5x("Northwind.Domain", database, @"C:\Temp\CatFactory.AspNetCore\Northwind\Northwind.Domain");

        // Apply settings for project
        efCoreProject.GlobalSelection(settings =>
        {
            settings.ForceOverwrite = true;
            settings.UseApplyConfigurationsFromAssemblyMethod = true;
        });

        // Build features for project, group all entities by schema into a feature
        efCoreProject.BuildFeatures();

        // Scaffolding =^^=
        efCoreProject
            .ScaffoldDomain()
            ;

        var aspNetCoreProject = efCoreProject
            .CreateAspNetCore3xProject("Northwind.API", @"C:\Temp\CatFactory.AspNetCore\Northwind\Northwind.API");

        aspNetCoreProject.GlobalSelection(settings => settings.ForceOverwrite = true);

        aspNetCoreProject
            .ScaffoldAspNetCore()
            ;
    }
}
