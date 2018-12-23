using System.Linq;
using CatFactory.NetCore;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.AspNetCore.Definitions.Extensions
{
    public static class RequestClassBuilder
    {
        public static RequestClassDefinition GetRequestClassDefinition(this AspNetCoreProject project, ITable table)
        {
            var definition = new RequestClassDefinition
            {
                Namespaces =
                {
                    "System",
                    "System.ComponentModel.DataAnnotations",
                    project.Database.HasDefaultSchema(table) ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(table.Schema)
                },
                Namespace = project.GetRequestsNamespace(),
                Name = table.GetRequestName()
            };

            var selection = project.GetSelection(table);

            foreach (var column in table.Columns.Where(item => selection.Settings.ConcurrencyToken != item.Name).ToList())
            {
                var property = new PropertyDefinition(project.Database.ResolveDatabaseType(column), column.GetPropertyName());

                if (table.PrimaryKey?.Key.Count > 0 && table.PrimaryKey?.Key.First() == column.Name)
                    property.Attributes.Add(new MetadataAttribute("Key"));

                if (!column.Nullable && table.PrimaryKey?.Key.Count > 0 && table.PrimaryKey?.Key.First() != column.Name)
                    property.Attributes.Add(new MetadataAttribute("Required"));

                if (project.Database.ColumnIsString(column) && column.Length > 0)
                    property.Attributes.Add(new MetadataAttribute("StringLength", column.Length.ToString()));

                definition.Properties.Add(property);
            }

            definition.SimplifyDataTypes();

            return definition;
        }
    }
}
