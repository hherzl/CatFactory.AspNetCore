using System.Collections.Generic;
using CatFactory.AspNetCore.Definitions;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.DotNetCore;
using CatFactory.EfCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.AspNetCore
{
    public static class EfCoreProjectExtensions
    {
        public static string GetResponsesNamespace(this EfCoreProject project)
            => string.Format("{0}.{1}", project.Name, "Responses");

        public static string GetViewModelsNamespace(this EfCoreProject project)
            => string.Format("{0}.{1}", project.Name, "ViewModels");

        internal static void GenerateResponses(this EfCoreProject project, AspNetCoreProjectSettings settings)
        {
            CSharpInterfaceBuilder.Create(
                settings.OutputDirectory,
                "Responses",
                project.Settings.ForceOverwrite,
                project.GetResponseInterfaceDefinition(),
                project.GetSingleResponseInterfaceDefinition(),
                project.GetListResponseInterfaceDefinition(),
                project.GetPagedResponseInterfaceDefinition()
            );

            CSharpClassBuilder.Create(
                settings.OutputDirectory,
                "Responses",
                project.Settings.ForceOverwrite,
                project.GetSingleResponseClassDefinition(),
                project.GetListResponseClassDefinition(),
                project.GetPagedResponseClassDefinition()
            );
        }

        internal static void GenerateResponsesExtensions(this EfCoreProject project, AspNetCoreProjectSettings settings)
        {
            var classDefinition = new CSharpClassDefinition
            {
                Namespaces = new List<string>()
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
                Lines = new List<ILine>()
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
                Lines = new List<ILine>()
                {
                    new CodeLine("var status = HttpStatusCode.OK;"),
                    new CodeLine(),
                    new CodeLine("return new ObjectResult(response) { StatusCode = (Int32)status };")
                }
            });

            CSharpClassBuilder.Create(settings.OutputDirectory, "Responses", project.Settings.ForceOverwrite, classDefinition);
        }

        internal static void GenerateViewModels(this EfCoreProject project, AspNetCoreProjectSettings settings)
        {
            var resolver = new ClrTypeResolver();

            foreach (var table in project.Database.Tables)
            {
                var classDefinition = new CSharpClassDefinition
                {
                    Namespaces = new List<string>()
                    {
                        "System"
                    },
                    Namespace = project.GetViewModelsNamespace(),
                    Name = table.GetViewModelName()
                };

                foreach (var column in table.Columns)
                {
                    classDefinition.Properties.Add(new PropertyDefinition(resolver.Resolve(column.Type), column.GetPropertyName()));
                }

                CSharpClassBuilder.Create(settings.OutputDirectory, "ViewModels", project.Settings.ForceOverwrite, classDefinition);
            }
        }

        private static MethodDefinition GetToEntityMethod(ITable table)
        {
            var lines = new List<ILine>();

            lines.Add(new CodeLine("return new {0}", table.GetEntityName()));
            lines.Add(new CodeLine("{"));

            for (var i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];

                lines.Add(new CodeLine(1, "{0} = viewModel.{0}{1}", column.GetPropertyName(), i < table.Columns.Count - 1 ? "," : string.Empty));
            }

            lines.Add(new CodeLine("};"));

            return new MethodDefinition(table.GetEntityName(), "ToEntity", new ParameterDefinition(table.GetViewModelName(), "viewModel"))
            {
                IsStatic = true,
                IsExtension = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetToViewModelMethod(ITable table)
        {
            var lines = new List<ILine>();

            lines.Add(new CodeLine("return new {0}", table.GetViewModelName()));
            lines.Add(new CodeLine("{"));

            for (var i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];

                lines.Add(new CodeLine(1, "{0} = entity.{0}{1}", column.GetPropertyName(), i < table.Columns.Count - 1 ? "," : string.Empty));
            }

            lines.Add(new CodeLine("};"));

            return new MethodDefinition(table.GetViewModelName(), "ToViewModel", new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsStatic = true,
                IsExtension = true,
                Lines = lines
            };
        }

        internal static void GenerateViewModelsExtensions(this EfCoreProject project, AspNetCoreProjectSettings settings)
        {
            foreach (var table in project.Database.Tables)
            {
                var classDefinition = new CSharpClassDefinition
                {
                    Namespaces = new List<string>()
                    {
                        "System"
                    },
                    Namespace = project.GetViewModelsNamespace(),
                    Name = table.GetViewModelExtensionName(),
                    IsStatic = true
                };

                if (table.HasDefaultSchema())
                {
                    classDefinition.Namespaces.AddUnique(project.GetEntityLayerNamespace());
                }
                else
                {
                    classDefinition.Namespaces.AddUnique(project.GetEntityLayerNamespace(table.Schema));
                }

                classDefinition.Methods.Add(GetToEntityMethod(table));
                classDefinition.Methods.Add(GetToViewModelMethod(table));

                CSharpClassBuilder.Create(settings.OutputDirectory, "ViewModels", project.Settings.ForceOverwrite, classDefinition);
            }
        }

        public static EfCoreProject GenerateAspNetCoreProject(this EfCoreProject project, AspNetCoreProjectSettings settings)
        {
            project.GenerateResponses(settings);
            project.GenerateResponsesExtensions(settings);
            project.GenerateViewModels(settings);
            project.GenerateViewModelsExtensions(settings);

            foreach (var feature in project.Features)
            {
                CSharpClassBuilder.Create(settings.OutputDirectory, "Controllers", feature.GetEfCoreProject().Settings.ForceOverwrite, feature.GetControllerClassDefinition());
            }

            return project;
        }
    }
}
