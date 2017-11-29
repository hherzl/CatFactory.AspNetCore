using System.Collections.Generic;
using CatFactory.DotNetCore;
using CatFactory.EfCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public static class ListResponseClassDefinition
    {
        public static CSharpClassDefinition GetListResponseClassDefinition(this AspNetCoreProjectSettings settings)
        {
            var definition = new CSharpClassDefinition();

            definition.Namespaces.Add("System");
            definition.Namespaces.Add("System.Collections.Generic");
            definition.Namespace = settings.GetResponsesNamespace();
            definition.Name = "ListResponse";

            definition.GenericTypes = new List<GenericTypeDefinition>
            {
                new GenericTypeDefinition { Name = "TModel", Constraint = "TModel : class" }
            };

            definition.Implements.Add("IListResponse<TModel>");

            definition.Properties.Add(new PropertyDefinition("String", "Message"));
            definition.Properties.Add(new PropertyDefinition("Boolean", "DidError"));
            definition.Properties.Add(new PropertyDefinition("String", "ErrorMessage"));
            definition.Properties.Add(new PropertyDefinition("IEnumerable<TModel>", "Model"));

            return definition;
        }
    }
}
