using System.Collections.Generic;
using System.Linq;
using CatFactory.EntityFrameworkCore;
using CatFactory.NetCore;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.AspNetCore.Definitions.Extensions
{
    public static class RequestClassBuilder
    {
        public static RequestClassDefinition GetPostRequestClassDefinition(this AspNetCoreProject project, ITable table)
        {
            var definition = new RequestClassDefinition
            {
                Namespaces =
                {
                    "System",
                    "System.ComponentModel.DataAnnotations"
                },
                Namespace = project.GetRequestsNamespace(),
                AccessModifier = AccessModifier.Public,
                Name = project.GetPostRequestName(table)
            };

            var selection = project.EntityFrameworkCoreProject.GetSelection(table);

            var exclusions = new List<string>
            {
                selection.Settings.ConcurrencyToken,
                selection.Settings.AuditEntity.CreationUserColumnName,
                selection.Settings.AuditEntity.CreationDateTimeColumnName,
                selection.Settings.AuditEntity.LastUpdateUserColumnName,
                selection.Settings.AuditEntity.LastUpdateDateTimeColumnName
            };

            if (table.Identity != null)
                exclusions.Add(table.Identity.Name);

            foreach (var column in table.Columns.Where(item => !exclusions.Contains(item.Name)).ToList())
            {
                var property = new PropertyDefinition
                {
                    AccessModifier = AccessModifier.Public,
                    Type = project.Database.ResolveDatabaseType(column),
                    Name = project.GetPropertyName(table, column),
                    IsAutomatic = true
                };

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

        public static RequestClassDefinition GetPutRequestClassDefinition(this AspNetCoreProject project, ITable table)
        {
            var definition = new RequestClassDefinition
            {
                Namespaces =
                {
                    "System",
                    "System.ComponentModel.DataAnnotations"
                },
                Namespace = project.GetRequestsNamespace(),
                AccessModifier = AccessModifier.Public,
                Name = project.GetPutRequestName(table)
            };

            var selection = project.EntityFrameworkCoreProject.GetSelection(table);

            var exclusions = new List<string>
            {
                selection.Settings.ConcurrencyToken,
                selection.Settings.AuditEntity.CreationUserColumnName,
                selection.Settings.AuditEntity.CreationDateTimeColumnName,
                selection.Settings.AuditEntity.LastUpdateUserColumnName,
                selection.Settings.AuditEntity.LastUpdateDateTimeColumnName
            };

            if (table.Identity != null)
                exclusions.Add(table.Identity.Name);

            foreach (var column in table.Columns.Where(item => !exclusions.Contains(item.Name)).ToList())
            {
                var property = new PropertyDefinition
                {
                    AccessModifier = AccessModifier.Public,
                    Type = project.Database.ResolveDatabaseType(column),
                    Name = project.GetPropertyName(table, column),
                    IsAutomatic = true
                };

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
