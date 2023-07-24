using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.EntityFrameworkCore;
using CatFactory.NetCore;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.AspNetCore.Definitions.Extensions
{
    public static class RequestClassBuilder
    {
        public static RequestClassDefinition GetGetRequestClassDefinition(this AspNetCoreProject project, ITable table)
        {
            var definition = new RequestClassDefinition
            {
                Namespaces =
                {
                    "System"
                },
                Namespace = project.GetRequestsNamespace(),
                AccessModifier = AccessModifier.Public,
                Name = project.GetGetRequestName(table)
            };

            //definition.Properties.Add(new PropertyDefinition { AccessModifier = AccessModifier.Public, Type = "int?", Name = "PageSize", IsAutomatic = true });
            //definition.Properties.Add(new PropertyDefinition { AccessModifier = AccessModifier.Public, Type = "int?", Name = "PageNumber", IsAutomatic = true });

            definition.Properties.Add(new PropertyDefinition(AccessModifier.Public, "int?", "PageSize") { IsAutomatic = true });
            definition.Properties.Add(new PropertyDefinition(AccessModifier.Public, "int?", "PageNumber") { IsAutomatic = true });

            definition.Constructors.Add(new ClassConstructorDefinition
            {
                AccessModifier = AccessModifier.Public,
                Lines =
                {
                    new CodeLine("PageSize = 10;"),
                    new CodeLine("PageNumber = 1;")
                }
            });

            foreach (var foreignKey in table.ForeignKeys)
            {
                var parentTable = project.Database.FindTable(foreignKey.References);

                if (parentTable == null)
                    continue;

                if (parentTable.PrimaryKey?.Key.Count == 1)
                {
                    var column = (Column)table.GetColumnsFromConstraint(foreignKey).First();

                    definition.AddAutomaticProperty(AccessModifier.Public, project.Database.ResolveDatabaseType(column), project.GetPropertyName(table, column));
                }
                else
                {
                    // todo: add logic for multiple columns in key
                }
            }

            definition.SimplifyDataTypes();

            return definition;
        }

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

            var exclusions = new List<string>();

            if (selection.Settings.HasConcurrencyToken)
                exclusions.Add(selection.Settings.ConcurrencyToken);

            if (selection.Settings.AuditEntity != null)
            {
                var auditEntity = selection.Settings.AuditEntity;
                exclusions.Add(auditEntity.CreationUserColumnName);
                exclusions.Add(auditEntity.CreationDateTimeColumnName);
                exclusions.Add(auditEntity.LastUpdateUserColumnName);
                exclusions.Add(auditEntity.LastUpdateDateTimeColumnName);
            }

            if (table.Identity != null)
                exclusions.Add(table.Identity.Name);

            var aspNetCoreSelection = project.GetSelection(table);

            foreach (var column in table.Columns.Where(item => !exclusions.Contains(item.Name)).ToList())
            {
                var property = new PropertyDefinition(AccessModifier.Public, project.Database.ResolveDatabaseType(column), project.GetPropertyName(table, column))
                {
                    IsAutomatic = true
                };

                if (aspNetCoreSelection.Settings.UseDataAnnotationsToValidateRequestModels)
                {
                    if (table.PrimaryKey?.Key.Count > 0 && table.PrimaryKey?.Key.First() == column.Name)
                        property.Attributes.Add(new MetadataAttribute("Key"));

                    if (!column.Nullable && table.PrimaryKey?.Key.Count > 0 && table.PrimaryKey?.Key.First() != column.Name)
                        property.Attributes.Add(new MetadataAttribute("Required"));

                    if (project.Database.ColumnIsString(column) && column.Length > 0)
                        property.Attributes.Add(new MetadataAttribute("StringLength", column.Length.ToString()));
                }

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

            var exclusions = new List<string>();

            if (selection.Settings.HasConcurrencyToken)
                exclusions.Add(selection.Settings.ConcurrencyToken);

            if (selection.Settings.AuditEntity != null)
            {
                var auditEntity = selection.Settings.AuditEntity;
                exclusions.Add(auditEntity.CreationUserColumnName);
                exclusions.Add(auditEntity.CreationDateTimeColumnName);
                exclusions.Add(auditEntity.LastUpdateUserColumnName);
                exclusions.Add(auditEntity.LastUpdateDateTimeColumnName);
            }

            if (table.Identity != null)
                exclusions.Add(table.Identity.Name);

            foreach (var column in table.Columns.Where(item => !exclusions.Contains(item.Name)).ToList())
            {
                var property = new PropertyDefinition(AccessModifier.Public, project.Database.ResolveDatabaseType(column), project.GetPropertyName(table, column))
                {
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
