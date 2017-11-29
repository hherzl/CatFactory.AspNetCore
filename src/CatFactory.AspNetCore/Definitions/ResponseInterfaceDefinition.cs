using CatFactory.DotNetCore;
using CatFactory.EfCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public static class ResponseInterfaceDefinition
    {
        public static CSharpInterfaceDefinition GetResponseInterfaceDefinition(this EntityFrameworkCoreProject project, AspNetCoreProjectSettings settings)
        {
            var definition = new CSharpInterfaceDefinition();

            definition.Namespaces.Add("System");
            definition.Namespace = settings.GetResponsesNamespace();
            definition.Name = "IResponse";
            definition.Properties.Add(new PropertyDefinition("String", "Message"));
            definition.Properties.Add(new PropertyDefinition("Boolean", "DidError"));
            definition.Properties.Add(new PropertyDefinition("String", "ErrorMessage"));

            return definition;
        }
    }
}
