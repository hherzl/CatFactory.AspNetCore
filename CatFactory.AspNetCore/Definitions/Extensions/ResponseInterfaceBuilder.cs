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
                    new PropertyDefinition("string", "Message"),
                    new PropertyDefinition("bool", "DidError"),
                    new PropertyDefinition("string", "ErrorMessage")
                }
            };
    }
}
