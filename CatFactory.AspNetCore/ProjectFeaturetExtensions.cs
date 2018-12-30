using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory.Scaffolding;
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
            var settings = projectFeature.GetAspNetCoreProject().GetSelection(table).Settings;

            foreach (var column in table.Columns)
            {
                if (table.PrimaryKey != null && table.PrimaryKey.Key.Contains(column.Name))
                    continue;

                if (settings.AuditEntity != null && settings.AuditEntity.Names.Contains(column.Name))
                    continue;

                if (!string.IsNullOrEmpty(settings.ConcurrencyToken) && string.Compare(settings.ConcurrencyToken, column.Name) == 0)
                    continue;

                yield return column;
            }
        }
    }
}
