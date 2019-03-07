using CatFactory.CodeFactory.Scaffolding;
using CatFactory.EntityFrameworkCore;
using CatFactory.ObjectRelationalMapping.Actions;

namespace CatFactory.AspNetCore
{
    public static class EntityFrameworkCoreProjectExtensions
    {
        public static AspNetCoreProject CreateAspNetCoreProject(this EntityFrameworkCoreProject entityFrameworkCoreProject, string name, string outputDirectory)
        {
            var aspNetCoreProject = new AspNetCoreProject
            {
                Name = name,
                OutputDirectory = outputDirectory,
                Database = entityFrameworkCoreProject.Database,
                EntityFrameworkCoreProject = entityFrameworkCoreProject
            };

            aspNetCoreProject.GlobalSelection(settings =>
            {
                settings.Actions.Add(new ReadAllAction());
                settings.Actions.Add(new ReadByKeyAction());
                settings.Actions.Add(new ReadByUniqueAction());
                settings.Actions.Add(new AddEntityAction());
                settings.Actions.Add(new UpdateEntityAction());
                settings.Actions.Add(new RemoveEntityAction());
            });

            aspNetCoreProject.BuildFeatures();

            foreach (var selection in entityFrameworkCoreProject.Selections)
            {
                aspNetCoreProject.Selections.Add(new ProjectSelection<AspNetCoreProjectSettings>
                {
                    Pattern = selection.Pattern,
                    Settings = new AspNetCoreProjectSettings
                    {
                        ForceOverwrite = selection.Settings.ForceOverwrite,
                        UseLogger = true
                    }
                });
            }

            return aspNetCoreProject;
        }
    }
}
