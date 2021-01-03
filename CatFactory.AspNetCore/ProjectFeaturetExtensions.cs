using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.EntityFrameworkCore;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.AspNetCore
{
    public static class ProjectFeaturetExtensions
    {
        public static AspNetCoreProject GetAspNetCoreProject(this ProjectFeature<AspNetCoreProjectSettings> projectFeature)
            => projectFeature.Project as AspNetCoreProject;

        public static string GetControllerName(this ProjectFeature<AspNetCoreProjectSettings> projectFeature)
            => projectFeature.Project.CodeNamingConvention.GetClassName(string.Format("{0}{1}", projectFeature.Name, "Controller"));

        public static string GetInterfaceRepositoryName(this ProjectFeature<AspNetCoreProjectSettings> projectFeature)
            => projectFeature.Project.CodeNamingConvention.GetInterfaceName(string.Format("{0}Repository", projectFeature.Name));

        public static IEnumerable<Column> GetUpdateColumns(this ProjectFeature<AspNetCoreProjectSettings> projectFeature, ITable table)
        {
            var aspNetCoreProject = projectFeature.GetAspNetCoreProject();
            var efCoreProjectSettings = aspNetCoreProject.EntityFrameworkCoreProject.GetSelection(table).Settings;

            foreach (var column in table.Columns)
            {
                if (table.PrimaryKey != null && table.PrimaryKey.Key.Contains(column.Name))
                    continue;

                if (efCoreProjectSettings.AuditEntity != null && efCoreProjectSettings.AuditEntity.Names.Contains(column.Name))
                    continue;

                if (!string.IsNullOrEmpty(efCoreProjectSettings.ConcurrencyToken) && string.Compare(efCoreProjectSettings.ConcurrencyToken, column.Name) == 0)
                    continue;

                yield return column;
            }
        }
    }
}
