using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.EntityFrameworkCore;
using CatFactory.NetCore;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.AspNetCore.Definitions.Extensions
{
    public static class ValidatorClassBuilder
    {
        public static ValidatorClassDefinition GetValidatorClassDefinition(this AspNetCoreProject project, ITable table)
        {
            var definition = new ValidatorClassDefinition
            {
                Namespaces =
                {
                    "FluentValidation"
                },
                Namespace = project.GetValidatorsNamespace(),
                AccessModifier = AccessModifier.Public,
                Name = project.GetValidatorName(table),
                BaseClass = string.Format("AbstractValidator<{0}>", project.GetPostRequestName(table))
            };

            var lines = new List<ILine>();

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

            var columns = table.Columns.Where(item => !exclusions.Contains(item.Name)).ToList();

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                var property = new PropertyDefinition
                {
                    AccessModifier = AccessModifier.Public,
                    Type = project.Database.ResolveDatabaseType(column),
                    Name = project.GetPropertyName(table, column)
                };

                var configs = new List<string>
                {
                    string.Format("RuleFor(m => m.{0})", property.Name)
                };

                if (!column.Nullable)
                    configs.Add("NotNull()");

                if (project.Database.ColumnIsString(column))
                    configs.Add(string.Format("Length({0}, {1})", 0, column.Length));

                if (configs.Count == 1)
                    continue;

                lines.Add(new CodeLine("{0};", string.Join(".", configs)));

                if (i < columns.Count - 1)
                    lines.Add(new EmptyLine());
            }

            definition.Constructors.Add(new ClassConstructorDefinition
            {
                AccessModifier = AccessModifier.Public,
                Lines = lines
            });

            return definition;
        }
    }
}
