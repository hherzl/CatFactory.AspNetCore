using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CatFactory.AspNetCore.Definitions.Extensions;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.Collections;
using CatFactory.EntityFrameworkCore;
using CatFactory.NetCore.CodeFactory;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.AspNetCore
{
    public static class AspNetCoreProjectExtensions
    {
        public static string GetControllerGetAllAsyncMethodName(this AspNetCoreProject project, ITable table)
            => string.Format("{0}{1}{2}", "Get", project.EntityFrameworkCoreProject.GetPluralName(table), "Async");

        public static string GetResponsesNamespace(this AspNetCoreProject project)
            => string.Format("{0}.{1}", project.Name, project.AspNetCoreProjectNamespaces.Responses);

        public static string GetRequestsNamespace(this AspNetCoreProject project)
            => string.Format("{0}.{1}", project.Name, project.AspNetCoreProjectNamespaces.Requests);

        public static string GetControllerGetAsyncMethodName(this AspNetCoreProject project, ITable table)
            => string.Format("{0}{1}{2}", "Get", project.EntityFrameworkCoreProject.GetEntityName(table), "Async");

        public static string GetControllerPostAsyncMethodName(this AspNetCoreProject project, ITable table)
            => string.Format("{0}{1}{2}", "Post", project.EntityFrameworkCoreProject.GetEntityName(table), "Async");

        public static string GetControllerPutAsyncMethodName(this AspNetCoreProject project, ITable table)
            => string.Format("{0}{1}{2}", "Put", project.EntityFrameworkCoreProject.GetEntityName(table), "Async");

        public static string GetControllerDeleteAsyncMethodName(this AspNetCoreProject project, ITable table)
            => string.Format("{0}{1}{2}", "Delete", project.EntityFrameworkCoreProject.GetEntityName(table), "Async");

        public static string GetRequestName(this AspNetCoreProject project, ITable table)
            => string.Format("{0}Request", project.EntityFrameworkCoreProject.GetEntityName(table));

        public static string GetRequestExtensionName(this AspNetCoreProject project, ITable table)
            => string.Format("{0}Extensions", project.GetRequestName(table));

        public static string GetEntityLayerNamespace(this AspNetCoreProject project)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.EntityFrameworkCoreProject.Name), project.CodeNamingConvention.GetNamespace(project.EntityFrameworkCoreProjectNamespaces.EntityLayer));

        public static string GetEntityLayerNamespace(this AspNetCoreProject project, string ns)
            => string.IsNullOrEmpty(ns) ? GetEntityLayerNamespace(project) : string.Join(".", project.EntityFrameworkCoreProject.Name, project.EntityFrameworkCoreProjectNamespaces.EntityLayer, ns);

        public static string GetDataLayerContractsNamespace(this AspNetCoreProject project)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.EntityFrameworkCoreProject.Name), project.EntityFrameworkCoreProjectNamespaces.DataLayer, project.EntityFrameworkCoreProjectNamespaces.Contracts);

        public static string GetDataLayerDataContractsNamespace(this AspNetCoreProject project)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.EntityFrameworkCoreProject.Name), project.EntityFrameworkCoreProjectNamespaces.DataLayer, project.EntityFrameworkCoreProjectNamespaces.DataContracts);

        public static string GetDataLayerRepositoriesNamespace(this AspNetCoreProject project)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.EntityFrameworkCoreProject.Name), project.EntityFrameworkCoreProjectNamespaces.DataLayer, project.EntityFrameworkCoreProjectNamespaces.Repositories);

        public static AspNetCoreProject GlobalSelection(this AspNetCoreProject project, Action<AspNetCoreProjectSettings> action = null)
        {
            var settings = new AspNetCoreProjectSettings();

            var selection = project.Selections.FirstOrDefault(item => item.IsGlobal);

            if (selection == null)
            {
                selection = new ProjectSelection<AspNetCoreProjectSettings>
                {
                    Pattern = ProjectSelection<AspNetCoreProjectSettings>.GlobalPattern,
                    Settings = settings
                };

                project.Selections.Add(selection);
            }
            else
            {
                settings = selection.Settings;
            }

            action?.Invoke(settings);

            return project;
        }

        public static ProjectSelection<AspNetCoreProjectSettings> GlobalSelection(this AspNetCoreProject project)
            => project.Selections.FirstOrDefault(item => item.IsGlobal);

        public static AspNetCoreProject Selection(this AspNetCoreProject project, string pattern, Action<AspNetCoreProjectSettings> action = null)
        {
            var selection = project.Selections.FirstOrDefault(item => item.Pattern == pattern);

            if (selection == null)
            {
                var globalSettings = project.GlobalSelection().Settings;

                selection = new ProjectSelection<AspNetCoreProjectSettings>
                {
                    Pattern = pattern,
                    Settings = new AspNetCoreProjectSettings
                    {
                        ForceOverwrite = globalSettings.ForceOverwrite,
                        UseLogger = globalSettings.UseLogger
                    }
                };

                project.Selections.Add(selection);
            }

            action?.Invoke(selection.Settings);

            return project;
        }

        [Obsolete("Use Selection method")]
        public static AspNetCoreProject Select(this AspNetCoreProject project, string pattern, Action<AspNetCoreProjectSettings> action = null)
            => project.Selection(pattern, action);

        public static ProjectSelection<AspNetCoreProjectSettings> GetSelection(this AspNetCoreProject project, ITable table)
        {
            // Sales.Order
            var selectionForFullName = project.Selections.FirstOrDefault(item => item.Pattern == table.FullName);

            if (selectionForFullName != null)
                return selectionForFullName;

            // Sales.*
            var selectionForSchema = project.Selections.FirstOrDefault(item => item.Pattern == string.Format("{0}.*", table.Schema));

            if (selectionForSchema != null)
                return selectionForSchema;

            // *.Order
            var selectionForName = project.Selections.FirstOrDefault(item => item.Pattern == string.Format("*.{0}", table.Name));

            if (selectionForName != null)
                return selectionForName;

            return project.GlobalSelection();
        }

        public static ProjectSelection<AspNetCoreProjectSettings> GetSelection(this AspNetCoreProject project, IView view)
        {
            // Sales.Order
            var selectionForFullName = project.Selections.FirstOrDefault(item => item.Pattern == view.FullName);

            if (selectionForFullName != null)
                return selectionForFullName;

            // Sales.*
            var selectionForSchema = project.Selections.FirstOrDefault(item => item.Pattern == string.Format("{0}.*", view.Schema));

            if (selectionForSchema != null)
                return selectionForSchema;

            // *.Order
            var selectionForName = project.Selections.FirstOrDefault(item => item.Pattern == string.Format("*.{0}", view.Name));

            if (selectionForName != null)
                return selectionForName;

            return project.GlobalSelection();
        }

        internal static void ScaffoldResponses(this AspNetCoreProject project)
        {
            var globalSelection = project.GlobalSelection();

            CSharpInterfaceBuilder.CreateFiles(project.OutputDirectory, project.AspNetCoreProjectNamespaces.Responses, globalSelection.Settings.ForceOverwrite,
                project.GetResponseInterfaceDefinition(),
                project.GetSingleResponseInterfaceDefinition(),
                project.GetListResponseInterfaceDefinition(),
                project.GetPagedResponseInterfaceDefinition()
            );

            CSharpClassBuilder.CreateFiles(project.OutputDirectory, project.AspNetCoreProjectNamespaces.Responses, globalSelection.Settings.ForceOverwrite,
                project.GetResponseClassDefinition(),
                project.GetSingleResponseClassDefinition(),
                project.GetListResponseClassDefinition(),
                project.GetPagedResponseClassDefinition()
            );
        }

        internal static void ScaffoldResponsesExtensions(this AspNetCoreProject project)
        {
            CSharpClassBuilder.CreateFiles(project.OutputDirectory, project.AspNetCoreProjectNamespaces.Responses, project.GlobalSelection().Settings.ForceOverwrite, project.GetResponsesExtensionsClassDefinition());
        }

        internal static void ScaffoldRequests(this AspNetCoreProject project)
        {
            foreach (var table in project.Database.Tables)
            {
                var classDefinition = project.GetRequestClassDefinition(table);

                CSharpClassBuilder.CreateFiles(project.OutputDirectory, project.AspNetCoreProjectNamespaces.Requests, project.GlobalSelection().Settings.ForceOverwrite, classDefinition);
            }
        }

        internal static void ScaffoldRequestsExtensions(this AspNetCoreProject project)
        {
            foreach (var table in project.Database.Tables)
            {
                var classDefinition = project.GetRequestExtensionsClassDefinition(table);

                CSharpClassBuilder.CreateFiles(project.OutputDirectory, project.AspNetCoreProjectNamespaces.Requests, project.GlobalSelection().Settings.ForceOverwrite, classDefinition);
            }
        }

        private static void ScaffoldReadMe(this AspNetCoreProject project)
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

        public static AspNetCoreProject ScaffoldAspNetCore(this AspNetCoreProject aspNetCoreProject)
        {
            aspNetCoreProject.ScaffoldResponses();
            aspNetCoreProject.ScaffoldResponsesExtensions();
            aspNetCoreProject.ScaffoldRequests();
            aspNetCoreProject.ScaffoldRequestsExtensions();
            aspNetCoreProject.ScaffoldReadMe();

            foreach (var feature in aspNetCoreProject.Features)
            {
                CSharpClassBuilder.CreateFiles(aspNetCoreProject.OutputDirectory, aspNetCoreProject.AspNetCoreProjectNamespaces.Controllers, aspNetCoreProject.GlobalSelection().Settings.ForceOverwrite, feature.GetControllerClassDefinition());
            }

            return aspNetCoreProject;
        }
    }
}
