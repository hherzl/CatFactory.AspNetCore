using CatFactory.AspNetCore.Definitions;
using CatFactory.DotNetCore;
using CatFactory.EfCore;

namespace CatFactory.AspNetCore
{
    public static class EfCoreProjectExtensions
    {
        public static EfCoreProject GenerateAspNetCoreProject(this EfCoreProject project, AspNetCoreProjectSettings settings)
        {
            project.GenerateResponses(settings);
            project.GenerateResponsesExtensions(settings);
            project.GenerateViewModels(settings);

            foreach (var feature in project.Features)
            {
                var codeBuilder = new CSharpClassBuilder
                {
                    OutputDirectory = settings.OutputDirectory,
                    ObjectDefinition = new ControllerClassDefinition(feature)
                };

                codeBuilder.CreateFile(subdirectory: "Controllers");
            }

            return project;
        }
    }
}
