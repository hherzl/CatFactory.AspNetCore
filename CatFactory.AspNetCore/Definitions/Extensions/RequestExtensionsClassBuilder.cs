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
        public static RequestModelExtensionsClassDefinition GetRequestExtensionsClassDefinition(this AspNetCoreProject project, ITable table)
        {
            var definition = new RequestModelExtensionsClassDefinition
            {
                Namespaces =
                {
                    "System",
                    project.GetEntityLayerNamespace()
                },
                Namespace = project.GetRequestsNamespace(),
                AccessModifier = AccessModifier.Public,
                IsStatic = true,
                Name = string.Format("{0}RequestExtensions", project.EntityFrameworkCoreProject.GetEntityName(table))
            };

            if (!project.Database.HasDefaultSchema(table))
                definition.Namespaces.AddUnique(project.GetEntityLayerNamespace(table.Schema));

            definition.Methods.Add(GetToEntityMethod(project, table));
            definition.Methods.Add(GetToRequestModelMethod(project, table));

            return definition;
        }

        private static MethodDefinition GetToEntityMethod(AspNetCoreProject project, ITable table)
        {
            var lines = new List<ILine>
            {
                new CodeLine("return new {0}", project.EntityFrameworkCoreProject.GetEntityName(table)),
                new CodeLine("{")
            };

            var selection = project.EntityFrameworkCoreProject.GetSelection(table);

            var columns = table.Columns.Where(item => item.Name != selection.Settings.ConcurrencyToken).ToList();

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                lines.Add(new CodeLine(1, "{0} = request.{0}{1}", column.GetPropertyName(), i < columns.Count - 1 ? "," : string.Empty));
            }

            lines.Add(new CodeLine("};"));

            return new MethodDefinition(project.EntityFrameworkCoreProject.GetEntityName(table), "ToEntity", new ParameterDefinition(project.GetRequestName(table), "request"))
            {
                AccessModifier = AccessModifier.Public,
                IsStatic = true,
                IsExtension = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetToRequestModelMethod(AspNetCoreProject project, ITable table)
        {
            var lines = new List<ILine>
            {
                new CodeLine("return new {0}", project.GetRequestName(table)),
                new CodeLine("{")
            };

            var selection = project.EntityFrameworkCoreProject.GetSelection(table);

            var columns = table.Columns.Where(item => item.Name != selection.Settings.ConcurrencyToken).ToList();

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                lines.Add(new CodeLine(1, "{0} = entity.{0}{1}", column.GetPropertyName(), i < columns.Count - 1 ? "," : string.Empty));
            }

            lines.Add(new CodeLine("};"));

            return new MethodDefinition(project.GetRequestName(table), "ToRequest", new ParameterDefinition(project.EntityFrameworkCoreProject.GetEntityName(table), "entity"))
            {
                AccessModifier = AccessModifier.Public,
                IsStatic = true,
                IsExtension = true,
                Lines = lines
            };
        }
    }
}
