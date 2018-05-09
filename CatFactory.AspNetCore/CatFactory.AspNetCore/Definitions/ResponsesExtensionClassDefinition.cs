using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.NetCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public static class ResponsesExtensionClassDefinition
    {
        public static CSharpClassDefinition GetResponsesExtensionsClassDefinition(this AspNetCoreProject project)
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
                    new TodoLine("Add logic to save exception in file"),
                    new CodeLine("logger?.LogError(ex.ToString());")
                }
            });

            classDefinition.Methods.Add(new MethodDefinition("IActionResult ", "ToHttpResponse", new ParameterDefinition("ISingleResponse<TModel>", "response"))
            {
                IsStatic = true,
                IsExtension = true,
                GenericTypes = new List<GenericTypeDefinition>
                {
                    new GenericTypeDefinition { Name = "TModel", Constraint = "TModel : class" }
                },
                Lines = new List<ILine>
                {
                    new CodeLine("var status = HttpStatusCode.OK;"),
                    new CodeLine(),
                    new CodeLine("if (response.Model == null)"),
                    new CodeLine("{"),
                    new CodeLine(1, "status = HttpStatusCode.NotFound;"),
                    new CodeLine("}"),
                    new CodeLine(),
                    new CodeLine("if (response.DidError)"),
                    new CodeLine("{"),
                    new CodeLine(1, "status = HttpStatusCode.InternalServerError;"),
                    new CodeLine("}"),
                    new CodeLine(),
                    new CodeLine("return new ObjectResult(response)"),
                    new CodeLine("{"),
                    new CodeLine(1, "StatusCode = (Int32)status"),
                    new CodeLine("};")
                }
            });

            classDefinition.Methods.Add(new MethodDefinition("IActionResult ", "ToHttpResponse", new ParameterDefinition("IListResponse<TModel>", "response"))
            {
                IsStatic = true,
                IsExtension = true,
                GenericTypes = new List<GenericTypeDefinition>
                {
                    new GenericTypeDefinition { Name = "TModel", Constraint = "TModel : class" }
                },
                Lines = new List<ILine>
                {
                    new CodeLine("var status = HttpStatusCode.OK;"),
                    new CodeLine(),
                    new CodeLine("if (response.Model == null)"),
                    new CodeLine("{"),
                    new CodeLine(1, "status = HttpStatusCode.NoContent;"),
                    new CodeLine("}"),
                    new CodeLine(),
                    new CodeLine("if (response.DidError)"),
                    new CodeLine("{"),
                    new CodeLine(1, "status = HttpStatusCode.InternalServerError;"),
                    new CodeLine("}"),
                    new CodeLine(),
                    new CodeLine("return new ObjectResult(response)"),
                    new CodeLine("{"),
                    new CodeLine(1, "StatusCode = (Int32)status"),
                    new CodeLine("};")
                }
            });

            return classDefinition;
        }
    }
}
