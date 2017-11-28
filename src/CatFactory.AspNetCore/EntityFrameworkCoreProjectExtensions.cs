using System.Collections.Generic;
using System.Linq;
using CatFactory.AspNetCore.Definitions;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.DotNetCore;
using CatFactory.EfCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.AspNetCore
{
    public static class EntityFrameworkCoreProjectExtensions
    {
        public static string GetResponsesNamespace(this EntityFrameworkCoreProject project)
            => string.Format("{0}.{1}", project.Name, "Responses");

        public static string GetRequestModelsNamespace(this EntityFrameworkCoreProject project)
            => string.Format("{0}.{1}", project.Name, "RequestModels");

        internal static void GenerateResponses(this EntityFrameworkCoreProject project, AspNetCoreProjectSettings settings)
        {
            CSharpInterfaceBuilder.CreateFiles(settings.OutputDirectory, "Responses", project.Settings.ForceOverwrite,
                project.GetResponseInterfaceDefinition(),
                project.GetSingleResponseInterfaceDefinition(),
                project.GetListResponseInterfaceDefinition(),
                project.GetPagedResponseInterfaceDefinition()
            );

            CSharpClassBuilder.CreateFiles(settings.OutputDirectory, "Responses", project.Settings.ForceOverwrite,
                project.GetSingleResponseClassDefinition(),
                project.GetListResponseClassDefinition(),
                project.GetPagedResponseClassDefinition()
            );
        }

        internal static void GenerateResponsesExtensions(this EntityFrameworkCoreProject project, AspNetCoreProjectSettings settings)
        {
            var classDefinition = new CSharpClassDefinition
            {
                Namespaces = new List<string>
                {
                    "System",
                    "System.Net",
                    "Microsoft.AspNetCore.Mvc",
                    "Microsoft.Extensions.Logging"
                },
                Namespace = project.GetResponsesNamespace(),
                Name = "ResponsesExtensions",
                IsStatic = true
            };

            classDefinition.Methods.Add(new MethodDefinition("void", "SetError", new ParameterDefinition("IResponse", "response"), new ParameterDefinition("Exception", "ex"), new ParameterDefinition("ILogger", "logger"))
            {
                IsStatic = true,
                IsExtension = true,
                Lines = new List<ILine>
                {
                    new CodeLine("response.DidError = true;"),
                    new CodeLine("response.ErrorMessage = ex.Message;"),
                    new CodeLine(),
                    new CodeLine("logger?.LogError(ex.ToString());")
                }
            });

            classDefinition.Methods.Add(new MethodDefinition("IActionResult ", "ToHttpResponse", new ParameterDefinition("IResponse", "response"))
            {
                IsStatic = true,
                IsExtension = true,
                Lines = new List<ILine>
                {
                    new CodeLine("var status = HttpStatusCode.OK;"),
                    new CodeLine(),
                    new CodeLine("return new ObjectResult(response) { StatusCode = (Int32)status };")
                }
            });

            CSharpClassBuilder.CreateFiles(settings.OutputDirectory, "Responses", project.Settings.ForceOverwrite, classDefinition);
        }

        internal static void GenerateRequestModels(this EntityFrameworkCoreProject project, AspNetCoreProjectSettings settings)
        {
            foreach (var table in project.Database.Tables)
            {
                var classDefinition = new CSharpClassDefinition
                {
                    Namespaces = new List<string>
                    {
                        "System",
                        "System.ComponentModel.DataAnnotations"
                    },
                    Namespace = project.GetRequestModelsNamespace(),
                    Name = table.GetRequestModelName()
                };

                foreach (var column in table.Columns.Where(item => project.Settings.ConcurrencyToken != item.Name).ToList())
                {
                    var property = new PropertyDefinition(column.GetClrType(), column.GetPropertyName());

                    if (table.PrimaryKey?.Key.Count > 0 && table.PrimaryKey?.Key.First() == column.Name)
                    {
                        property.Attributes.Add(new MetadataAttribute("Key"));
                    }

                    if (!column.Nullable && table.PrimaryKey?.Key.Count > 0 && table.PrimaryKey?.Key.First() != column.Name)
                    {
                        property.Attributes.Add(new MetadataAttribute("Required"));
                    }

                    if (column.IsString() && column.Length > 0)
                    {
                        property.Attributes.Add(new MetadataAttribute("StringLength", column.Length.ToString()));
                    }

                    classDefinition.Properties.Add(property);
                }

                CSharpClassBuilder.CreateFiles(settings.OutputDirectory, "RequestModels", project.Settings.ForceOverwrite, classDefinition);
            }
        }

        private static MethodDefinition GetToEntityMethod(EntityFrameworkCoreProject project, ITable table)
        {
            var lines = new List<ILine>();

            lines.Add(new CodeLine("return new {0}", table.GetEntityName()));
            lines.Add(new CodeLine("{"));

            var columns = table.Columns.Where(item => item.Name != project.Settings.ConcurrencyToken).ToList();

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

        private static MethodDefinition GetToViewModelMethod(EntityFrameworkCoreProject project, ITable table)
        {
            var lines = new List<ILine>();

            lines.Add(new CodeLine("return new {0}", table.GetRequestModelName()));
            lines.Add(new CodeLine("{"));

            var columns = table.Columns.Where(item => item.Name != project.Settings.ConcurrencyToken).ToList();

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

        internal static void GenerateRequestModelsExtensions(this EntityFrameworkCoreProject project, AspNetCoreProjectSettings settings)
        {
            var classDefinition = new CSharpClassDefinition
            {
                Namespaces = new List<string>
                {
                    "System"
                },
                Namespace = project.GetRequestModelsNamespace(),
                Name = "Extensions",
                IsStatic = true
            };

            foreach (var table in project.Database.Tables)
            {
                if (table.HasDefaultSchema())
                {
                    classDefinition.Namespaces.AddUnique(project.GetEntityLayerNamespace());
                }
                else
                {
                    classDefinition.Namespaces.AddUnique(project.GetEntityLayerNamespace(table.Schema));
                }

                classDefinition.Methods.Add(GetToEntityMethod(project, table));
                classDefinition.Methods.Add(GetToViewModelMethod(project, table));
            }

            CSharpClassBuilder.CreateFiles(settings.OutputDirectory, "RequestModels", project.Settings.ForceOverwrite, classDefinition);
        }

        public static EntityFrameworkCoreProject ScaffoldAspNetCoreProject(this EntityFrameworkCoreProject project, AspNetCoreProjectSettings settings)
        {
            project.GenerateResponses(settings);
            project.GenerateResponsesExtensions(settings);
            project.GenerateRequestModels(settings);
            project.GenerateRequestModelsExtensions(settings);

            foreach (var feature in project.Features)
            {
                CSharpClassBuilder.CreateFiles(settings.OutputDirectory, "Controllers", feature.GetEntityFrameworkCoreProject().Settings.ForceOverwrite, feature.GetControllerClassDefinition());
            }

            return project;
        }
    }
}
