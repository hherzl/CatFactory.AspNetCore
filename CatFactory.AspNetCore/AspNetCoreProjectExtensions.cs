using System.Collections.Generic;
using System.IO;
using CatFactory.AspNetCore.Definitions.Extensions;
using CatFactory.Collections;
using CatFactory.EntityFrameworkCore;
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

            aspNetCoreProject.ScaffoldReadMe();

            return aspNetCoreProject;
        }

        internal static void ScaffoldRequests(this AspNetCoreProject project)
        {
            foreach (var table in project.Database.Tables)
            {
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
                project.GetSingleResponseInterfaceDefinition(),
                project.GetListResponseInterfaceDefinition(),
                project.GetPagedResponseInterfaceDefinition()
            };

            foreach (var definition in interfaces)
            {
                project.Scaffold(definition, project.OutputDirectory, project.AspNetCoreProjectNamespaces.Responses);
            }

            var classes = new List<CSharpClassDefinition>
            {
                project.GetResponseClassDefinition(),
                project.GetSingleResponseClassDefinition(),
                project.GetListResponseClassDefinition(),
                project.GetPagedResponseClassDefinition()
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

        internal static void ScaffoldReadMe(this AspNetCoreProject project)
        {
            var lines = new List<string>
            {
                "CatFactory: Scaffolding Made Easy",
                string.Empty,

                "How to use this code:",
                string.Empty,

                "1. Install EntityFrameworkCore.SqlServer package",
                string.Empty,

                "2. Register your DbContext and repositories in ConfigureServices method (Startup class):",
                string.Format(" services.AddDbContext<{0}>(options => options.UseSqlServer(Configuration[\"ConnectionString\"]));", project.EntityFrameworkCoreProject.GetDbContextName(project.Database)),

                " services.AddScoped<IDboRepository, DboRepository>();",
                string.Empty,

                "3. Register logger instance for your controllers in ConfigureServices method (Startup class):",
                string.Format(" services.AddScoped<ILogger<DboController>, Logger<DboController>>();"),

                string.Empty,
                "Happy scaffolding!",
                string.Empty,

                "You can check the guide for this package in:",
                "https://www.codeproject.com/Tips/1229909/Scaffolding-ASP-NET-Core-with-CatFactory",
                string.Empty,
                "Also you can check source code on GitHub:",
                "https://github.com/hherzl/CatFactory.AspNetCore",
                string.Empty,
                "CatFactory Development Team ==^^=="
            };

            File.WriteAllText(Path.Combine(project.OutputDirectory, "CatFactory.AspNetCore.ReadMe.txt"), lines.ToStringBuilder().ToString());
        }
    }
}
