using CatFactory.CodeFactory;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions.Extensions
{
    public static class ResponsesExtensionClassBuilder
    {
        public static ResponsesExtensionClassDefinition GetResponsesExtensionsClassDefinition(this AspNetCoreProject project)
        {
            var definition = new ResponsesExtensionClassDefinition
            {
                Namespaces =
                {
                    "System",
                    "System.Net",
                    "Microsoft.AspNetCore.Mvc",
                    "Microsoft.Extensions.Logging"
                },
                Namespace = project.GetResponsesNamespace(),
                IsStatic = true,
                Name = "ResponsesExtensions"
            };

            definition.Methods.Add(new MethodDefinition("void", "SetError", new ParameterDefinition("IResponse", "response"), new ParameterDefinition("ILogger", "logger"), new ParameterDefinition("Exception", "ex"))
            {
                IsStatic = true,
                IsExtension = true,
                Lines =
                {
                    new CodeLine("response.DidError = true;"),
                    new CodeLine("response.ErrorMessage = ex.Message;"),
                    new CodeLine(),
                    new TodoLine("Add additional logic to save exception"),
                    new CodeLine("logger?.LogCritical(ex.ToString());")
                }
            });

            definition.Methods.Add(new MethodDefinition("IActionResult ", "ToHttpResponse", new ParameterDefinition("ISingleResponse<TModel>", "response"))
            {
                IsStatic = true,
                IsExtension = true,
                GenericTypes =
                {
                    new GenericTypeDefinition
                    {
                        Name = "TModel",
                        Constraint = "TModel : class"
                    }
                },
                Lines =
                {
                    new CodeLine("var status = HttpStatusCode.OK;"),
                    new CodeLine(),
                    new CodeLine("if (response.Model == null)"),
                    new CodeLine(1, "status = HttpStatusCode.NotFound;"),
                    new CodeLine(),
                    new CodeLine("if (response.DidError)"),
                    new CodeLine(1, "status = HttpStatusCode.InternalServerError;"),
                    new CodeLine(),
                    new CodeLine("return new ObjectResult(response)"),
                    new CodeLine("{"),
                    new CodeLine(1, "StatusCode = (int)status"),
                    new CodeLine("};")
                }
            });

            definition.Methods.Add(new MethodDefinition("IActionResult ", "ToHttpResponse", new ParameterDefinition("IListResponse<TModel>", "response"))
            {
                IsStatic = true,
                IsExtension = true,
                GenericTypes =
                {
                    new GenericTypeDefinition
                    {
                        Name = "TModel",
                        Constraint = "TModel : class"
                    }
                },
                Lines =
                {
                    new CodeLine("var status = HttpStatusCode.OK;"),
                    new CodeLine(),
                    new CodeLine("if (response.Model == null)"),
                    new CodeLine(1, "status = HttpStatusCode.NoContent;"),
                    new CodeLine(),
                    new CodeLine("if (response.DidError)"),
                    new CodeLine(1, "status = HttpStatusCode.InternalServerError;"),
                    new CodeLine(),
                    new CodeLine("return new ObjectResult(response)"),
                    new CodeLine("{"),
                    new CodeLine(1, "StatusCode = (int)status"),
                    new CodeLine("};")
                }
            });

            return definition;
        }
    }
}
