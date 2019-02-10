using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.AspNetCore.Definitions.Extensions
{
    public static class ResponseClassBuilder
    {
        public static ResponseClassDefinition GetResponseClassDefinition(this AspNetCoreProject project)
            => new ResponseClassDefinition
            {
                Namespaces =
                {
                    "System"
                },
                Namespace = project.GetResponsesNamespace(),
                AccessModifier = AccessModifier.Public,
                Name = "Response",
                Implements =
                {
                    "IResponse"
                },
                Properties =
                {
                    new PropertyDefinition(AccessModifier.Public, "string", "Message") { IsAutomatic = true },
                    new PropertyDefinition(AccessModifier.Public, "bool", "DidError") { IsAutomatic = true },
                    new PropertyDefinition(AccessModifier.Public, "string", "ErrorMessage") { IsAutomatic = true }
                }
            };
    }
}
