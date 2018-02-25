using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public static class ResponseInterfaceDefinition
    {
        public static CSharpInterfaceDefinition GetResponseInterfaceDefinition(this AspNetCoreProject project)
        {
            var definition = new CSharpInterfaceDefinition();

            definition.Namespaces.Add("System");
            definition.Namespace = project.GetResponsesNamespace();
            definition.Name = "IResponse";
            definition.Properties.Add(new PropertyDefinition("String", "Message"));
            definition.Properties.Add(new PropertyDefinition("Boolean", "DidError"));
            definition.Properties.Add(new PropertyDefinition("String", "ErrorMessage"));

            return definition;
        }
    }
}
