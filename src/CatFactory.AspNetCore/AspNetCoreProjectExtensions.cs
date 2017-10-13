using System;
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
    public static class AspNetCoreProjectExtensions
    {
        private static ClrTypeResolver resolver;

        static AspNetCoreProjectExtensions()
        {
            resolver = new ClrTypeResolver();
        }

        public static String GetResponsesNamespace(this EfCoreProject project)
            => String.Format("{0}.{1}", project.Name, "Responses");

        public static String GetViewModelsNamespace(this EfCoreProject project)
            => String.Format("{0}.{1}", project.Name, "ViewModels");

        internal static void GenerateResponses(this EfCoreProject project, AspNetCoreProjectSettings settings)
        {
            var interfaceDefinitions = new CSharpInterfaceDefinition[]
            {
                new ResponseInterfaceDefinition(),
                new SingleResponseInterfaceDefinition(),
                new ListResponseInterfaceDefinition(),
                new PagedResponseInterfaceDefinition()
            };

            foreach (var definition in interfaceDefinitions)
            {
                definition.Namespace = project.GetResponsesNamespace();

                var codeBuilder = new CSharpInterfaceBuilder
                {
                    OutputDirectory = settings.OutputDirectory,
                    ObjectDefinition = definition
                };

                codeBuilder.CreateFile(subdirectory: "Responses");
            }

            var classDefinitions = new CSharpClassDefinition[]
            {
                new SingleResponseClassDefinition(),
                new ListResponseClassDefinition(),
                new PagedResponseClassDefinition()
            };

            foreach (var definition in classDefinitions)
            {
                definition.Namespace = project.GetResponsesNamespace();

                var codeBuilder = new CSharpClassBuilder
                {
                    OutputDirectory = settings.OutputDirectory,
                    ObjectDefinition = definition
                };

                codeBuilder.CreateFile(subdirectory: "Responses");
            }
        }

        internal static void GenerateResponsesExtensions(this EfCoreProject project, AspNetCoreProjectSettings settings)
        {
            var definition = new CSharpClassDefinition
            {
                Namespaces = new List<String>()
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

            definition.Methods.Add(new MethodDefinition("void", "SetError", new ParameterDefinition("IResponse", "response"), new ParameterDefinition("Exception", "ex"), new ParameterDefinition("ILogger", "logger"))
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

            definition.Methods.Add(new MethodDefinition("IActionResult ", "ToHttpResponse", new ParameterDefinition("IResponse", "response"))
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

            var codeBuilder = new CSharpClassBuilder
            {
                OutputDirectory = settings.OutputDirectory,
                ObjectDefinition = definition
            };

            codeBuilder.CreateFile(subdirectory: "Responses");
        }

        internal static void GenerateViewModels(this EfCoreProject project, AspNetCoreProjectSettings settings)
        {
            foreach (var table in project.Database.Tables)
            {
                var viewModelClassDefinition = new CSharpClassDefinition
                {
                    Namespaces = new List<String>()
                    {
                        "System"
                    },
                    Namespace = project.GetViewModelsNamespace(),
                    Name = table.GetViewModelName()
                };

                foreach (var column in table.Columns)
                {
                    viewModelClassDefinition.Properties.Add(new PropertyDefinition(resolver.Resolve(column.Type), column.GetPropertyName()));
                }

                var codeBuilder = new CSharpClassBuilder
                {
                    OutputDirectory = settings.OutputDirectory,
                    ObjectDefinition = viewModelClassDefinition
                };

                codeBuilder.CreateFile(subdirectory: "ViewModels");
            }
        }

        internal static void GenerateViewModelsExtensions(this EfCoreProject project, AspNetCoreProjectSettings settings)
        {
            foreach (var table in project.Database.Tables)
            {
                var viewModelClassExtensionDefinition = new CSharpClassDefinition
                {
                    Namespaces = new List<String>()
                    {
                        "System"
                    },
                    Namespace = project.GetViewModelsNamespace(),
                    Name = table.GetViewModelExtensionName(),
                    IsStatic = true
                };

                if (CatFactory.EfCore.DbObjectsExtensions.HasDefaultSchema(table))
                {
                    viewModelClassExtensionDefinition.Namespaces.AddUnique(project.GetEntityLayerNamespace());
                }
                else
                {
                    viewModelClassExtensionDefinition.Namespaces.AddUnique(project.GetEntityLayerNamespace(table.Schema));
                }

                viewModelClassExtensionDefinition.Methods.Add(GetToEntityMethod(table));
                viewModelClassExtensionDefinition.Methods.Add(GetToViewModelMethod(table));

                var codeBuilder = new CSharpClassBuilder
                {
                    OutputDirectory = settings.OutputDirectory,
                    ObjectDefinition = viewModelClassExtensionDefinition
                };

                codeBuilder.CreateFile(subdirectory: "ViewModels");
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

                lines.Add(new CodeLine(1, "{0} = viewModel.{0}{1}", column.GetPropertyName(), i < table.Columns.Count - 1 ? "," : String.Empty));
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

                lines.Add(new CodeLine(1, "{0} = entity.{0}{1}", column.GetPropertyName(), i < table.Columns.Count - 1 ? "," : String.Empty));
            }

            lines.Add(new CodeLine("};"));

            return new MethodDefinition(table.GetViewModelName(), "ToViewModel", new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsStatic = true,
                IsExtension = true,
                Lines = lines
            };
        }
    }
}
