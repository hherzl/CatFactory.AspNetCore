using System;
using CatFactory.AspNetCore.Definitions;
using CatFactory.DotNetCore;
using CatFactory.EfCore;

namespace CatFactory.AspNetCore
{
    public static class AspNetCoreProjectExtensions
    {
        public static String GetResponsesNamespace(this AspNetCoreProject project)
            => String.Format("{0}.{1}", project.Name, "Responses");

        private static void GenerateResponses(this AspNetCoreProject project)
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
                    OutputDirectory = "C:\\Temp\\CatFactory.AspNetCore\\Store.AspNetCore\\src\\Store.AspNetCore",
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
                    OutputDirectory = "C:\\Temp\\CatFactory.AspNetCore\\Store.AspNetCore\\src\\Store.AspNetCore",
                    ObjectDefinition = definition
                };

                codeBuilder.CreateFile(subdirectory: "Responses");
            }
        }

        public static AspNetCoreProject GenerateAspNetCoreProject(this EfCoreProject project)
        {
            var proj = new AspNetCoreProject
            {
                Name = project.Name
            };

            proj.GenerateResponses();

            foreach (var feature in proj.Features)
            {
                var classDefinition = new ControllerClassDefinition(feature);

                var codeBuilder = new CSharpClassBuilder
                {
                    OutputDirectory = "C:\\Temp\\CatFactory.AspNetCore\\Store.AspNetCore\\src\\Store.AspNetCore",
                    ObjectDefinition = classDefinition
                };

                codeBuilder.CreateFile(subdirectory: "Controllers");
            }

            return proj;
        }
    }
}
