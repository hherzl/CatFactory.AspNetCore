using CatFactory.DotNetCore;
using CatFactory.EfCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public static class SingleResponseInterfaceDefinition
    {
        public static CSharpInterfaceDefinition GetSingleResponseInterfaceDefinition(this EfCoreProject project)
        {
            var definition = new CSharpInterfaceDefinition();

            definition.Namespace = project.GetResponsesNamespace();
            definition.Name = "ISingleResponse";
            definition.GenericType = "TModel";
            definition.Implements.Add("IResponse");
            definition.Properties.Add(new PropertyDefinition("TModel", "Model"));

            return definition;
        }
    }
}
