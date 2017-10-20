using CatFactory.DotNetCore;
using CatFactory.EfCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public static class ListResponseInterfaceDefinition
    {
        public static CSharpInterfaceDefinition GetListResponseInterfaceDefinition(this EfCoreProject project)
        {
            var definition = new CSharpInterfaceDefinition();

            definition.Namespaces.Add("System.Collections.Generic");
            definition.Namespace = project.GetResponsesNamespace();
            definition.Name = "IListResponse";
            definition.GenericType = "TModel";
            definition.Implements.Add("IResponse");
            definition.Properties.Add(new PropertyDefinition("IEnumerable<TModel>", "Model"));

            return definition;
        }
    }
}
