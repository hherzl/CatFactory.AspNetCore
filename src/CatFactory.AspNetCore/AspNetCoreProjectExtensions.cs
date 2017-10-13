using System;
using System.Collections.Generic;
using CatFactory.AspNetCore.Definitions;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.DotNetCore;
using CatFactory.EfCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore
{
    public static class AspNetCoreProjectExtensions
    {
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
                IsExtension = true
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
            var resolver = new ClrTypeResolver();

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

                if (table.HasDefaultSchema())
                {
                    viewModelClassExtensionDefinition.Namespaces.AddUnique(project.GetEntityLayerNamespace());
                }
                else
                {
                    viewModelClassExtensionDefinition.Namespaces.AddUnique(project.GetEntityLayerNamespace(table.Schema));
                }

                viewModelClassExtensionDefinition.Methods.Add(new MethodDefinition(table.GetEntityName(), "ToEntity", new ParameterDefinition(table.GetViewModelName(), "viewModel"))
                {
                    IsStatic = true,
                    IsExtension = true,
                    Lines = new List<ILine>()
                    {
                        new CodeLine("return new {0}();", table.GetEntityName())
                    }
                });

                viewModelClassExtensionDefinition.Methods.Add(new MethodDefinition(table.GetViewModelName(), "ToViewModel", new ParameterDefinition(table.GetEntityName(), "entity"))
                {
                    IsStatic = true,
                    IsExtension = true,
                    Lines = new List<ILine>()
                    {
                        new CodeLine("return new {0}();", table.GetViewModelName())
                    }
                });

                var codeBuilder = new CSharpClassBuilder
                {
                    OutputDirectory = settings.OutputDirectory,
                    ObjectDefinition = viewModelClassExtensionDefinition
                };

                codeBuilder.CreateFile(subdirectory: "ViewModels");
            }
        }
    }
}
