using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CatFactory.AspNetCore.Definitions.Extensions;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.EntityFrameworkCore;
using CatFactory.Mapping;
using CatFactory.NetCore;
using CatFactory.NetCore.CodeFactory;

namespace CatFactory.AspNetCore
{
    public static class AspNetCoreProjectExtensions
    {
        private static ICodeNamingConvention namingConvention;

        static AspNetCoreProjectExtensions()
        {
            namingConvention = new DotNetNamingConvention();
        }

        public static string GetResponsesNamespace(this AspNetCoreProject project)
            => string.Format("{0}.{1}", project.Name, "Responses");

        public static string GetRequestModelsNamespace(this AspNetCoreProject project)
            => string.Format("{0}.{1}", project.Name, "RequestModels");

        public static string GetEntityLayerNamespace(this AspNetCoreProject project)
            => string.Join(".", namingConvention.GetNamespace(project.ReferencedProjectName), namingConvention.GetNamespace(project.Namespaces.EntityLayer));

        public static string GetEntityLayerNamespace(this AspNetCoreProject project, string ns)
            => string.IsNullOrEmpty(ns) ? GetEntityLayerNamespace(project) : string.Join(".", project.ReferencedProjectName, project.Namespaces.EntityLayer, ns);

        public static string GetDataLayerContractsNamespace(this AspNetCoreProject project)
            => string.Join(".", namingConvention.GetNamespace(project.ReferencedProjectName), project.Namespaces.DataLayer, project.Namespaces.Contracts);

        public static string GetDataLayerDataContractsNamespace(this AspNetCoreProject project)
            => string.Join(".", namingConvention.GetNamespace(project.ReferencedProjectName), project.Namespaces.DataLayer, project.Namespaces.DataContracts);

        public static string GetDataLayerRepositoriesNamespace(this AspNetCoreProject project)
            => string.Join(".", namingConvention.GetNamespace(project.ReferencedProjectName), project.Namespaces.DataLayer, project.Namespaces.Repositories);

        public static string GetInterfaceRepositoryName(this ProjectFeature<AspNetCoreProjectSettings> projectFeature)
            => namingConvention.GetInterfaceName(string.Format("{0}Repository", projectFeature.Name));

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

        public static AspNetCoreProject Select(this AspNetCoreProject project, string pattern, Action<AspNetCoreProjectSettings> action = null)
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
                        UseLogger = globalSettings.UseLogger,
                        ConcurrencyToken = globalSettings.ConcurrencyToken,
                        AuditEntity = new AuditEntity
                        {
                            CreationUserColumnName = globalSettings.AuditEntity.CreationUserColumnName,
                            CreationDateTimeColumnName = globalSettings.AuditEntity.CreationDateTimeColumnName,
                            LastUpdateUserColumnName = globalSettings.AuditEntity.LastUpdateUserColumnName,
                            LastUpdateDateTimeColumnName = globalSettings.AuditEntity.LastUpdateDateTimeColumnName
                        },
                        EntitiesWithDataContracts = globalSettings.EntitiesWithDataContracts
                    }
                };

                project.Selections.Add(selection);
            }

            action?.Invoke(selection.Settings);

            return project;
        }

        internal static void ScaffoldResponses(this AspNetCoreProject project)
        {
            var globalSelection = project.GlobalSelection();

            CSharpInterfaceBuilder.CreateFiles(project.OutputDirectory, "Responses", globalSelection.Settings.ForceOverwrite,
                project.GetResponseInterfaceDefinition(),
                project.GetSingleResponseInterfaceDefinition(),
                project.GetListResponseInterfaceDefinition(),
                project.GetPagedResponseInterfaceDefinition()
            );

            CSharpClassBuilder.CreateFiles(project.OutputDirectory, "Responses", globalSelection.Settings.ForceOverwrite,
                project.GetSingleResponseClassDefinition(),
                project.GetListResponseClassDefinition(),
                project.GetPagedResponseClassDefinition()
            );
        }

        internal static void ScaffoldResponsesExtensions(this AspNetCoreProject project)
        {
            CSharpClassBuilder.CreateFiles(project.OutputDirectory, "Responses", project.GlobalSelection().Settings.ForceOverwrite, project.GetResponsesExtensionsClassDefinition());
        }

        internal static void ScaffoldRequestModels(this AspNetCoreProject project)
        {
            foreach (var table in project.Database.Tables)
            {
                var classDefinition = project.GetResponsesExtensionsClassDefinition(table);

                CSharpClassBuilder.CreateFiles(project.OutputDirectory, "RequestModels", project.GlobalSelection().Settings.ForceOverwrite, classDefinition);
            }
        }

        internal static void ScaffoldRequestModelsExtensions(this AspNetCoreProject project)
        {
            var classDefinition = project.GetRequestModelExtensionsClassDefinition();

            CSharpClassBuilder.CreateFiles(project.OutputDirectory, "RequestModels", project.GlobalSelection().Settings.ForceOverwrite, classDefinition);
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
                string.Format(" services.AddDbContext<{0}>(options => options.UseSqlServer(Configuration[\"ConnectionString\"]));", project.Database.GetDbContextName()),

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

        public static AspNetCoreProject CreateAspNetCoreProject(this EntityFrameworkCoreProject entityFrameworkProject, string name, string outputDirectory, Database database)
        {
            var aspNetCoreProject = new AspNetCoreProject
            {
                Name = name,
                OutputDirectory = outputDirectory,
                Database = database,
                ReferencedProjectName = entityFrameworkProject.Name
            };

            aspNetCoreProject.BuildFeatures();

            foreach (var selection in entityFrameworkProject.Selections)
            {
                aspNetCoreProject.Selections.Add(new ProjectSelection<AspNetCoreProjectSettings>
                {
                    Pattern = selection.Pattern,
                    Settings = new AspNetCoreProjectSettings
                    {
                        ForceOverwrite = selection.Settings.ForceOverwrite,
                        UseLogger = true,
                        ConcurrencyToken = selection.Settings.ConcurrencyToken,
                        AuditEntity = selection.Settings.AuditEntity == null ? null : new AuditEntity
                        {
                            CreationUserColumnName = selection.Settings.AuditEntity.CreationUserColumnName,
                            CreationDateTimeColumnName = selection.Settings.AuditEntity.CreationDateTimeColumnName,
                            LastUpdateUserColumnName = selection.Settings.AuditEntity.LastUpdateUserColumnName,
                            LastUpdateDateTimeColumnName = selection.Settings.AuditEntity.LastUpdateDateTimeColumnName
                        },
                        EntitiesWithDataContracts = selection.Settings.EntitiesWithDataContracts
                    }
                });
            }

            return aspNetCoreProject;
        }

        public static AspNetCoreProject ScaffoldAspNetCore(this AspNetCoreProject aspNetCoreProject)
        {
            aspNetCoreProject.ScaffoldResponses();
            aspNetCoreProject.ScaffoldResponsesExtensions();
            aspNetCoreProject.ScaffoldRequestModels();
            aspNetCoreProject.ScaffoldRequestModelsExtensions();
            aspNetCoreProject.ScaffoldReadMe();

            foreach (var feature in aspNetCoreProject.Features)
            {
                CSharpClassBuilder.CreateFiles(aspNetCoreProject.OutputDirectory, "Controllers", aspNetCoreProject.GlobalSelection().Settings.ForceOverwrite, feature.GetControllerClassDefinition());
            }

            return aspNetCoreProject;
        }
    }
}
