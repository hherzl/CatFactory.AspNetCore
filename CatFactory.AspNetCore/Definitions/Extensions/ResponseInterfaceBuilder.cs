using CatFactory.ObjectOrientedProgramming;

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
                AccessModifier = AccessModifier.Public,
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
