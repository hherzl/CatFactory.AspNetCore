using CatFactory.CodeFactory.Scaffolding;
using CatFactory.EntityFrameworkCore;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.AspNetCore
{
    public static class EntityFrameworkCoreProjectExtensions
    {
        public static AspNetCoreProject CreateAspNetCoreProject(this EntityFrameworkCoreProject entityFrameworkCoreProject, string name, string outputDirectory, Database database)
        {
            var aspNetCoreProject = new AspNetCoreProject
            {
                Name = name,
                OutputDirectory = outputDirectory,
                Database = database,
                EntityFrameworkCoreProject = entityFrameworkCoreProject
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
    }
}
