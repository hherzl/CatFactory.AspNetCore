using CatFactory.AspNetCore.Definitions;
using CatFactory.DotNetCore;
using CatFactory.EfCore;

namespace CatFactory.AspNetCore
{
    public static class EntityFrameworkCoreProjectExtensions
    {
        public static string GetResponsesNamespace(this AspNetCoreProjectSettings settings)
            => string.Format("{0}.{1}", settings.ProjectName, "Responses");

        public static string GetRequestModelsNamespace(this AspNetCoreProjectSettings settings)
            => string.Format("{0}.{1}", settings.ProjectName, "RequestModels");

        internal static void ScaffoldResponses(this EntityFrameworkCoreProject project, AspNetCoreProjectSettings settings)
        {
            CSharpInterfaceBuilder.CreateFiles(settings.OutputDirectory, "Responses", project.GlobalSelection().Settings.ForceOverwrite,
                project.GetResponseInterfaceDefinition(settings),
                project.GetSingleResponseInterfaceDefinition(settings),
                project.GetListResponseInterfaceDefinition(settings),
                settings.GetPagedResponseInterfaceDefinition()
            );

            CSharpClassBuilder.CreateFiles(settings.OutputDirectory, "Responses", project.GlobalSelection().Settings.ForceOverwrite,
                project.GetSingleResponseClassDefinition(settings),
                settings.GetListResponseClassDefinition(),
                project.GetPagedResponseClassDefinition(settings)
            );
        }

        internal static void ScaffoldResponsesExtensions(this EntityFrameworkCoreProject project, AspNetCoreProjectSettings settings)
        {
            CSharpClassBuilder.CreateFiles(settings.OutputDirectory, "Responses", project.GlobalSelection().Settings.ForceOverwrite, project.GetResponsesExtensionsClassDefinition(settings));
        }

        internal static void ScaffoldRequestModels(this EntityFrameworkCoreProject project, AspNetCoreProjectSettings settings)
        {
            foreach (var table in project.Database.Tables)
            {
                var classDefinition = project.GetResponsesExtensionsClassDefinition(table, settings);

                CSharpClassBuilder.CreateFiles(settings.OutputDirectory, "RequestModels", project.GlobalSelection().Settings.ForceOverwrite, classDefinition);
            }
        }

        internal static void ScaffoldRequestModelsExtensions(this EntityFrameworkCoreProject project, AspNetCoreProjectSettings settings)
        {
            var classDefinition = project.GetRequestModelExtensionsClassDefinition(settings);

            CSharpClassBuilder.CreateFiles(settings.OutputDirectory, "RequestModels", project.GlobalSelection().Settings.ForceOverwrite, classDefinition);
        }

        public static EntityFrameworkCoreProject ScaffoldAspNetCore(this EntityFrameworkCoreProject project, AspNetCoreProjectSettings settings)
        {
            project.ScaffoldResponses(settings);
            project.ScaffoldResponsesExtensions(settings);
            project.ScaffoldRequestModels(settings);
            project.ScaffoldRequestModelsExtensions(settings);

            foreach (var feature in project.Features)
            {
                CSharpClassBuilder.CreateFiles(settings.OutputDirectory, "Controllers", project.GlobalSelection().Settings.ForceOverwrite, feature.GetControllerClassDefinition(settings));
            }

            return project;
        }
    }
}
