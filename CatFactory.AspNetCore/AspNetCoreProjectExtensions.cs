using System.Collections.Generic;
using System.IO;
using CatFactory.AspNetCore.Definitions.Extensions;
using CatFactory.EntityFrameworkCore;
using CatFactory.Markdown;
using CatFactory.NetCore.ObjectOrientedProgramming;

namespace CatFactory.AspNetCore
{
    public static class AspNetCoreProjectExtensions
    {
        public static AspNetCoreProject ScaffoldAspNetCore(this AspNetCoreProject aspNetCoreProject)
        {
            aspNetCoreProject.ScaffoldRequests();
            aspNetCoreProject.ScaffoldRequestsExtensions();
            aspNetCoreProject.ScaffoldResponses();
            aspNetCoreProject.ScaffoldResponsesExtensions();

            foreach (var feature in aspNetCoreProject.Features)
            {
                aspNetCoreProject.Scaffold(feature.GetControllerClassDefinition(), aspNetCoreProject.OutputDirectory, aspNetCoreProject.AspNetCoreProjectNamespaces.Controllers);
            }

            aspNetCoreProject.ScaffoldMdReadMe();

            return aspNetCoreProject;
        }

        internal static void ScaffoldRequests(this AspNetCoreProject project)
        {
            foreach (var table in project.Database.Tables)
            {
                var getRequestClassDefinition = project.GetGetRequestClassDefinition(table);

                project.Scaffold(getRequestClassDefinition, project.OutputDirectory, project.AspNetCoreProjectNamespaces.Requests);

                var postRequestClassDefinition = project.GetPostRequestClassDefinition(table);

                project.Scaffold(postRequestClassDefinition, project.OutputDirectory, project.AspNetCoreProjectNamespaces.Requests);

                var putRequestClassDefinition = project.GetPutRequestClassDefinition(table);

                project.Scaffold(putRequestClassDefinition, project.OutputDirectory, project.AspNetCoreProjectNamespaces.Requests);
            }
        }

        internal static void ScaffoldRequestsExtensions(this AspNetCoreProject project)
        {
            foreach (var table in project.Database.Tables)
            {
                var classDefinition = project.GetRequestExtensionsClassDefinition(table);

                project.Scaffold(classDefinition, project.OutputDirectory, project.AspNetCoreProjectNamespaces.Requests);
            }
        }

        internal static void ScaffoldResponses(this AspNetCoreProject project)
        {
            var interfaces = new List<CSharpInterfaceDefinition>
            {
                project.GetResponseInterfaceDefinition(),
                project.GetListResponseInterfaceDefinition(),
                project.GetPagedResponseInterfaceDefinition(),
                project.GetSingleResponseInterfaceDefinition(),
                project.GetPostResponseInterfaceDefinition()
            };

            foreach (var definition in interfaces)
            {
                project.Scaffold(definition, project.OutputDirectory, project.AspNetCoreProjectNamespaces.Responses);
            }

            var classes = new List<CSharpClassDefinition>
            {
                project.GetResponseClassDefinition(),
                project.GetListResponseClassDefinition(),
                project.GetPagedResponseClassDefinition(),
                project.GetSingleResponseClassDefinition(),
                project.GetPostResponseClassDefinition(),
            };

            foreach (var definition in classes)
            {
                project.Scaffold(definition, project.OutputDirectory, project.AspNetCoreProjectNamespaces.Responses);
            }
        }

        internal static void ScaffoldResponsesExtensions(this AspNetCoreProject project)
        {
            project.Scaffold(project.GetResponsesExtensionsClassDefinition(), project.OutputDirectory, project.AspNetCoreProjectNamespaces.Responses);
        }

        internal static void ScaffoldMdReadMe(this AspNetCoreProject project)
        {
            var readMe = new MdDocument();

            readMe.H1("CatFactory ==^^==: Scaffolding Made Easy");

            readMe.WriteLine("How to use this code on your ASP.NET Core Application:");

            readMe.OrderedList(
                "Install EntityFrameworkCore.SqlServer package",
                "Register the DbContext and Repositories in ConfigureServices method (Startup class)"
                );

            readMe.H2("Install package");

            readMe.WriteLine("You can install the NuGet packages in Visual Studio or Windows Command Line, for more info:");

            readMe.WriteLine(
                Md.Link("Install and manage packages with the Package Manager Console in Visual Studio (PowerShell)", "https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-powershell")
                );

            readMe.WriteLine(
                Md.Link(".NET Core CLI", "https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-add-package")
                );

            readMe.H2("Register DbContext and Repositories");

            readMe.WriteLine("Add the following code lines in {0} method (Startup class):", Md.Bold("ConfigureServices"));
            readMe.WriteLine("  services.AddDbContext<{0}>(options => options.UseSqlServer(\"ConnectionString\"));", project.EntityFrameworkCoreProject.GetDbContextName(project.Database));
            readMe.WriteLine("  services.AddScope<{0}, {1}>()", "IDboRepository", "DboRepository");

            readMe.H2("Register Loggers");

            readMe.WriteLine("Add the following code lines in {0} method (Startup class):");
            readMe.WriteLine("  services.AddScoped<ILogger<DboController>, Logger<DboController>>();");

            if (FluentValidationExtensions.Used)
                readMe.WriteLine("You have been enabled Fluent Validation for your project, read more information on {0}.", Md.Link("This link", "https://fluentvalidation.net/aspnet"));

            readMe.WriteLine("Happy scaffolding!");

            var codeProjectLink = Md.Link("Scaffolding ASP.NET Core with CatFactory", "https://www.codeproject.com/Tips/1229909/Scaffolding-ASP-NET-Core-with-CatFactory");

            readMe.WriteLine("You can check the guide for this package in: {0}", codeProjectLink);

            var gitHubRepositoryLink = Md.Link("GitHub repository", "https://github.com/hherzl/CatFactory.AspNetCore");

            readMe.WriteLine("Also you can check the source code on {0}", gitHubRepositoryLink);

            readMe.WriteLine("CatFactory Development Team ==^^==");

            File.WriteAllText(Path.Combine(project.OutputDirectory, "CatFactory.AspNetCore.ReadMe.MD"), readMe.ToString());
        }
    }
}
