using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions.Extensions
{
    public static class ResponseInterfaceBuilder
    {
        public static ResponseInterfaceDefinition GetResponseInterfaceDefinition(this AspNetCoreProject project)
            => new ResponseInterfaceDefinition
            {
                Namespaces =
                {
                    "System"
                },
                Namespace = project.GetResponsesNamespace(),
                Name = "IResponse",
                Properties =
                {
                    new PropertyDefinition("String", "Message"),
                    new PropertyDefinition("Boolean", "DidError"),
                    new PropertyDefinition("String", "ErrorMessage")
                }
            };
    }
}
