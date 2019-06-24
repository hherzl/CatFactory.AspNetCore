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
            //definition.Methods.Add(GetToRequestModelMethod(project, table));

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

            var exclusions = new List<string>
            {
                selection.Settings.ConcurrencyToken,
                selection.Settings.AuditEntity.CreationUserColumnName,
                selection.Settings.AuditEntity.CreationDateTimeColumnName,
                selection.Settings.AuditEntity.LastUpdateUserColumnName,
                selection.Settings.AuditEntity.LastUpdateDateTimeColumnName
            };

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

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                IsStatic = true,
                Type = project.EntityFrameworkCoreProject.GetEntityName(table),
                Name = "ToEntity",
                IsExtension = true,
                Parameters =
                {
                    new ParameterDefinition(project.GetPostRequestName(table), "request")
                },
                Lines = lines
            };
        }

        //private static MethodDefinition GetToRequestModelMethod(AspNetCoreProject project, ITable table)
        //{
        //    var lines = new List<ILine>
        //    {
        //        new CodeLine("return new {0}", project.GetPostRequestName(table)),
        //        new CodeLine("{")
        //    };

        //    var selection = project.EntityFrameworkCoreProject.GetSelection(table);

        //    var exclusions = new List<string>
        //    {
        //        selection.Settings.ConcurrencyToken,
        //        selection.Settings.AuditEntity.CreationUserColumnName,
        //        selection.Settings.AuditEntity.CreationDateTimeColumnName,
        //        selection.Settings.AuditEntity.LastUpdateUserColumnName,
        //        selection.Settings.AuditEntity.LastUpdateDateTimeColumnName
        //    };

        //    if (table.PrimaryKey != null)
        //        exclusions.Add(table.PrimaryKey.Key.First());

        //    if (table.Identity != null)
        //        exclusions.Add(table.Identity.Name);

        //    var columns = table.Columns.Where(item => !exclusions.Contains(item.Name)).ToList();

        //    for (var i = 0; i < columns.Count; i++)
        //    {
        //        var column = columns[i];

        //        lines.Add(new CodeLine(1, "{0} = entity.{0}{1}", project.GetPropertyName(table, column), i < columns.Count - 1 ? "," : string.Empty));
        //    }

        //    lines.Add(new CodeLine("};"));

        //    return new MethodDefinition
        //    {
        //        AccessModifier = AccessModifier.Public,
        //        IsStatic = true,
        //        Type = project.GetPostRequestName(table),
        //        Name = "ToRequest",
        //        IsExtension = true,
        //        Parameters =
        //        {
        //            new ParameterDefinition(project.EntityFrameworkCoreProject.GetEntityName(table), "entity")
        //        },
        //        Lines = lines
        //    };
        //}
    }
}
