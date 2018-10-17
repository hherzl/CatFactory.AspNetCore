using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.EntityFrameworkCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions.Extensions
{
    public static class RequestModelExtensionsClassBuilder
    {
        public static RequestModelExtensionsClassDefinition GetRequestModelExtensionsClassDefinition(this AspNetCoreProject project)
        {
            var classDefinition = new RequestModelExtensionsClassDefinition
            {
                Namespaces =
                {
                    "System",
                    project.GetEntityLayerNamespace()
                },
                Namespace = project.GetRequestModelsNamespace(),
                IsStatic = true,
                Name = "Extensions"
            };

            foreach (var table in project.Database.Tables)
            {
                if (!table.HasDefaultSchema())
                    classDefinition.Namespaces.AddUnique(project.GetEntityLayerNamespace(table.Schema));

                classDefinition.Methods.Add(GetToEntityMethod(project, table));
                classDefinition.Methods.Add(GetToRequestModelMethod(project, table));
            }

            return classDefinition;
        }

        private static MethodDefinition GetToEntityMethod(AspNetCoreProject project, ITable table)
        {
            var lines = new List<ILine>
            {
                new CodeLine("return new {0}", table.GetEntityName()),
                new CodeLine("{")
            };

            var selection = project.GetSelection(table);

            var columns = table.Columns.Where(item => item.Name != selection.Settings.ConcurrencyToken).ToList();

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                lines.Add(new CodeLine(1, "{0} = requestModel.{0}{1}", column.GetPropertyName(), i < columns.Count - 1 ? "," : string.Empty));
            }

            lines.Add(new CodeLine("};"));

            return new MethodDefinition(table.GetEntityName(), "ToEntity", new ParameterDefinition(table.GetRequestModelName(), "requestModel"))
            {
                IsStatic = true,
                IsExtension = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetToRequestModelMethod(AspNetCoreProject project, ITable table)
        {
            var lines = new List<ILine>
            {
                new CodeLine("return new {0}", table.GetRequestModelName()),
                new CodeLine("{")
            };

            var selection = project.GetSelection(table);

            var columns = table.Columns.Where(item => item.Name != selection.Settings.ConcurrencyToken).ToList();

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                lines.Add(new CodeLine(1, "{0} = entity.{0}{1}", column.GetPropertyName(), i < columns.Count - 1 ? "," : string.Empty));
            }

            lines.Add(new CodeLine("};"));

            return new MethodDefinition(table.GetRequestModelName(), "ToRequestModel", new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsStatic = true,
                IsExtension = true,
                Lines = lines
            };
        }
    }
}
