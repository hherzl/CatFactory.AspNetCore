using System.Collections.Generic;
using CatFactory.DotNetCore;
using CatFactory.EfCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public static class SingleResponseClassDefinition
    {
        public static CSharpClassDefinition GetSingleResponseClassDefinition(this EntityFrameworkCoreProject project)
        {
            var definition = new CSharpClassDefinition();

            definition.Namespaces.Add("System");
            definition.Namespace = project.GetResponsesNamespace();
            definition.Name = "SingleResponse";

            definition.GenericTypes = new List<GenericTypeDefinition>
            {
                new GenericTypeDefinition { Name = "TModel", Constraint = "TModel : class" }
            };

            definition.Implements.Add("ISingleResponse<TModel>");
            definition.Properties.Add(new PropertyDefinition("String", "Message"));
            definition.Properties.Add(new PropertyDefinition("Boolean", "DidError"));
            definition.Properties.Add(new PropertyDefinition("String", "ErrorMessage"));
            definition.Properties.Add(new PropertyDefinition("TModel", "Model"));

            return definition;
        }
    }
}
