using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.EntityFrameworkCore;
using CatFactory.NetCore;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.AspNetCore.Definitions.Extensions
{
    public static class RequestExtensionsClassBuilder
    {
        public static RequestModelExtensionsClassDefinition GetRequestExtensionsClassDefinition(this AspNetCoreProject project, ITable table, bool isDomainDrivenDesign = true)
        {
            var efCoreProject = project.EntityFrameworkCoreProject;

            var definition = new RequestModelExtensionsClassDefinition
            {
                Namespaces =
                {
                    "System",
                    isDomainDrivenDesign ? efCoreProject.GetDomainModelsNamespace() : project.GetEntityLayerNamespace()
                },
                Namespace = project.GetRequestsNamespace(),
                AccessModifier = AccessModifier.Public,
                IsStatic = true,
                Name = string.Format("{0}RequestExtensions", efCoreProject.GetEntityName(table))
            };

            if (!project.Database.HasDefaultSchema(table))
            {
                if (isDomainDrivenDesign)
                    definition.Namespaces.AddUnique(efCoreProject.GetDomainModelsNamespace(table.Schema));
                else
                    definition.Namespaces.AddUnique(project.GetEntityLayerNamespace(table.Schema));
            }

            definition.Methods.Add(GetToEntityMethod(project, table));

            return definition;
        }

        private static MethodDefinition GetToEntityMethod(AspNetCoreProject project, ITable table)
        {
            var efCoreProject = project.EntityFrameworkCoreProject;
            var selection = efCoreProject.GetSelection(table);

            var lines = new List<ILine>
            {
                new CodeLine("return new {0}", efCoreProject.GetEntityName(table)),
                new CodeLine("{")
            };

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

            if (table.PrimaryKey != null)
                exclusions.Add(table.PrimaryKey.Key.First());

            if (table.Identity != null)
                exclusions.Add(table.Identity.Name);

            var columns = table.Columns.Where(item => !exclusions.Contains(item.Name)).ToList();

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                lines.Add(new CodeLine(1, "{0} = request.{0}{1}", project.GetPropertyName(table, column), i < columns.Count - 1 ? "," : string.Empty));
            }

            lines.Add(new CodeLine("};"));

            return new MethodDefinition(AccessModifier.Public, efCoreProject.GetEntityName(table), "ToEntity")
            {
                IsStatic = true,
                IsExtension = true,
                Parameters =
                {
                    new ParameterDefinition(project.GetPostRequestName(table), "request")
                },
                Lines = lines
            };
        }
    }
}
