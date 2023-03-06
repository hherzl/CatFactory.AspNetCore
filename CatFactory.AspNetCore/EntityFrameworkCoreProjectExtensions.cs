using CatFactory.CodeFactory.Scaffolding;
using CatFactory.EntityFrameworkCore;

namespace CatFactory.AspNetCore
{
    public static class EntityFrameworkCoreProjectExtensions
    {
        public static AspNetCoreProject CreateAspNetCore2xProject(this EntityFrameworkCoreProject entityFrameworkCoreProject, string name, string outputDirectory)
        {
            var aspNetCoreProject = new AspNetCoreProject
            {
                Name = name,
                OutputDirectory = outputDirectory,
                Database = entityFrameworkCoreProject.Database,
                EntityFrameworkCoreProject = entityFrameworkCoreProject,
                Version = AspNetCoreVersion.Version_2_0
            };

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

        public static AspNetCoreProject CreateAspNetCore3xProject(this EntityFrameworkCoreProject entityFrameworkCoreProject, string name, string outputDirectory)
        {
            var aspNetCoreProject = new AspNetCoreProject
            {
                Name = name,
                OutputDirectory = outputDirectory,
                Database = entityFrameworkCoreProject.Database,
                EntityFrameworkCoreProject = entityFrameworkCoreProject,
                Version = AspNetCoreVersion.Version_3_0
            };

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
