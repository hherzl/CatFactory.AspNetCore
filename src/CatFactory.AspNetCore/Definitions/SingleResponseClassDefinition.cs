using CatFactory.DotNetCore;
using CatFactory.EfCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public static class SingleResponseClassDefinition
    {
        public static CSharpClassDefinition GetSingleResponseClassDefinition(this EfCoreProject project)
        {
            var definition = new CSharpClassDefinition();

            definition.Namespaces.Add("System");
            definition.Namespace = project.GetResponsesNamespace();
            definition.Name = "SingleResponse";
            definition.GenericType = "TModel";
            definition.Implements.Add("ISingleResponse<TModel>");
            definition.Properties.Add(new PropertyDefinition("String", "Message"));
            definition.Properties.Add(new PropertyDefinition("Boolean", "DidError"));
            definition.Properties.Add(new PropertyDefinition("String", "ErrorMessage"));
            definition.Properties.Add(new PropertyDefinition("TModel", "Model"));

            return definition;
        }
    }
}
