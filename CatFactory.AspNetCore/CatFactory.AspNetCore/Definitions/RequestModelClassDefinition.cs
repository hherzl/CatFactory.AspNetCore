using System.Collections.Generic;
using System.Linq;
using CatFactory.EntityFrameworkCore;
using CatFactory.Mapping;
using CatFactory.NetCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public static class RequestModelClassDefinition
    {
        public static CSharpClassDefinition GetResponsesExtensionsClassDefinition(this AspNetCoreProject project, ITable table)
        {
            var classDefinition = new CSharpClassDefinition
            {
                Namespaces = new List<string>
                {
                    "System",
                    "System.ComponentModel.DataAnnotations",
                    table.HasDefaultSchema() ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(table.Schema)
                },
                Namespace = project.GetRequestModelsNamespace(),
                Name = table.GetRequestModelName()
            };

            var selection = project.GetSelection(table);

            foreach (var column in table.Columns.Where(item => selection.Settings.ConcurrencyToken != item.Name).ToList())
            {
                var property = new PropertyDefinition(EntityFrameworkCore.DatabaseExtensions.ResolveType(project.Database, column), column.GetPropertyName());

                if (table.PrimaryKey?.Key.Count > 0 && table.PrimaryKey?.Key.First() == column.Name)
                    property.Attributes.Add(new MetadataAttribute("Key"));

                if (!column.Nullable && table.PrimaryKey?.Key.Count > 0 && table.PrimaryKey?.Key.First() != column.Name)
                    property.Attributes.Add(new MetadataAttribute("Required"));

                if (project.Database.ColumnIsString(column) && column.Length > 0)
                    property.Attributes.Add(new MetadataAttribute("StringLength", column.Length.ToString()));

                classDefinition.Properties.Add(property);
            }

            return classDefinition;
        }
    }
}
